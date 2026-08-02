using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.DataProtection;
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
    [ObservableProperty] private string _aiModel = "";

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

        s.OpenaiKey = (string.IsNullOrEmpty(OpenaiKey)) ? "" : App.Instance.Protector.Protect(OpenaiKey);
        s.AiModel = AiModel;
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

        OpenaiKey = (string.IsNullOrEmpty(s.OpenaiKey)) ? "" : App.Instance.Protector.Unprotect(s.OpenaiKey);
        ProfileName = s.ProfileName;
        ProfileMail = s.ProfileMail;
        AiModel = s.AiModel;

        Theme = s.Theme;
        Language = s.Language;
        FontSize = s.FontSize;

        SummaryStyle = s.SummaryStyle;
        Difficulty = s.Difficulty;
        QuestionType = s.QuestionType;
    }
}