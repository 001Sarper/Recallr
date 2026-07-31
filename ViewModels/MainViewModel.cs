using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Views;
using Avalonia.Platform.Storage;
using OpenAI.Chat;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public SettingsService Settings => SettingsService.Instance;
    public AppStateService AppState => AppStateService.Instance;
    
    [ObservableProperty] private string _lernzettel = "";

    private readonly Window _window;
    
    
    [ObservableProperty]
    private int _selectedNavIndex = 0;
    

    public MainViewModel()
    {
        App.Instance.SetTheme(Settings.Theme); 
        AppState.ShowCourseView();
    }

    partial void OnSelectedNavIndexChanged(int selectedNavIndex)
    {
        switch (selectedNavIndex)
        {
            case 0:
                AppState.ClearBreadcrumbs();
                AppState.CurrentOption = "Meine Fächer";
                if(AppStateService.Instance.IsOverlayVisible) AppStateService.Instance.CloseOverlay();
                AppState.ShowCourseView();
                break;
            case 1:
                AppState.ClearBreadcrumbs();
                AppState.CurrentOption = "Einstellungen";
                if(AppStateService.Instance.IsOverlayVisible) AppStateService.Instance.CloseOverlay();
                AppState.ShowSettingsView();
                break;
        }
    }
    
    [RelayCommand]
    private void GoBack()
    {
        if (AppState.CurrentLearningsheet != "")
        {
            AppState.ShowLearningsheetView();
            AppState.CurrentLearningsheet = "";
            AppState.OptionArrow2 = false;
        } else if (AppState.CurrentCourse != "")
        {
            AppState.ShowCourseView();
            AppState.CurrentCourse = "";
            AppState.OptionArrow1 = false;
        }
    }
    
    
}