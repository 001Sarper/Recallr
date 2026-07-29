using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Recallr.Models.Services.Interfaces;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickFilesAsync(string title, bool allowMultiple, IReadOnlyList<FilePickerFileType>? filters = null);
}