using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Recallr.Models.Configuration;

namespace Recallr.Models.Services;

public partial class CoursesService : ObservableObject
{
    public static CoursesService Instance { get; } = new CoursesService();
    
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _teacherName;
    [ObservableProperty] private Learnsheet _learnsheet;
    
    
    private ConfigManager _configManager;
    private static string _coursesFilePath;
    
    public CoursesService()
    {
        _coursesFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");
        
        var json = File.ReadAllText(_coursesFilePath);
        _configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();
        
    }
}