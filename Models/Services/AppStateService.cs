using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.ViewModels;

namespace Recallr.Models.Services;

public partial class AppStateService : ObservableObject
{
    public static AppStateService Instance { get; } = new AppStateService();
    
    [ObservableProperty] private bool _isOverlayVisible = false;
    
    [ObservableProperty] private ViewModelBase? _currentDialog;

    [ObservableProperty] private IBrush _borderColor = Brushes.Gray;
    
    //CourseEditor Variables
    
    [ObservableProperty] private int _courseEmoji = 0;
    [ObservableProperty] private string _courseTeacher;
    [ObservableProperty] private string _courseName;

    [ObservableProperty] private bool _isOverlayEditMode = false;
    [ObservableProperty] private string _buttonContent = "";
    
    //Breadcrumbs Variables
    
    [ObservableProperty] private string _currentOption = LocalizationService.Instance["courses_nav"];
    [ObservableProperty] private string _currentCourse = "";
    [ObservableProperty] private string _currentLearningsheet = "";
    
    [ObservableProperty] private bool _optionArrow1 = false;
    [ObservableProperty] private bool _optionArrow2 = false;
    
    //MessageBox Variables
    
    [ObservableProperty] private string _messageViewTitle = "";
    [ObservableProperty] private string _messageViewMessage = "";
    
    [ObservableProperty]
    private ViewModelBase currentPageViewModel;

    public ClientCourses course;
    
    [RelayCommand]
    private void ShowNewCourseOverlay() => CurrentDialog = new NewCourseOverlayViewModel();
    
    
    public void ShowCourseView() => CurrentPageViewModel = new CourseViewModel();
    public void ShowSettingsView() => CurrentPageViewModel = new SettingsViewModel();
    public void ShowAiChatView() => CurrentPageViewModel = new AiChatViewModel();
    public void ShowLearningsheetView() => CurrentPageViewModel = new LearningsheetViewModel();
    public void ShowLearningsheetDetailedView(string learningsheetID) => CurrentPageViewModel = new LearningsheetDetailedViewModel(learningsheetID);

    public void CloseOverlay()
    {
        CurrentDialog = null;
        IsOverlayVisible = false;
    }

    public void ShowCourseEditorOverlay()
    {
        CurrentDialog = new NewCourseOverlayViewModel();
        BorderColor = Brushes.CadetBlue;
        IsOverlayVisible = true;
    }

    public void ShowMessageOverlay(string title, string message, IBrush borderColor)
    {
        MessageViewTitle = title;
        MessageViewMessage = message;
        CurrentDialog = new MessageOverlayViewModel();
        BorderColor = borderColor;
        IsOverlayVisible = true;
    }

    public void ClearData()
    {
        CourseEmoji = 0;
        CourseTeacher = "";
        CourseName = "";
        MessageViewTitle = "";
        MessageViewMessage = "";
        BorderColor = Brushes.Gray;
    }
    
    public void ClearBreadcrumbs()
    {
        CurrentOption = "";
        CurrentCourse = "";
        CurrentLearningsheet = "";
        OptionArrow1 = false;
        OptionArrow2 = false;
    }
    
}