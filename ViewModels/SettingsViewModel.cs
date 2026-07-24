using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Recallr.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public SettingsService Settings => SettingsService.Instance;
    
    // Account
    [ObservableProperty] private string _draftOpenaiKey;
    [ObservableProperty] private string _draftProfileName;
    [ObservableProperty] private string _draftProfileMail;

    // View
    [ObservableProperty] private int _draftTheme;
    [ObservableProperty] private int _draftLanguage;
    [ObservableProperty] private int _draftFontSize;

    // AI-Behaviour
    [ObservableProperty] private int _draftSummaryStyle;
    [ObservableProperty] private int _draftDifficulty;
    [ObservableProperty] private int _draftQuestionType;

    public SettingsViewModel()
    {
        //Load Settings
        LoadSettings();
        
    }

    public void LoadSettings()
    {
        DraftOpenaiKey = Settings.OpenaiKey;
        DraftProfileName = Settings.ProfileName;
        DraftProfileMail = Settings.ProfileMail;

        DraftTheme = Settings.Theme;
        DraftLanguage = Settings.Language;
        DraftFontSize = Settings.FontSize;

        DraftSummaryStyle = Settings.SummaryStyle;
        DraftDifficulty = Settings.Difficulty;
        DraftQuestionType = Settings.QuestionType;
    }

    [RelayCommand]
    public void SaveSettings()
    {
        Settings.OpenaiKey = DraftOpenaiKey;
        Settings.ProfileName = DraftProfileName;
        Settings.ProfileMail = DraftProfileMail;

        Settings.Theme = DraftTheme;
        Settings.Language = DraftLanguage;
        Settings.FontSize = DraftFontSize;

        Settings.SummaryStyle = DraftSummaryStyle;
        Settings.Difficulty = DraftDifficulty;
        Settings.QuestionType = DraftQuestionType;
        
        Settings.Save();
    }
}