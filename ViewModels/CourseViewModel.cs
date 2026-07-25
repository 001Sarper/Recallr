using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class CourseViewModel : ViewModelBase
{
    [RelayCommand]
    public void ShowOverlay()
    {
        OverlayService.Instance.IsOverlayVisible = true;
        OverlayService.Instance.ShowNewCourseOverlayCommand.Execute(null);
    }
}