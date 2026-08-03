using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class FileEntryViewModel : ObservableObject
{
    public string FileName { get; }
    public string DisplayText { get; }

    public FileEntryViewModel(string fileName, int pageCount)
    {
        FileName = fileName;
        var shortFileName = Path.GetFileName(fileName);
        DisplayText = $"{shortFileName} ~ {pageCount} {LocalizationService.Instance["file_picker_pages_text"]}";
    }
}