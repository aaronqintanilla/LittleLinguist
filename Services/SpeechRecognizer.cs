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

    public async Task<string> RecognizeAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",

            Arguments =
                "-f avfoundation " +
                "-i \":0\" " +
                "-ar 16000 " +
                "-ac 1 " +
                "-f s16le " +
                "-",

            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = startInfo;

        process.Start();

        using var recognizer = new VoskRecognizer(model, 16000.0f);

        byte[] buffer = new byte[4096];

        Stream output = process.StandardOutput.BaseStream;

        string finalText = "";

        // Escuchamos durante 5 segundos
        DateTime endTime = DateTime.Now.AddSeconds(5);

        while (DateTime.Now < endTime)
        {
            int bytesRead = await output.ReadAsync(
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

        // Terminamos FFmpeg
        try
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        catch
        {
            // El proceso ya podía haber terminado.
        }

        // Obtenemos el resultado final de Vosk
        string result = recognizer.FinalResult();

        using JsonDocument json =
            JsonDocument.Parse(result);

        if (json.RootElement.TryGetProperty(
            "text",
            out JsonElement textElement))
        {
            finalText = textElement.GetString() ?? "";
        }

        return finalText;
    }

}