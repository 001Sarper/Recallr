using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Models;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public SettingsService Settings => SettingsService.Instance;
    
    // Account
    [ObservableProperty] private string _draftOpenaiKey;
    [ObservableProperty] private string _draftProfileName;
    [ObservableProperty] private string _draftProfileMail;
    [ObservableProperty] private string _draftAiModel;

    // View
    [ObservableProperty] private int _draftTheme;
    [ObservableProperty] private int _draftLanguage;
    [ObservableProperty] private int _draftFontSize;

    // AI-Behaviour
    [ObservableProperty] private int _draftSummaryStyle;
    [ObservableProperty] private int _draftDifficulty;
    [ObservableProperty] private int _draftQuestionType;

    [ObservableProperty] private ObservableCollection<string> _models = new();
    [ObservableProperty] private bool _modelsLoading;
    

    public SettingsViewModel()
    {
        //Load Settings
        LoadSettings();
        LoadModelsAsync();
    }

    private void LoadSettings()
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

    private async void LoadModelsAsync()
    {
        ModelsLoading = true;
        try
        {
            Models = await AIService.Instance.GetOpenAiModels();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Modelle: {ex.Message}");
        }
        finally
        {
            DraftAiModel = Settings.AiModel;
            ModelsLoading = false;
        }
    }

    [RelayCommand]
    public void SaveSettings()
    {
        Settings.OpenaiKey = DraftOpenaiKey;
        Settings.ProfileName = DraftProfileName;
        Settings.ProfileMail = DraftProfileMail;
        Settings.AiModel = DraftAiModel;

        Settings.Theme = DraftTheme;
        Settings.Language = DraftLanguage;
        Settings.FontSize = DraftFontSize;

        Settings.SummaryStyle = DraftSummaryStyle;
        Settings.Difficulty = DraftDifficulty;
        Settings.QuestionType = DraftQuestionType;
        
        Settings.Save();
    }
}