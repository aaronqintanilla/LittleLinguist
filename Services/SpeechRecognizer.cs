using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Vosk;

namespace LittleLinguist.Services;

// Handles speech recognition using Vosk and FFmpeg.
public class SpeechRecognizer
{
    private readonly Model model;

    // Loads the speech recognition model from the specified path.
    public SpeechRecognizer(string modelPath)
    {
        model = new Model(modelPath);
    }

    // Records audio and recognizes whether the target word was spoken.
    public async Task<string> RecognizeAsync(string targetWord)
    {
        string normalizedTarget =
            targetWord.Trim().ToLowerInvariant();

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",

            Arguments =
                "-loglevel error " +
                "-f pulse " +
                "-i default " +
                "-t 4 " +
                "-ar 16000 " +
                "-ac 1 " +
                "-f s16le " +
                "-",

            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        Console.WriteLine(
            $"Listening for: {normalizedTarget}"
        );

        process.Start();

        Task<string> errorTask =
            process.StandardError.ReadToEndAsync();

        string grammar = JsonSerializer.Serialize(
            new[]
            {
                normalizedTarget,
                "[unk]"
            }
        );

        using var recognizer =
            new VoskRecognizer(
                model,
                16000.0f,
                grammar
            );

        byte[] buffer = new byte[4096];

        Stream output =
            process.StandardOutput.BaseStream;

        while (true)
        {
            int bytesRead =
                await output.ReadAsync(
                    buffer,
                    0,
                    buffer.Length
                );

            if (bytesRead <= 0)
            {
                break;
            }

            recognizer.AcceptWaveform(
                buffer,
                bytesRead
            );
        }

        await process.WaitForExitAsync();

        string ffmpegError =
            await errorTask;

        if (process.ExitCode != 0)
        {
            Console.Error.WriteLine(
                "FFMPEG ERROR:"
            );

            Console.Error.WriteLine(
                ffmpegError
            );

            return "";
        }

        string result =
            recognizer.FinalResult();

        Console.WriteLine(
            $"Vosk raw result: {result}"
        );

        using JsonDocument json =
            JsonDocument.Parse(result);

        if (json.RootElement.TryGetProperty(
            "text",
            out JsonElement textElement))
        {
            string recognized =
                textElement.GetString() ?? "";

            Console.WriteLine(
                $"Recognized: '{recognized}'"
            );

            return recognized;
        }

        return "";
    }
}