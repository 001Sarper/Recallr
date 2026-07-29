using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Recallr.Models.Services.Interfaces;

namespace Recallr.Models.Services;

public class FilePickerService : IFilePickerService
{
    private readonly Func<TopLevel?> _topLevelProvider;

    public FilePickerService(Func<TopLevel?> topLevelProvider)
    {
        _topLevelProvider = topLevelProvider;
    }

    public async Task<IReadOnlyList<string>> PickFilesAsync(string title, bool allowMultiple, IReadOnlyList<FilePickerFileType>? filters = null)
    {
        var topLevel = _topLevelProvider();
        if (topLevel is null)
            return Array.Empty<string>();

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
            FileTypeFilter = filters
        });

        return files.Select(f => f.Path.LocalPath).ToList();
    }
}