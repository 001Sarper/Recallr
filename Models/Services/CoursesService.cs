using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    
    private static readonly string _configPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");

    [ObservableProperty]
    private ObservableCollection<ClientCourses> courses = new();
    
    
    private ConfigManager _configManager;
    private static string _coursesFilePath;
    
    public CoursesService()
    {
        _coursesFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");
        
        var json = File.ReadAllText(_coursesFilePath);
        _configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();
        
    }
    
    public void LoadCourses()
    {
        if (!File.Exists(_configPath))
            return;

        var json = File.ReadAllText(_configPath);
        var configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();

        Courses = new ObservableCollection<ClientCourses>(configManager.ClientCourses);
    }
    
    
    
}