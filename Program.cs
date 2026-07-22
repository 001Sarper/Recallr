using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Recallr.Models.Settings;

namespace Recallr;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        EnsureConfigFiles();
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);   
    }

    static void EnsureConfigFiles()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string parentDirectory = Path.Combine(appData, "Recallr");
        Directory.CreateDirectory(parentDirectory);
        
        string configDirectory = Path.Combine(parentDirectory, "Config");
        Directory.CreateDirectory(configDirectory);
        
        string settingsFilePath = Path.Combine(configDirectory, "ClientSettings.json");
        
        string coursesDirectory = Path.Combine(parentDirectory, "Courses");
        Directory.CreateDirectory(coursesDirectory);
        
        
        


        if (!File.Exists(settingsFilePath))
        {
            var defaultSettings = new SettingsManager
            {
                ClientSettings = new List<ClientSettings>
                {
                    new ClientSettings
                    {
                        OpenaiKey = "", ProfileName = "Example User", ProfileMail = "examplemail@proton.me",
                        Theme = 0, Language = 0, FontSize = 0, SummaryStyle = 0, Difficulty = 1, QuestionType = 1
                    }
                }
            };
            File.WriteAllText(settingsFilePath, JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
    

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
