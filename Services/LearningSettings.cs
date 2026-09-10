using System.Text.Json;
using System;
using System.IO;

namespace LittleLinguist.Services;

// Stores the user's selected learning skills and manages their persistence.
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

    // Loads the saved settings or returns the default configuration.
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
            return new LearningSettings();
        }
    }

    // Saves the current settings to the configuration file.
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