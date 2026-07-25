using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.ViewModels;

namespace Recallr.Models.Services;

public partial class OverlayService : ObservableObject
{
    public static OverlayService Instance { get; } = new OverlayService();
    
    [ObservableProperty] private bool _isOverlayVisible = false;
    
    [ObservableProperty] private ViewModelBase? _currentDialog;
    
    [RelayCommand]
    private void ShowNewCourseOverlay() => CurrentDialog = new NewCourseOverlayViewModel();

    public void CloseOverlay()
    {
        CurrentDialog = null;
        IsOverlayVisible = false;
    }
}