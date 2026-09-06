using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;

// Converts text into speech using the Piper engine and plays it
// through the system's default audio output.
// Sentences are queued and played sequentially, so the caller
// never blocks waiting for playback to finish.
public class SpeechReader : IDisposable
{
    public static SpeechReader Instance { get; } = new SpeechReader();

    // Raised when a sentence starts playing, and with null when
    // playback stops. Fired from a background thread.
    public event Action<string?>? SentenceChanged;

    private readonly string _piperPath;
    private readonly string _modelPath;

    // Sentences not yet spoken, kept so they can be regenerated
    // if the speed changes. The first one is the sentence playing now.
    private readonly List<string> _pendingSentences = new();
    private readonly object _pendingLock = new();

    // Playback speed. Below 1.0 is faster, above 1.0 is slower.
    private float _lengthScale = 1.2f;

    // The playback process currently running, if any.
    private Process? _currentPlayback;
    private readonly object _playbackLock = new();

    // Identifies the current playback session.
    // Incremented whenever queued work must be discarded.
    private int _sessionId;

    // Blocks the playback loop while paused. Starts in the paused state.
    private readonly ManualResetEventSlim _playGate = new(false);

    // True while audio is allowed to play.
    public bool IsPlaying => _playGate.IsSet;

    // Sentences waiting to be converted into audio.
    private readonly BlockingCollection<(int Session, string Text)> _textQueue = new();

    // Audio files already generated, waiting to be played.
    private readonly BlockingCollection<(int Session, string Text, string File)> _audioQueue = new(2);

    // Used to stop the background loops when the application closes.
    private readonly CancellationTokenSource _cancellation = new();

    // Locates the required files, verifies they exist and starts
    // the background loops that generate and play the queued sentences.
    private SpeechReader()
    {
        string basePath = Path.Combine(AppContext.BaseDirectory, "resources");

        // On windows, executable name is piper.exe, on UNIX-like, only piper
        _piperPath = OperatingSystem.IsWindows() ? Path.Combine(basePath, "piper", "piper.exe") : Path.Combine(basePath, "piper", "piper");
        _modelPath = Path.Combine(basePath, "voices", "en_US-lessac-medium.onnx");

        if (!File.Exists(_piperPath))
            throw new FileNotFoundException(
                "Piper engine not found. Run ./install-resources.sh", _piperPath);

        if (!File.Exists(_modelPath))
            throw new FileNotFoundException(
                "Voice model not found. Run ./install-resources.sh", _modelPath);

        Task.Run(ProcessTextQueue);
        Task.Run(ProcessAudioQueue);
    }

    // Queues a sentence to be spoken. Returns immediately.
    public void Speak(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence)) return;

        string trimmed = sentence.Trim();

        lock (_pendingLock)
        {
            _pendingSentences.Add(trimmed);
        }

        _textQueue.Add((Volatile.Read(ref _sessionId), trimmed));
    }

    // Queues the contents of a text file to be spoken. Returns immediately.
    public void SpeakFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Text file not found", filePath);

        Speak(File.ReadAllText(filePath));
    }

    // Allows the playback loop to continue.
    public void Play()
    {
        _playGate.Set();
    }

    // Pauses playback immediately, cutting the current sentence short.
    // That sentence is replayed from the beginning when Play() is called.
    public void Pause()
    {
        _playGate.Reset();
        KillPlayback();
        SentenceChanged?.Invoke(null);
    }

    // Stops everything and forgets every pending sentence.
    public void Stop()
    {
        Interlocked.Increment(ref _sessionId);

        lock (_pendingLock)
        {
            _pendingSentences.Clear();
        }

        DrainQueues();
        KillPlayback();
        SentenceChanged?.Invoke(null);
    }

    // Sets the speaking speed and regenerates the sentences still
    // pending, so the change is heard straight away.
    public void SetSpeed(float lengthScale)
    {
        float newScale = Math.Clamp(lengthScale, 0.4f, 2.6f);

        if (Math.Abs(newScale - _lengthScale) < 0.01f) return;

        _lengthScale = newScale;

        // Invalidate the audio generated at the old speed.
        Interlocked.Increment(ref _sessionId);

        DrainQueues();
        KillPlayback();

        // Requeue the same sentences under the new session.
        List<string> remaining;

        lock (_pendingLock)
        {
            remaining = new List<string>(_pendingSentences);
        }

        int session = Volatile.Read(ref _sessionId);

        foreach (string sentence in remaining)
            _textQueue.Add((session, sentence));
    }

    // Background loop: converts queued sentences into audio files.
    // Work belonging to an earlier session is discarded.
    private void ProcessTextQueue()
    {
        foreach (var item in _textQueue.GetConsumingEnumerable(_cancellation.Token))
        {
            if (item.Session != Volatile.Read(ref _sessionId))
                continue;

            string tempFile = Path.Combine(Path.GetTempPath(), $"speech_{Guid.NewGuid()}.wav");

            try
            {
                GenerateAudio(item.Text, tempFile);

                // Check again: the session may have changed while generating.
                if (item.Session != Volatile.Read(ref _sessionId))
                {
                    File.Delete(tempFile);
                    continue;
                }

                _audioQueue.Add((item.Session, item.Text, tempFile), _cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Speech generation error: " + ex.Message);

                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }

    // Background loop: plays the generated audio files one after another.
    // A sentence cut short by Pause() is replayed once playback resumes.
    private async Task ProcessAudioQueue()
    {
        try
        {
            foreach (var item in _audioQueue.GetConsumingEnumerable(_cancellation.Token))
            {
                bool finished = false;

                while (!finished)
                {
                    _playGate.Wait(_cancellation.Token);

                    // The session changed: this audio is no longer valid.
                    if (item.Session != Volatile.Read(ref _sessionId))
                        break;

                    try
                    {
                        SentenceChanged?.Invoke(item.Text);
                        await PlayAudio(item.File);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Speech playback error: " + ex.Message);
                        break;
                    }

                    // If the gate is still open, playback ended naturally.
                    // If it is closed, Pause() cut it short: wait and replay.
                    finished = _playGate.IsSet;
                }

                if (finished)
                {
                    // Nothing is being read now.
                    SentenceChanged?.Invoke(null);

                    // Remove it from the pending list only if it was spoken.
                    if (item.Session == Volatile.Read(ref _sessionId))
                    {
                        lock (_pendingLock)
                        {
                            if (_pendingSentences.Count > 0)
                                _pendingSentences.RemoveAt(0);
                        }
                    }
                }

                if (File.Exists(item.File))
                    File.Delete(item.File);
            }
        }
        catch (OperationCanceledException)
        {
            // The application is closing.
        }
    }

    // Runs the Piper engine to turn a sentence into a WAV file.
    private void GenerateAudio(string text, string outputFile)
    {
        var info = new ProcessStartInfo
        {
            FileName = _piperPath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true
        };

        info.ArgumentList.Add("--model");
        info.ArgumentList.Add(_modelPath);
        info.ArgumentList.Add("--length_scale");
        info.ArgumentList.Add(_lengthScale.ToString(CultureInfo.InvariantCulture));
        info.ArgumentList.Add("--output_file");
        info.ArgumentList.Add(outputFile);

        using Process process = Process.Start(info)
            ?? throw new Exception("Could not start the Piper engine.");

        process.StandardInput.Write(text);
        process.StandardInput.Close();

        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception("Audio generation failed: " + error);
    }

    // Plays a WAV file through the system's default audio output.
    // The running process is stored so playback can be stopped on demand.
    private async Task PlayAudio(string filePath)
    {
        if (OperatingSystem.IsWindows())
        {
            await Task.Run(() =>
            {
                if (!OperatingSystem.IsWindows()) return;
                using var player = new System.Media.SoundPlayer(filePath);
            });

            return;
        }

        string executable;
        if (OperatingSystem.IsLinux())
        {
            executable = "aplay";
        }
        else if (OperatingSystem.IsMacOS())
        {
            executable = "afplay";
        }
        else
        {
            throw new PlatformNotSupportedException($"Audio playback not supported on platform: {RuntimeInformation.OSDescription}");
        }

        var info = new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = false,
            RedirectStandardError = true
        };
        
        // -q is quiet mode, only available in linux aplay, but not in macos afplay
        if (OperatingSystem.IsLinux()) info.ArgumentList.Add("-q");
        info.ArgumentList.Add(filePath);

        Process process = Process.Start(info)
            ?? throw new Exception("Could not start audio playback.");

        lock (_playbackLock)
        {
            _currentPlayback = process;
        }

        try
        {
            process.StandardError.ReadToEnd();
            process.WaitForExit();
        }
        finally
        {
            lock (_playbackLock)
            {
                _currentPlayback = null;
            }

            process.Dispose();
        }
    }

    // Empties both queues, deleting any audio file already generated.
    private void DrainQueues()
    {
        while (_textQueue.TryTake(out _)) { }

        while (_audioQueue.TryTake(out var item))
        {
            if (File.Exists(item.File))
                File.Delete(item.File);
        }
    }

    // Stops the sentence being played right now, if any.
    private void KillPlayback()
    {
        lock (_playbackLock)
        {
            try
            {
                if (_currentPlayback is not null && !_currentPlayback.HasExited)
                    _currentPlayback.Kill();
            }
            catch (InvalidOperationException)
            {
                // The process had already finished. Nothing to do.
            }
        }
    }

    // Stops the background loops and releases resources.
    public void Dispose()
    {
        Stop();

        _cancellation.Cancel();
        _playGate.Set();          // release the playback loop so it can exit

        _textQueue.CompleteAdding();
        _audioQueue.CompleteAdding();

        _cancellation.Dispose();
        _textQueue.Dispose();
        _audioQueue.Dispose();
        _playGate.Dispose();
    }
}