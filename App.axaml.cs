using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Recallr.ViewModels;
using Recallr.Views;
using LiveChartsCore; // <-- WICHTIG
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Recallr.Models.Services;
using Recallr.Models.Services.Interfaces;

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
        LiveCharts.Configure(config => 
                config
                    .AddSkiaSharp()
                    .AddDefaultMappers()
                    .AddDarkTheme() // oder AddLightTheme()
        );
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            // FilePickerService registrieren
            services.AddSingleton<IFilePickerService>(new FilePickerService(
                () => desktop.MainWindow));
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
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