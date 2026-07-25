using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.ViewModels;

namespace Recallr.Models.Services;

public partial class OverlayService : ObservableObject
{
    public static OverlayService Instance { get; } = new OverlayService();
    
    [ObservableProperty] private bool _isOverlayVisible = false;
    
    [ObservableProperty] private ViewModelBase? _currentDialog;
    
    [ObservableProperty] private int _courseEmoji = 0;
    [ObservableProperty] private string _courseTeacher;
    [ObservableProperty] private string _courseName;

    [ObservableProperty] private bool _isOverlayEditMode = false;
    [ObservableProperty] private string _buttonContent = "";
    
    [ObservableProperty]
    private ViewModelBase currentPageViewModel;

    public ClientCourses course;
    
    [RelayCommand]
    private void ShowNewCourseOverlay() => CurrentDialog = new NewCourseOverlayViewModel();
    
    
    public void ShowCourseView() => CurrentPageViewModel = new CourseViewModel();

    
    public void ShowSettingsView() => CurrentPageViewModel = new SettingsViewModel();
    
    
    public void ShowLearningsheetView() => CurrentPageViewModel = new LearningsheetViewModel();

    public void CloseOverlay()
    {
        CurrentDialog = null;
        IsOverlayVisible = false;
    }

    public void ShowOverlay()
    {
        CurrentDialog = new NewCourseOverlayViewModel();
        IsOverlayVisible = true;
    }

    public void ClearData()
    {
        CourseEmoji = 0;
        CourseTeacher = "";
        CourseName = "";
    }
    
}