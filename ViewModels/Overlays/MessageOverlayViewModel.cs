using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class MessageOverlayViewModel : ViewModelBase
{
    public AppStateService AppStateService => AppStateService.Instance;


    [RelayCommand]
    private void CloseMessageOverlay()
    {
        AppStateService.ClearData();
        AppStateService.CloseOverlay();
    }
}