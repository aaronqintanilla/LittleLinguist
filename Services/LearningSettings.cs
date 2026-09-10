using System.Text.Json;
using System;
using System.IO;

namespace LittleLinguist.Services;

public class LearningSettings
{
    public bool Pronunciation { get; set; } = true;
    public bool Writing { get; set; } = true;
    public bool Reading { get; set; } = true;
    public bool Listening { get; set; } = true;

    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "LittleLinguist");

    private static readonly string SettingsFile =
        Path.Combine(SettingsDirectory, "settings.json");

    public static LearningSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFile))
            {
                return new LearningSettings();
            }

            string json = File.ReadAllText(SettingsFile);

            return JsonSerializer.Deserialize<LearningSettings>(json)
                   ?? new LearningSettings();
        }
        catch
        {
            // Si el archivo está corrupto o no se puede leer,
            // usamos la configuración por defecto.
            return new LearningSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);

        string json = JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(SettingsFile, json);
    }
}