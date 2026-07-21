using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Views;

namespace Recallr.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] 
    private string _lernzettel = "";

    [RelayCommand]
    private void PDF_Hochladen_Click()
    {
        
    }
}
