using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using QuestPDF;
using QuestPDF.Infrastructure;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            EnsureConfigFiles();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Failed to initialize config files: {ex.GetType().Name}");
            Console.WriteLine($"[CRITICAL] Error message: {ex.Message}");
            Console.WriteLine("[WARNING] App will continue with potentially missing configuration files.");
        }

        QuestPDF.Settings.License = LicenseType.Community;
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);   
    }

    static void EnsureConfigFiles()
    {
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string parentDirectory = Path.Combine(appData, "Recallr");
            Directory.CreateDirectory(parentDirectory);

            string configDirectory = Path.Combine(parentDirectory, "Config");
            Directory.CreateDirectory(configDirectory);

            string settingsFilePath = Path.Combine(configDirectory, "ClientSettings.json");
            string coursesFilePath = Path.Combine(configDirectory, "ClientCourses.json");

            string coursesDirectory = Path.Combine(parentDirectory, "Courses");
            Directory.CreateDirectory(coursesDirectory);

            if (!File.Exists(settingsFilePath))
            {
                var defaultSettings = new ConfigManager
                {
                    ClientSettings = new List<ClientSettings>
                    {
                        new ClientSettings
                        {
                            OpenaiKey = "", ProfileName = "Example User", ProfileMail = "examplemail@proton.me",
                            AiModel = "gpt-5.6-luna",
                            Theme = 0, Language = 1, FontSize = 0, SummaryStyle = 0, Difficulty = 1, QuestionType = 1
                        }
                    }
                };
                File.WriteAllText(settingsFilePath,
                    JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true }));
            }

            if (!File.Exists(coursesFilePath))
            {
                File.WriteAllText(coursesFilePath, "{}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
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
