using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Recallr;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

public partial class SettingsService : ObservableObject
{
    public static SettingsService Instance { get; } = new SettingsService();

    // Account
    [ObservableProperty] private string _openaiKey = "";
    [ObservableProperty] private string _profileName = "";
    [ObservableProperty] private string _profileMail = "";

    // View
    [ObservableProperty] private int _theme = 0;
    [ObservableProperty] private int _language = 0;
    [ObservableProperty] private int _fontSize = 0;

    // AI-Behaviour
    [ObservableProperty] private int _summaryStyle = 0;
    [ObservableProperty] private int _difficulty = 0;
    [ObservableProperty] private int _questionType = 0;

    private ConfigManager _configManager;
    
    private SettingsService()
    {
        var json = File.ReadAllText(FileSystemPaths.settingsFilePath);
        _configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();
        Load();
    }

    public void Save()
    {
        var s = _configManager.ClientSettings[0];

        s.OpenaiKey = OpenaiKey;
        s.ProfileName = ProfileName;
        s.ProfileMail = ProfileMail;
        s.Theme = Theme;
        s.Language = Language;
        s.FontSize = FontSize;
        s.SummaryStyle = SummaryStyle;
        s.Difficulty = Difficulty;
        s.QuestionType = QuestionType;

        File.WriteAllText(FileSystemPaths.settingsFilePath, JsonSerializer.Serialize(_configManager));

        App.Instance.SetTheme(Theme);
    }

    private void Load()
    {
        var s = _configManager.ClientSettings[0];

        OpenaiKey = s.OpenaiKey;
        ProfileName = s.ProfileName;
        ProfileMail = s.ProfileMail;

        Theme = s.Theme;
        Language = s.Language;
        FontSize = s.FontSize;

        SummaryStyle = s.SummaryStyle;
        Difficulty = s.Difficulty;
        QuestionType = s.QuestionType;
    }
}