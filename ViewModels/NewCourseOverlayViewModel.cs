using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class NewCourseOverlayViewModel : ViewModelBase
{
    [ObservableProperty] private int _courseEmoji = 0;
    [ObservableProperty] private string _courseTeacher;
    [ObservableProperty] private string _courseName;
    
    private static string _coursesDirectoryPath;
    private static string _coursesConfigPath;
    private ConfigManager _configManager;

    public NewCourseOverlayViewModel()
    {
        _coursesDirectoryPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Courses");
        _coursesConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");
        
        var json = File.ReadAllText(_coursesConfigPath);
        _configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();
    }
    
    [RelayCommand]
    private void CloseOverlay()
    {
        OverlayService.Instance.CloseOverlay();
    }
    
    [RelayCommand]
    private void CreateCourse()
    {
        var newCourse = new ClientCourses
        {
            Icon = CourseEmoji, ID = Guid.NewGuid(), Name = CourseName, TeacherName = CourseTeacher
        };
        
        _configManager.ClientCourses.Add(newCourse);
        CoursesService.Instance.Courses.Add(newCourse);
        
        SaveConfig();
        
        var courseFolder = Path.Combine(_coursesDirectoryPath, newCourse.ID.ToString());
        Directory.CreateDirectory(courseFolder);
        
        OverlayService.Instance.CloseOverlay();
    }
    
    private void SaveConfig()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_configManager, options);
        File.WriteAllText(_coursesConfigPath, json);
    }
}