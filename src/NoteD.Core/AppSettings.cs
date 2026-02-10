using System.Text.Json;
using System.Text.Json.Serialization;

namespace NoteD.Core;

public class AppSettings
{
    public List<string> MonitoredKeys { get; set; } = new() { "A", "D", "W", "S" };
    public double LookupWindowMs { get; set; } = 200.0;
    public double DebounceMs { get; set; } = 2.0;
    public int BufferRetentionSeconds { get; set; } = 30;
    
    public OverlaySettings Overlay { get; set; } = new();
    public HotkeySettings Hotkeys { get; set; } = new();
    public string OutputDirectory { get; set; } = "";
    
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NoteD"
    );
    
    private static readonly string SettingsPath = Path.Combine(SettingsFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
        }
        
        var settings = new AppSettings();
        settings.SetDefaults();
        return settings;
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
            
            string json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
        }
    }

    private void SetDefaults()
    {
        if (string.IsNullOrEmpty(OutputDirectory))
        {
            OutputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "NoteD"
            );
        }
    }
}

public class OverlaySettings
{
    public double Opacity { get; set; } = 1.0;
    public double TimelineSeconds { get; set; } = 5.0;
    public int FontSize { get; set; } = 12;
    public KeyColorSettings Colors { get; set; } = new();
}

public class KeyColorSettings
{
    public string AKey { get; set; } = "#FF6B35";
    public string DKey { get; set; } = "#4ECDC4";
    public string WKey { get; set; } = "#FFD93D";
    public string SKey { get; set; } = "#6BCB77";
    public string Click { get; set; } = "#FF3366";
}

public class HotkeySettings
{
    public string ToggleLogging { get; set; } = "F9";
    public string OpenSettings { get; set; } = "F8";
    public string ToggleOverlay { get; set; } = "F7";
    public string ToggleClickThrough { get; set; } = "F6";
    public string TogglePauseVisual { get; set; } = "Tab";
}
