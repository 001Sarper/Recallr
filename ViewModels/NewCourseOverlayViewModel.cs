using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class NewCourseOverlayViewModel : ViewModelBase
{
    public OverlayService OverlayService => OverlayService.Instance;
    
    
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
        OverlayService.ClearData();
        OverlayService.CloseOverlay();
    }
    
    [RelayCommand]
    private void CreateCourse()
    {
        if (!OverlayService.IsOverlayEditMode)
        {
            var newCourse = new ClientCourses
            {
                Icon = OverlayService.CourseEmoji, ID = Guid.NewGuid(), Name = OverlayService.CourseName, TeacherName = OverlayService.CourseTeacher
            };
        
            _configManager.ClientCourses.Add(newCourse);
            CoursesService.Instance.Courses.Add(newCourse);
        
            SaveConfig();
            OverlayService.ClearData();
        
            var courseFolder = Path.Combine(_coursesDirectoryPath, newCourse.ID.ToString());
            Directory.CreateDirectory(courseFolder);
        
            OverlayService.Instance.CloseOverlay();
        }
        else
        {
            var existingCourse = _configManager.ClientCourses
                .FirstOrDefault(c => c.ID == OverlayService.course.ID);
            
            existingCourse.Icon = OverlayService.Instance.CourseEmoji;
            existingCourse.Name = OverlayService.Instance.CourseName;
            existingCourse.TeacherName = OverlayService.Instance.CourseTeacher;

            SaveConfig();
            OverlayService.ClearData();
            
            OverlayService.Instance.CloseOverlay();
        }
    }
    
    private void SaveConfig()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_configManager, options);
        File.WriteAllText(_coursesConfigPath, json);
    }
}