using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recallr.ViewModels;

public partial class FileEntryViewModel : ObservableObject
{
    public string FileName { get; }
    public string DisplayText { get; }

    public FileEntryViewModel(string fileName, int pageCount)
    {
        FileName = fileName;
        var shortFileName = Path.GetFileName(fileName);
        DisplayText = $"{shortFileName} ~ {pageCount} Seiten erfasst";
    }
}