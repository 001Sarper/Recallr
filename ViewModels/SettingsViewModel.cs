using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Settings;

namespace Recallr.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _parentDirectory = Path.Combine(_appData, "Recallr");
    private static readonly string _configDirectory = Path.Combine(_parentDirectory, "Config");
    
    private static readonly string _settingsFilePath = Path.Combine(_configDirectory, "ClientSettings.json");
    
    public static string json = File.ReadAllText(_settingsFilePath);
    public static SettingsManager? settingsManager = JsonSerializer.Deserialize<SettingsManager>(json) ?? new SettingsManager();
    
    //Account
    [ObservableProperty] private string _openaiKey = "";
    [ObservableProperty] private string _profileName = "";
    [ObservableProperty] private string _profileMail = "";
    
    //View
    [ObservableProperty] private int _theme = 0;
    [ObservableProperty] private int _language = 0;
    [ObservableProperty] private int _fontSize = 0;
    
    //AI-Behaviour
    [ObservableProperty] private int _summaryStyle = 0;
    [ObservableProperty] private int _difficulty = 0;
    [ObservableProperty] private int _questionType = 0;

    public SettingsViewModel()
    {
        //Load Settings
        OpenaiKey = settingsManager.ClientSettings[0].OpenaiKey;
        ProfileName = settingsManager.ClientSettings[0].ProfileName;
        ProfileMail = settingsManager.ClientSettings[0].ProfileMail;
        
        Theme = settingsManager.ClientSettings[0].Theme;
        Language = settingsManager.ClientSettings[0].Language;
        FontSize = settingsManager.ClientSettings[0].FontSize;
        
        SummaryStyle = settingsManager.ClientSettings[0].SummaryStyle;
        Difficulty = settingsManager.ClientSettings[0].Difficulty;
        QuestionType = settingsManager.ClientSettings[0].QuestionType;
        
    }

    [RelayCommand]
    public void SaveSettings()
    {
        settingsManager.ClientSettings[0].OpenaiKey = OpenaiKey;
        settingsManager.ClientSettings[0].ProfileName = ProfileName;
        settingsManager.ClientSettings[0].ProfileMail = ProfileMail;

        settingsManager.ClientSettings[0].Theme = Theme;
        settingsManager.ClientSettings[0].Language = Language;
        settingsManager.ClientSettings[0].FontSize = FontSize;

        settingsManager.ClientSettings[0].SummaryStyle = SummaryStyle;
        settingsManager.ClientSettings[0].Difficulty = Difficulty;
        settingsManager.ClientSettings[0].QuestionType = QuestionType;
        
        File.WriteAllText(_settingsFilePath, JsonSerializer.Serialize(settingsManager));

        App.Instance.SetTheme(Theme);
    }
}