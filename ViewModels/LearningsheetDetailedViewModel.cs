using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Models;
using Recallr.Models.Services;
using Recallr.Models.Services.Interfaces;

namespace Recallr.ViewModels;

public partial class LearningsheetDetailedViewModel : ViewModelBase
{
    //Important Variables thar are necessary
    [ObservableProperty] private bool _isFilesSelected = true;
    [ObservableProperty] private bool _isSummarySelected = false;
    [ObservableProperty] private bool _isChatSelected = false;
    [ObservableProperty] private bool _isKnownledgeSelected = false;

    [ObservableProperty] private string _learnsheetContent = $"# {LocalizationService.Instance["no_learnsheet_yet"]}";

    [ObservableProperty] private ObservableCollection<Border> _chatlog = new();

    [ObservableProperty] private string _currentInput = string.Empty;
    
    [ObservableProperty] private string _testDate = string.Empty;

    [ObservableProperty] private bool _learnsheetCreating;

    private readonly IFilePickerService _filePickerService;
    public ObservableCollection<FileEntryViewModel> Files { get; } = new();
    private string _learningsheetID;
    private static string _currentLearningsheetFolder;
    private List<string> _currentLearningsheetFolderFiles;

    private Learnsheet _currentLearnsheet;
    
    public ObservableCollection<ChatEntry> Messages { get; } = new();

    public LearningsheetDetailedViewModel(string learningsheetID)
    {

        _filePickerService = new FilePickerService(() =>
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        _learningsheetID = learningsheetID;
        PickFilesCommand = new AsyncRelayCommand(PickFilesAsync);
        _currentLearningsheetFolder = Path.Combine(FileSystemPaths.coursesDirectoryPath, CoursesService.currentCourseID,
            _learningsheetID);

        _currentLearningsheetFolderFiles = Directory.GetFiles(_currentLearningsheetFolder).ToList();
        _currentLearningsheetFolderFiles.Remove(Path.Combine(_currentLearningsheetFolder, "learnsheet.md"));

        _currentLearnsheet = CoursesService.configManager.ClientCourses
            .FirstOrDefault(course => course.ID.ToString() == CoursesService.currentCourseID)
            .learnsheets.FirstOrDefault(learnsheet => learnsheet.ID.ToString() == _learningsheetID);

        if (!_currentLearnsheet.TestDate.Contains(LocalizationService.Instance["learn_sheet_blank_test_date"]))
        {
            TestDate = _currentLearnsheet.TestDate;
        }

        foreach (var file in _currentLearningsheetFolderFiles)
        {
            int pageCount = (file.Contains(".pdf") ? PdfService.GetDocumentPageCount(file) : 1);
            Files.Add(new FileEntryViewModel(Path.GetFileName(file), pageCount));
        }

        var learnsheetTextFile = Path.Combine(_currentLearningsheetFolder, "learnsheet.md");
        if (Path.Exists(learnsheetTextFile))
        {
            LearnsheetContent = File.ReadAllText(learnsheetTextFile);
        }

        InitializeAsync();

    }

    private async Task InitializeAsync()
    {
        var savedMessages =
            await ChatStorageService.Instance.LoadMessagesAsync(CoursesService.currentCourseID, _learningsheetID);

        foreach (var msg in savedMessages)
            Messages.Add(msg);
    }

    [RelayCommand]
    private void DeleteFile(string fileName)
    {
        var item = Files.FirstOrDefault(f => f.FileName == fileName);
        if (item != null)
            Files.Remove(item);
        string fullPath = Path.Combine(_currentLearningsheetFolder, fileName);
        File.Delete(fullPath);
        _currentLearningsheetFolderFiles.Remove(fullPath);
    }

    public ICommand PickFilesCommand { get; }

    private async Task PickFilesAsync()
    {
        var filters = new[]
        {
            new FilePickerFileType(LocalizationService.Instance["file_picker_filetype"])
            {
                Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.pdf", "*.webp" }
            }
        };

        var paths = await _filePickerService.PickFilesAsync(LocalizationService.Instance["file_picker_title"], true, filters);

        foreach (var path in paths)
        {
            string newFilePath = Path.Combine(_currentLearningsheetFolder, Path.GetFileName(path));
            if (!Path.Exists(newFilePath))
            {
                Files.Add(new FileEntryViewModel(path, PdfService.GetDocumentPageCount(path)));
                File.Copy(path, newFilePath, true);
                _currentLearningsheetFolderFiles.Add(newFilePath);
            }
        }
    }

    [RelayCommand]
    private async void CreateLearningsheetAsync()
    {
        if (!LearnsheetCreating)
        {
            await StartLearnsheetCreationAsync();
        }
    }

    private async Task StartLearnsheetCreationAsync()
    {
        LearnsheetCreating = true;
        
        var learnsheetReponse = await AIService.Instance.CreateLearningsheetAsync(_currentLearningsheetFolderFiles);
        CoursesService.SaveConfig();
        var learnsheetTextFile = Path.Combine(_currentLearningsheetFolder, "learnsheet.md");
        File.WriteAllText(learnsheetTextFile, learnsheetReponse);
        LearnsheetContent = learnsheetReponse;

        var learnsheetMetaData = await AIService.Instance.CreateLearnsheetMetaData(learnsheetReponse);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var meta = JsonSerializer.Deserialize<LearnsheetMetaData>(learnsheetMetaData, options);

        if (meta == null)
        {
            return;
        }

        var course = CoursesService.configManager.ClientCourses
            .FirstOrDefault(c => c.ID.ToString() == CoursesService.currentCourseID);

        var learnsheet = course?.learnsheets
            .FirstOrDefault(ls => ls.ID.ToString() == _learningsheetID);

        if (learnsheet != null)
        {
            learnsheet.Name = meta.Title;
            AppStateService.Instance.CurrentLearningsheet = meta.Title;
            learnsheet.Description = meta.Description;
            CoursesService.SaveConfig();
            LearnsheetCreating = false;
        }
    }

    public async Task SendMessageAsync(string userInput)
    {
        Messages.Add(new ChatEntry { Sender = ChatSender.User, Text = userInput });

        var aiMessage = new ChatEntry() { Sender = ChatSender.Ai };
        Messages.Add(aiMessage);

        await foreach (var token in AIService.Instance.StreamResponseAsync(Messages.SkipLast(1), LearnsheetContent))
        {
            await Dispatcher.UIThread.InvokeAsync(() => aiMessage.AppendToken(token));
        }

        await ChatStorageService.Instance.SaveMessagesAsync(CoursesService.currentCourseID, _learningsheetID, Messages);
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(CurrentInput)) return;

        var input = CurrentInput;
        CurrentInput = string.Empty; // TextBox sofort leeren

        await SendMessageAsync(input);
    }
    
    [RelayCommand]
    private void ExportAsPdf()
    {
        if (LearnsheetContent != string.Empty)
        {
            string exportLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads", "learnsheet.pdf");
            
            PdfService.ExportMarkdownAsPdf(LearnsheetContent, exportLocation);
        }
    }

    [RelayCommand]
    private void SaveTestDate()
    {
        _currentLearnsheet.TestDate = TestDate;
        CoursesService.SaveConfig();
    }

}