using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class CourseViewModel : ViewModelBase
{
    public CoursesService CoursesService => CoursesService.Instance;

    public CourseViewModel()
    {
        CoursesService.Instance.LoadCourses();
    }
    
    [RelayCommand]
    public void ShowOverlay()
    {
        OverlayService.Instance.IsOverlayVisible = true;
        OverlayService.Instance.ShowNewCourseOverlayCommand.Execute(null);
    }
    
    
    [RelayCommand]
    private void OpenCourse(ClientCourses course)
    {
        
    }

    [RelayCommand]
    private void EditCourse(ClientCourses course)
    {
        // z.B. das gleiche Overlay wie beim Erstellen öffnen, 
        // aber mit vorausgefüllten Werten aus `course`
    }

    [RelayCommand]
    private void DeleteCourse(ClientCourses course)
    {
        
    }
}