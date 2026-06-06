using System.Text;
using System.Text.Json;

namespace EasyBuilderRelease;

internal sealed class LastReleaseStore
{
    private const string FileName = "ultimo-release.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public LastReleaseStore(string baseDirectory)
    {
        SettingsFile = Path.Combine(baseDirectory, FileName);
    }

    public string SettingsFile { get; }

    public LastReleaseSettings Load()
    {
        if (!File.Exists(SettingsFile))
        {
            return new LastReleaseSettings();
        }

        var json = File.ReadAllText(SettingsFile, Encoding.UTF8);
        return JsonSerializer.Deserialize<LastReleaseSettings>(json, JsonOptions)
            ?? new LastReleaseSettings();
    }

    public void Save(LastReleaseSettings settings)
    {
        var directory = Path.GetDirectoryName(SettingsFile);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFile, json, new UTF8Encoding(false));
    }
}

internal sealed class LastReleaseSettings
{
    public DateTime SavedAt { get; set; }

    public string? LastReleaseRoot { get; set; }

    public List<LastReleaseProject> Projects { get; set; } = [];
}

internal sealed class LastReleaseProject
{
    public string Kind { get; set; } = "";

    public string ProjectFile { get; set; } = "";
}
