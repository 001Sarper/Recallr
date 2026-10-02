using System.Collections.Generic;
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
    [ObservableProperty] private string _courseTeacher = "";
    [ObservableProperty] private string _courseName = "";

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
    private ViewModelBase currentPageViewModel = null!;

    private CourseViewModel? _courseViewModel;
    private SettingsViewModel? _settingsViewModel;
    private AiChatViewModel? _aiChatViewModel;
    private ExamsViewModel? _examsViewModel;

    public ClientCourses course = null!;
    
    [RelayCommand]
    private void ShowNewCourseOverlay() => CurrentDialog = new NewCourseOverlayViewModel();
    
    public void ShowCourseView()
    {
        _courseViewModel ??= new CourseViewModel();
        CurrentPageViewModel = _courseViewModel;
    }

    public void ShowSettingsView()
    {
        _settingsViewModel ??= new SettingsViewModel();
        CurrentPageViewModel = _settingsViewModel;
    }

    public void ShowAiChatView()
    {
        _aiChatViewModel ??= new AiChatViewModel();
        CurrentPageViewModel = _aiChatViewModel;
    }

    public void ShowExamsView()
    {
        _examsViewModel ??= new ExamsViewModel();
        CurrentPageViewModel = _examsViewModel;
    }

    public void ShowLearningsheetView() => CurrentPageViewModel = new LearningsheetViewModel();
    public void ShowLearningsheetDetailedView(string learningsheetID) => CurrentPageViewModel = new LearningsheetDetailedViewModel(learningsheetID);
    public void ShowExamDetailView(string examID) => CurrentPageViewModel = new ExamDetailViewModel(examID);

    public void ShowGenerateExamOverlay(List<Configuration.Learnsheet> selectedSheets, Configuration.ClientCourses? course)
    {
        CurrentDialog = new GenerateExamOverlayViewModel(selectedSheets, course);
        BorderColor = Brushes.CadetBlue;
        IsOverlayVisible = true;
    }


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