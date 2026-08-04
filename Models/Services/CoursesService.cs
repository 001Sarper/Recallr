using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Recallr.Models.Configuration;

namespace Recallr.Models.Services;

public partial class CoursesService : ObservableObject
{
    public static CoursesService Instance { get; } = new CoursesService();
    
    private static AppStateService AppState => AppStateService.Instance;

    [ObservableProperty]
    private ObservableCollection<ClientCourses> courses = new();
    
    [ObservableProperty]
    private ObservableCollection<Learnsheet> learnsheets = new();
    
    public static string currentCourseID = "";
    public static ConfigManager configManager;
    
    public CoursesService()
    {
        LoadCourses();
    }
    
    public void LoadCourses()
    {
        try
        {
            Courses.Clear();

            if (!File.Exists(FileSystemPaths.coursesFilePath))
                return;

            var json = File.ReadAllText(FileSystemPaths.coursesFilePath);
            configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();

            Courses = new ObservableCollection<ClientCourses>(configManager.ClientCourses);
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
        
        
        
    }
    
    public void LoadLearnsheets(string courseID)
    {
        try
        {
            Learnsheets.Clear();
                    
                    if (!File.Exists(FileSystemPaths.coursesFilePath))
                        return;
            
                    var json = File.ReadAllText(FileSystemPaths.coursesFilePath);
                    configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();
            
                    var course = configManager.ClientCourses.FirstOrDefault(course => course.ID.ToString() == courseID);
                    
                    Learnsheets = new ObservableCollection<Learnsheet>(course?.learnsheets ?? new List<Learnsheet>());
        }catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
        
        
    }
    
    public static void SaveConfig()
    {
        

        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(configManager, options);
                    File.WriteAllText(FileSystemPaths.coursesFilePath, json);
        }catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
    }
    
    
    
}