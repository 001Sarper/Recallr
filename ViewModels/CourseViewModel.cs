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
    public AppStateService AppStateService => AppStateService.Instance;
    
    [ObservableProperty] private string _searchText;
    
    
    public CourseViewModel()
    {
        CoursesService.LoadCourses();
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
        AppStateService.ButtonContent = LocalizationService.Instance["learn_sheet_overlay_create"];
        AppStateService.IsOverlayVisible = true;
        AppStateService.ShowOverlay();
    }
    
    
    [RelayCommand]
    private void OpenCourse(ClientCourses course)
    {
        CoursesService.currentCourseID = course.ID.ToString();
        AppStateService.CurrentCourse = course.Name;
        AppStateService.OptionArrow1 = true;
        AppStateService.ShowLearningsheetView();
    }

    [RelayCommand]
    private void EditCourse(ClientCourses course)
    {
        
        AppStateService.CourseEmoji = course.Icon;
        AppStateService.CourseName = course.Name;
        AppStateService.CourseTeacher = course.TeacherName;
        AppStateService.course = course;
        AppStateService.ButtonContent = LocalizationService.Instance["learn_sheet_overlay_edit"];
        AppStateService.IsOverlayEditMode = true;
        AppStateService.ShowOverlay();
    }

    [RelayCommand]
    private void DeleteCourse(ClientCourses course)
    {
        var configEntry = CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID == course.ID);

        if (configEntry != null)
        {
            var path = Path.Combine(FileSystemPaths.coursesDirectoryPath, course.ID.ToString());
            var chatlogCourseDirectory = Path.Combine(FileSystemPaths.chatlogDirectoryPath, course.ID.ToString());
            try
            {
                if (Directory.Exists(path) && Directory.Exists(chatlogCourseDirectory))
                {
                    Directory.Delete(path, true);
                    Directory.Delete(chatlogCourseDirectory, true);
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