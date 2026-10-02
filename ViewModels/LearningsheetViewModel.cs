using System;
using System.Collections.Generic;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActionBarVisible))]
    [NotifyPropertyChangedFor(nameof(SelectedCount))]
    private ObservableCollection<LearningsheetSelectionItem> _selectableLearnsheets = new();

    public bool IsActionBarVisible => SelectableLearnsheets.Any(i => i.IsSelected);
    public int SelectedCount => SelectableLearnsheets.Count(i => i.IsSelected);
    public string SelectedCountLabel => $"{SelectedCount} {LocalizationService.Instance["sheets_selected"]}";

    public LearningsheetViewModel()
    {
        CoursesService.LoadLearnsheets(CoursesService.currentCourseID);
        RebuildSelectableList();
        CoursesService.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CoursesService.Learnsheets))
                RebuildSelectableList();
        };
    }

    private void RebuildSelectableList()
    {
        // Preserve existing selection state when rebuilding
        var previouslySelected = SelectableLearnsheets
            .Where(i => i.IsSelected)
            .Select(i => i.Learnsheet.ID)
            .ToHashSet();

        var newList = CoursesService.Learnsheets
            .Select(ls =>
            {
                var item = new LearningsheetSelectionItem(ls)
                {
                    IsSelected = previouslySelected.Contains(ls.ID)
                };
                item.PropertyChanged += (_, _) =>
                {
                    OnPropertyChanged(nameof(IsActionBarVisible));
                    OnPropertyChanged(nameof(SelectedCount));
                    OnPropertyChanged(nameof(SelectedCountLabel));
                };
                return item;
            });

        SelectableLearnsheets = new ObservableCollection<LearningsheetSelectionItem>(newList);
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            CoursesService.LoadLearnsheets(CoursesService.currentCourseID);
        }
        else
        {
            var hitLearnsheets = CoursesService.configManager.ClientCourses
                .FirstOrDefault(course => course.ID.ToString() == CoursesService.currentCourseID)
                ?.learnsheets
                .Where(learnsheet => learnsheet.Name.ToLower().StartsWith(value.ToLower()))
                ?? Enumerable.Empty<Learnsheet>();

            CoursesService.Learnsheets.Clear();
            foreach (var learnsheet in hitLearnsheets)
                CoursesService.Learnsheets.Add(learnsheet);
        }
    }

    [RelayCommand]
    private void CreateLearnsheet()
    {
        var newGuid = Guid.NewGuid();

        var newLearnsheet = new Learnsheet
        {
            ID = newGuid,
            Description = LocalizationService.Instance["learn_sheet_blank_description"],
            Name = LocalizationService.Instance["learn_sheet_blank_name"],
            TestDate = LocalizationService.Instance["learn_sheet_blank_test_date"]
        };

        CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID)
            ?.learnsheets.Add(newLearnsheet);
        CoursesService.SaveConfig();
        CoursesService.Instance.LoadLearnsheets(CoursesService.currentCourseID);

        string newLearnsheetFolder = Path.Combine(FileSystemPaths.coursesDirectoryPath, CoursesService.currentCourseID, newGuid.ToString());
        Directory.CreateDirectory(newLearnsheetFolder);

        AppStateService.CurrentLearningsheet = LocalizationService.Instance["learn_sheet_blank_name"];
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
        var course = CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID);
        var configEntry = course?.learnsheets.FirstOrDefault(learnsheet => learnsheet.ID == item.ID);

        if (configEntry != null)
        {
            var path = Path.Combine(FileSystemPaths.coursesDirectoryPath, CoursesService.currentCourseID, item.ID.ToString());
            string chatlogFileName = item.ID + ".json";
            var chatlogPath = Path.Combine(FileSystemPaths.chatlogDirectoryPath, CoursesService.currentCourseID, chatlogFileName);
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                if (File.Exists(chatlogPath))
                    File.Delete(chatlogPath);
                course!.learnsheets.Remove(configEntry);
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

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var item in SelectableLearnsheets)
            item.IsSelected = false;
    }

    [RelayCommand]
    private void GenerateExam()
    {
        var selectedSheets = SelectableLearnsheets
            .Where(i => i.IsSelected)
            .Select(i => i.Learnsheet)
            .ToList();

        if (selectedSheets.Count == 0)
            return;

        var course = CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID);

        AppStateService.ShowGenerateExamOverlay(selectedSheets, course);
    }
}