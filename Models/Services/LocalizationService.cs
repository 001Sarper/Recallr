using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recallr.Models.Services;

public partial class LocalizationService : ObservableObject
{
    public static LocalizationService Instance { get; } = new();
    private Dictionary<string, string> _translations = new();

    public void SetLanguage(string code)
    {
        var uri = new Uri($"avares://Recallr/Assets/Language/{code}.json");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())!;
    
        OnPropertyChanged("Item"); // <-- ohne eckige Klammern
    }

    public string this[string key] =>
        _translations.TryGetValue(key, out var val) ? val : key;
}