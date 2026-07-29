using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

// Converts text into speech using the Piper engine and plays it
// through the system's default audio output.
// Sentences are queued and played sequentially, so the caller
// never blocks waiting for playback to finish.
public class SpeechReader : IDisposable
{
    public static SpeechReader Instance { get; } = new SpeechReader();

    private readonly string _piperPath;
    private readonly string _modelPath;

    // Playback speed. Below 1.0 is faster, above 1.0 is slower.
    private readonly float _lengthScale = 1.3f;

    // The playback process currently running, if any.
    private Process? _currentPlayback;
    private readonly object _playbackLock = new();

    // Identifies the current playback session.
    // Incremented on Stop() so that work started earlier is discarded.
    private int _sessionId;

    // Sentences waiting to be converted into audio.
    private readonly BlockingCollection<(int Session, string Text)> _textQueue = new();

    // Audio files already generated, waiting to be played.
    private readonly BlockingCollection<(int Session, string File)> _audioQueue = new(2);

    // Used to stop the background loop when the application closes.
    private readonly CancellationTokenSource _cancellation = new();

    // Locates the required files, verifies they exist and starts
    // the background loop that will play the queued sentences.
    private SpeechReader()
    {
        string basePath = Path.Combine(AppContext.BaseDirectory, "resources");

        _piperPath = Path.Combine(basePath, "piper", "piper");
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

        _textQueue.Add((Volatile.Read(ref _sessionId), sentence.Trim()));
    }

    // Queues the contents of a text file to be spoken. Returns immediately.
    public void SpeakFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Text file not found", filePath);

        Speak(File.ReadAllText(filePath));
    }

    // Background loop: converts queued sentences into audio files.
    // Work belonging to a previous session is discarded.
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

                // Check again: Stop() may have been called while generating.
                if (item.Session != Volatile.Read(ref _sessionId))
                {
                    File.Delete(tempFile);
                    continue;
                }

                _audioQueue.Add((item.Session, tempFile), _cancellation.Token);
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
    private void ProcessAudioQueue()
    {
        foreach (var item in _audioQueue.GetConsumingEnumerable(_cancellation.Token))
        {
            try
            {
                if (item.Session == Volatile.Read(ref _sessionId))
                    PlayAudio(item.File);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Speech playback error: " + ex.Message);
            }
            finally
            {
                if (File.Exists(item.File))
                    File.Delete(item.File);
            }
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
    private void PlayAudio(string filePath)
    {
        var info = new ProcessStartInfo
        {
            FileName = "aplay",
            UseShellExecute = false,
            RedirectStandardError = true
        };

        info.ArgumentList.Add("-q");
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

    // Stops the background loop and releases resources.
    public void Dispose()
    {
        Stop();

        _cancellation.Cancel();
        _textQueue.CompleteAdding();
        _audioQueue.CompleteAdding();
        _cancellation.Dispose();
        _textQueue.Dispose();
        _audioQueue.Dispose();
    }

    // Stops the current sentence and discards everything still queued,
    // including work that is being generated right now.
    public void Stop()
    {
        // Invalidate everything belonging to the previous session.
        Interlocked.Increment(ref _sessionId);

        // Discard pending sentences.
        while (_textQueue.TryTake(out _)) { }

        // Discard audio files already generated but not played.
        while (_audioQueue.TryTake(out var item))
        {
            if (File.Exists(item.File))
                File.Delete(item.File);
        }

        // Kill the sentence being played right now.
        lock (_playbackLock)
        {
            try
            {
                if (_currentPlayback is not null && !_currentPlayback.HasExited)
                    _currentPlayback.Kill();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}