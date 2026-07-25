using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class CourseViewModel : ViewModelBase
{
    public CoursesService CoursesService => CoursesService.Instance;
    public OverlayService OverlayService => OverlayService.Instance;
    
    private static string _coursesConfigPath;
    private ConfigManager _configManager;
    
    
    public CourseViewModel()
    {
        CoursesService.LoadCourses();
        _coursesConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");
        
        var json = File.ReadAllText(_coursesConfigPath);
        _configManager = JsonSerializer.Deserialize<ConfigManager>(json) ?? new ConfigManager();
    }
    
    [RelayCommand]
    public void ShowOverlay()
    {
        OverlayService.ButtonContent = "Erstellen";
        OverlayService.IsOverlayVisible = true;
        OverlayService.ShowOverlay();
    }
    
    
    [RelayCommand]
    private void OpenCourse(ClientCourses course)
    {
        
    }

    [RelayCommand]
    private void EditCourse(ClientCourses course)
    {
        
        OverlayService.CourseEmoji = course.Icon;
        OverlayService.CourseName = course.Name;
        OverlayService.CourseTeacher = course.TeacherName;
        OverlayService.course = course;
        OverlayService.ButtonContent = "Bearbeiten";
        OverlayService.IsOverlayEditMode = true;
        OverlayService.ShowOverlay();
    }

    [RelayCommand]
    private void DeleteCourse(ClientCourses course)
    {
        var configEntry = _configManager.ClientCourses
            .FirstOrDefault(c => c.ID == course.ID);
    
        if (configEntry != null)
            _configManager.ClientCourses.Remove(configEntry);

        CoursesService.Courses.Remove(course);
        
        SaveConfig();
        
    }
    
    private void SaveConfig()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_configManager, options);
        File.WriteAllText(_coursesConfigPath, json);
    }
}