using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public SettingsService Settings => SettingsService.Instance;
    public AppStateService AppState => AppStateService.Instance;
    
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
                AppState.CurrentOption = LocalizationService.Instance["courses_nav"];
                if(AppStateService.Instance.IsOverlayVisible) AppStateService.Instance.CloseOverlay();
                AppState.ShowCourseView();
                break;
            case 1:
                AppState.ClearBreadcrumbs();
                AppState.CurrentOption = LocalizationService.Instance["settings_nav"];
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