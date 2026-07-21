using Avalonia.Controls;
using Recallr.ViewModels;

namespace Recallr.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}