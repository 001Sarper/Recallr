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
    public OverlayService Overlay => OverlayService.Instance;
    
    [ObservableProperty] private string _lernzettel = "";

    private readonly Window _window;
    
    
    [ObservableProperty]
    private int _selectedNavIndex = 0;
    

    public MainViewModel()
    {
        App.Instance.SetTheme(Settings.Theme); 
        Overlay.ShowCourseView();
    }

    partial void OnSelectedNavIndexChanged(int selectedNavIndex)
    {
        switch (selectedNavIndex)
        {
            case 0:
                Overlay.ClearBreadcrumbs();
                Overlay.CurrentOption = "Meine Fächer";
                if(OverlayService.Instance.IsOverlayVisible) OverlayService.Instance.CloseOverlay();
                Overlay.ShowCourseView();
                break;
            case 1:
                Overlay.ClearBreadcrumbs();
                Overlay.CurrentOption = "Einstellungen";
                if(OverlayService.Instance.IsOverlayVisible) OverlayService.Instance.CloseOverlay();
                Overlay.ShowSettingsView();
                break;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (OverlayService.Instance.CurrentLearningsheet != "")
        {
            OverlayService.Instance.ShowLearningsheetView();
            OverlayService.Instance.CurrentLearningsheet = "";
            Overlay.OptionArrow2 = false;
        } else if (OverlayService.Instance.CurrentCourse != "")
        {
            OverlayService.Instance.ShowCourseView();
            OverlayService.Instance.CurrentCourse = "";
            Overlay.OptionArrow1 = false;
        }
    }
    
    
}