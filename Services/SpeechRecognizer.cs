using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Vosk;

namespace LittleLinguist.Services;

public class SpeechRecognizer
{
    private readonly Model model;

    public SpeechRecognizer(string modelPath)
    {
        model = new Model(modelPath);
    }

    // public async Task<string> RecognizeAsync()
    // {
    //     var startInfo = new ProcessStartInfo
    //     {
    //         // FileName = "ffmpeg", // ÉN UN FUTURO LO VOLVEMOS A PONER ASI QUIZAS
    //         FileName = "/usr/bin/ffmpeg",

    //         Arguments =
    //             "-f avfoundation " +
    //             "-i \":0\" " +
    //             "-ar 16000 " +
    //             "-ac 1 " +
    //             "-f s16le " +
    //             "-",

    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         UseShellExecute = false,
    //         CreateNoWindow = true
    //     };

    //     using var process = new Process();
    //     process.StartInfo = startInfo;

    //     process.Start();

    //     using var recognizer = new VoskRecognizer(model, 16000.0f);

    //     byte[] buffer = new byte[4096];

    //     Stream output = process.StandardOutput.BaseStream;

    //     string finalText = "";

    //     // Escuchamos durante 5 segundos
    //     DateTime endTime = DateTime.Now.AddSeconds(5);

    //     while (DateTime.Now < endTime)
    //     {
    //         int bytesRead = await output.ReadAsync(
    //             buffer,
    //             0,
    //             buffer.Length
    //         );

    //         if (bytesRead <= 0)
    //         {
    //             break;
    //         }

    //         recognizer.AcceptWaveform(
    //             buffer,
    //             bytesRead
    //         );
    //     }

    //     // Terminamos FFmpeg
    //     try
    //     {
    //         if (!process.HasExited)
    //         {
    //             process.Kill();
    //         }
    //     }
    //     catch
    //     {
    //         // El proceso ya podía haber terminado.
    //     }

    //     // Obtenemos el resultado final de Vosk
    //     string result = recognizer.FinalResult();

    //     using JsonDocument json =
    //         JsonDocument.Parse(result);

    //     if (json.RootElement.TryGetProperty(
    //         "text",
    //         out JsonElement textElement))
    //     {
    //         finalText = textElement.GetString() ?? "";
    //     }

    //     return finalText;
    // }
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

        // Vosk solo espera la palabra objetivo o algo desconocido.
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
            // Miramos lo que Vosk está reconociendo mientras hablamos.
            string partialResult =
                recognizer.PartialResult();

            using JsonDocument partialJson =
                JsonDocument.Parse(partialResult);

            if (partialJson.RootElement.TryGetProperty(
                "partial",
                out JsonElement partialElement))
            {
                string partialText =
                    partialElement.GetString()?
                        .Trim()
                        .ToLowerInvariant()
                    ?? "";

                Console.WriteLine(
                    $"Partial: '{partialText}'"
                );

                // Si ya ha detectado la palabra correcta,
                // dejamos de escuchar inmediatamente.
                if (partialText == normalizedTarget)
                {
                    Console.WriteLine(
                        $"Target detected: '{normalizedTarget}'"
                    );

                    try
                    {
                        if (!process.HasExited)
                            process.Kill();
                    }
                    catch
                    {
                    }

                    return normalizedTarget;
                }
            }
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