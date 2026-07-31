using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;

namespace Recallr.Models.Services;

public partial class CoursesService : ObservableObject
{
    public static CoursesService Instance { get; } = new CoursesService();

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
        Courses.Clear();
        
        if (!File.Exists(FileSystemPaths.coursesFilePath))
            return;

        var json = File.ReadAllText(FileSystemPaths.coursesFilePath);
        configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();

        Courses = new ObservableCollection<ClientCourses>(configManager.ClientCourses);
        
    }
    
    public void LoadLearnsheets(string courseID)
    {
        Learnsheets.Clear();
        
        if (!File.Exists(FileSystemPaths.coursesFilePath))
            return;

        var json = File.ReadAllText(FileSystemPaths.coursesFilePath);
        configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();

        var course = configManager.ClientCourses.FirstOrDefault(course => course.ID.ToString() == courseID);
        
        Learnsheets = new ObservableCollection<Learnsheet>(course?.learnsheets ?? new List<Learnsheet>());
    }
    
    public static void SaveConfig()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(configManager, options);
        File.WriteAllText(FileSystemPaths.coursesFilePath, json);
    }
    
    
    
}