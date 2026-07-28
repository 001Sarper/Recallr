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

    public NewCourseOverlayViewModel()
    {
        _coursesDirectoryPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Courses");
        
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
        
            CoursesService.configManager.ClientCourses.Add(newCourse);
            CoursesService.SaveConfig();
            CoursesService.Instance.LoadCourses();
            
            OverlayService.ClearData();
        
            var courseFolder = Path.Combine(_coursesDirectoryPath, newCourse.ID.ToString());
            Directory.CreateDirectory(courseFolder);
        
            OverlayService.Instance.CloseOverlay();
        }
        else
        {
            var existingCourse = CoursesService.configManager.ClientCourses
                .FirstOrDefault(c => c.ID == OverlayService.course.ID);
            
            existingCourse.Icon = OverlayService.Instance.CourseEmoji;
            existingCourse.Name = OverlayService.Instance.CourseName;
            existingCourse.TeacherName = OverlayService.Instance.CourseTeacher;

            CoursesService.SaveConfig();
            CoursesService.Instance.LoadCourses();
            OverlayService.ClearData();
            
            OverlayService.Instance.CloseOverlay();
        }
    }
}