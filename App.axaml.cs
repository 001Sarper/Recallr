using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Recallr.ViewModels;
using Recallr.Views;

namespace Recallr;

public partial class App : Application
{
    public static App Instance { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Instance = this;

    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel(mainWindow);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public void SetTheme(int theme)
    {
        switch (theme)
        {
            case 0:
                RequestedThemeVariant = ThemeVariant.Default;
                break;
            case 1:
                RequestedThemeVariant = ThemeVariant.Dark;
                break;
            case 2:
                RequestedThemeVariant = ThemeVariant.Light;
                break;
        }
            
    }
}