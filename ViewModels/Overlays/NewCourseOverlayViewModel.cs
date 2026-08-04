using System;
using System.IO;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class NewCourseOverlayViewModel : ViewModelBase
{
    public AppStateService AppStateService => AppStateService.Instance;
    
    [RelayCommand]
    private void CloseOverlay()
    {
        AppStateService.ClearData();
        AppStateService.CloseOverlay();
    }
    
    [RelayCommand]
    private void CreateCourse()
    {
        if (!AppStateService.IsOverlayEditMode)
        {
            var newCourse = new ClientCourses
            {
                Icon = AppStateService.CourseEmoji, ID = Guid.NewGuid(), Name = AppStateService.CourseName, TeacherName = AppStateService.CourseTeacher
            };
        
            CoursesService.configManager.ClientCourses.Add(newCourse);
            CoursesService.SaveConfig();
            CoursesService.Instance.LoadCourses();
            
            AppStateService.ClearData();
        
            var courseFolder = Path.Combine(FileSystemPaths.coursesDirectoryPath, newCourse.ID.ToString());
            Directory.CreateDirectory(courseFolder);
        
            AppStateService.ShowMessageOverlay(LocalizationService.Instance["successmessage_title"], LocalizationService.Instance["courses_successmessage_message"], Brushes.Green);
            
        }
        else
        {
            var existingCourse = CoursesService.configManager.ClientCourses
                .FirstOrDefault(c => c.ID == AppStateService.course.ID);
            
            existingCourse.Icon = AppStateService.Instance.CourseEmoji;
            existingCourse.Name = AppStateService.Instance.CourseName;
            existingCourse.TeacherName = AppStateService.Instance.CourseTeacher;

            CoursesService.SaveConfig();
            CoursesService.Instance.LoadCourses();
            AppStateService.ClearData();
        }
    }
}