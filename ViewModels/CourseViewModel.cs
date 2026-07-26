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
    private static string _coursesDirectory;
    
    [ObservableProperty] private string _searchText;
    
    
    public CourseViewModel()
    {
        CoursesService.LoadCourses();
        _coursesConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");
        _coursesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Courses");
        
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            CoursesService.LoadCourses();
        }
        else
        {
            var hitCourses = CoursesService.configManager.ClientCourses.Where(c => c.Name.ToLower().StartsWith(value.ToLower()));
            CoursesService.Courses.Clear();
            foreach (var course in hitCourses)
            {
                CoursesService.Courses.Add(course);
            }
        }
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
        OverlayService.ShowLearningsheetView();
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
        var configEntry = CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID == course.ID);

        if (configEntry != null)
        {
            var path = Path.Combine(_coursesDirectory, course.ID.ToString());
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                CoursesService.configManager.ClientCourses.Remove(configEntry);
            }
            catch (Exception ex)
            {
                return;
            }
        }
        
        CoursesService.SaveConfig();
        CoursesService.LoadCourses();
        
    }
    
}