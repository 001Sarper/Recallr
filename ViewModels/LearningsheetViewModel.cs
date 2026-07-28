using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class LearningsheetViewModel : ViewModelBase
{
    public CoursesService CoursesService => CoursesService.Instance;
    public OverlayService OverlayService => OverlayService.Instance;
    
    [ObservableProperty] private string _searchText;

    public LearningsheetViewModel()
    {
        CoursesService.LoadLearnsheets(CoursesService.currentCourseID);
    }
    
    
    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            CoursesService.LoadLearnsheets(CoursesService.currentCourseID);
        }
        else
        {
            var hitLearnsheets = CoursesService.configManager.ClientCourses.FirstOrDefault(course => course.ID.ToString() == CoursesService.currentCourseID).learnsheets.Where(learnsheet => learnsheet.Name.ToLower().StartsWith(value.ToLower()));
            CoursesService.Learnsheets.Clear();
            foreach (var learnsheet in hitLearnsheets)
            {
                CoursesService.Learnsheets.Add(learnsheet);
            }
        }
    }

    [RelayCommand]
    private void CreateLearnsheet()
    {
        OverlayService.ShowLearningsheetDetailedView();
    }

    [RelayCommand]
    private void OpenLernzettel(Learnsheet item)
    {
        // Navigation zur Detailansicht
    }

    [RelayCommand]
    private void EditLernzettel(Learnsheet item)
    {
        // z.B. Navigation zum Editor mit vorbefülltem Item
    }

    [RelayCommand]
    private void DeleteLernzettel(Learnsheet item)
    {
        // + Löschung in Storage/DB
    }
}