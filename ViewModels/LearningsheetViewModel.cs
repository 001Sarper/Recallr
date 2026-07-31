using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class LearningsheetViewModel : ViewModelBase
{
    public CoursesService CoursesService => CoursesService.Instance;
    public AppStateService AppStateService => AppStateService.Instance;
    
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
        var newGuid = Guid.NewGuid();
        
        var newLearnsheet = new Learnsheet
        {
            ID = newGuid, Description = "Lade Dateien hoch und klicke auf 'Lernzettel erstellen' – die KI übernimmt den Rest.", Name = "Unbenanntes Lernzettel", TestDate = "Lege in den Lernzettel Optionen ein Test Datum fest!"
        };
        
        CoursesService.configManager.ClientCourses.FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID).learnsheets.Add(newLearnsheet);
        CoursesService.SaveConfig();
        CoursesService.Instance.LoadLearnsheets(CoursesService.currentCourseID);
        
        string newLearnsheetFolder = Path.Combine(FileSystemPaths.coursesDirectoryPath, CoursesService.currentCourseID, newGuid.ToString());
        Directory.CreateDirectory(newLearnsheetFolder);
        
        AppStateService.CurrentLearningsheet = "Unbenanntes Lernzettel";
        AppStateService.OptionArrow2 = true;
        AppStateService.ShowLearningsheetDetailedView(newGuid.ToString());
    }

    [RelayCommand]
    private void OpenLernzettel(Learnsheet item)
    {
        AppStateService.CurrentLearningsheet = item.Name;
        AppStateService.OptionArrow2 = true;
        AppStateService.ShowLearningsheetDetailedView(item.ID.ToString());
    }

    [RelayCommand]
    private void DeleteLernzettel(Learnsheet item)
    {
        var configEntry = CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID).learnsheets.FirstOrDefault(learnsheet => learnsheet.ID == item.ID);

        if (configEntry != null)
        {
            var path = Path.Combine(FileSystemPaths.coursesDirectoryPath, CoursesService.currentCourseID, item.ID.ToString());
            string chatlogFileName = item.ID + ".json";
            var chatlogPath = Path.Combine(FileSystemPaths.chatlogDirectoryPath, CoursesService.currentCourseID, chatlogFileName);
            try
            {
                if (Directory.Exists(path) && File.Exists(chatlogPath))
                {
                    Directory.Delete(path, true);
                    File.Delete(chatlogPath);
                }
                CoursesService.configManager.ClientCourses.FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID).learnsheets.Remove(configEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        } 
        
        CoursesService.SaveConfig();
        CoursesService.LoadLearnsheets(CoursesService.currentCourseID);
    }
}