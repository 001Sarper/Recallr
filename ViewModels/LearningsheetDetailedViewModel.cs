using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfSharp.Pdf.IO;
using Recallr.Models.Models;
using Recallr.Models.Services;
using Recallr.Models.Services.Interfaces;

namespace Recallr.ViewModels;

public partial class LearningsheetDetailedViewModel : ViewModelBase
{
    //Graph Variables
    [ObservableProperty] private double[] _values1  = [2, 1, 3, 5, 3, 4, 6];
    [ObservableProperty] private double[] _values2  = [4, 2, 5, 2, 4, 5, 3];
    
    [ObservableProperty] private double[] _values3  = [20, 50, 40, 20, 40, 30, 50, 20, 50, 40];
    [ObservableProperty] private double[] _values4  = [3, 10, 5, 3, 7, 3, 8];

    
    
    [ObservableProperty] private PieData[] _data  = [
        new("Mary", 10),
        new("John", 20),
        new("Alice", 30),
        new("Bob", 40),
        new("Charlie", 50)
    ];
    
    [ObservableProperty] private double _value = 30;
    
    
    //Important Variables thar are necessary
    [ObservableProperty] private bool _isFilesSelected = true;
    [ObservableProperty] private bool _isSummarySelected = false;
    [ObservableProperty] private bool _isChatSelected = false;
    [ObservableProperty] private bool _isKnownledgeSelected = false;

    [ObservableProperty] private string _learnsheetContent;
    
    [ObservableProperty] private ObservableCollection<Border> _chatlog = new();
    
    [ObservableProperty] private string _currentInput = string.Empty;

    private readonly IFilePickerService _filePickerService;
    public ObservableCollection<FileEntryViewModel> Files { get; } = new();
    private static string _coursesDirectoryPath;
    private string _learningsheetID;
    private string _currentLearningsheetFolder;
    private List<string> _currentLearningsheetFolderFiles;
    
    public CoursesService CoursesService => CoursesService.Instance;


    public ObservableCollection<ChatEntry> Messages { get; } = new();
    
    

    public LearningsheetDetailedViewModel(string learningsheetID)
    {
        _filePickerService = new FilePickerService(() =>
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
        _coursesDirectoryPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Courses");
        
        _learningsheetID = learningsheetID;
        PickFilesCommand = new AsyncRelayCommand(PickFilesAsync);

        _currentLearningsheetFolder = Path.Combine(_coursesDirectoryPath, CoursesService.currentCourseID, _learningsheetID);
        _currentLearningsheetFolderFiles = Directory.GetFiles(_currentLearningsheetFolder).ToList();
        _currentLearningsheetFolderFiles.Remove(Path.Combine(_currentLearningsheetFolder, "learnsheet.txt"));

        foreach (var file in _currentLearningsheetFolderFiles)
        {
            int pageCount = (file.Contains(".pdf") ? getDocumentPageCount(file) : 1);
            Files.Add(new FileEntryViewModel(Path.GetFileName(file), pageCount));
        }

        var learnsheetTextFile = Path.Combine(_currentLearningsheetFolder, "learnsheet.txt");
        if (Path.Exists(learnsheetTextFile))
        {
            LearnsheetContent = File.ReadAllText(learnsheetTextFile);
        }

        InitializeAsync();

    }
    
    public async Task InitializeAsync()
    {
        var savedMessages = await ChatStorageService.Instance.LoadMessagesAsync(CoursesService.currentCourseID, _learningsheetID);

        foreach (var msg in savedMessages)
            Messages.Add(msg);
    }
    
    [RelayCommand]
    private void DeleteFile(string fileName)
    {
        var item = Files.FirstOrDefault(f => f.FileName == fileName);
        if (item != null)
            Files.Remove(item);
        Console.WriteLine(Path.Combine(_currentLearningsheetFolder, fileName));
        File.Delete(Path.Combine(_currentLearningsheetFolder, fileName));
    }

    public ICommand PickFilesCommand { get; }
    private async Task PickFilesAsync()
    {
        var filters = new[]
        {
            new FilePickerFileType("Dateien")
            {
                Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.pdf", "*.webp" }
            }
        };

        var paths = await _filePickerService.PickFilesAsync("Foto auswählen", true, filters);

        foreach (var path in paths)
        {
            string newFilePath = Path.Combine(_currentLearningsheetFolder, Path.GetFileName(path));
            if (!Path.Exists(newFilePath))
            {
                Files.Add(new FileEntryViewModel(path, getDocumentPageCount(path)));
                File.Copy(path, newFilePath, true);
            }
        }
    }

    [RelayCommand]
    private async Task CreateLearningsheetAsync()
    {
        var learnsheetReponse = await AIService.Instance.CreateLearningsheetAsync(_currentLearningsheetFolderFiles);
        var learnsheetTextFile = Path.Combine(_currentLearningsheetFolder, "learnsheet.txt");
        File.WriteAllText(learnsheetTextFile, learnsheetReponse);
        LearnsheetContent = learnsheetReponse;
    }

    private int getDocumentPageCount(string path)
    {
        using (PdfSharp.Pdf.PdfDocument document = PdfReader.Open(path, PdfDocumentOpenMode.InformationOnly))
        {
            return document.PageCount;
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


}

public class PieData(string name, double value)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];
}