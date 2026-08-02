using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recallr.Models.Services;

public partial class LocalizationService : ObservableObject
{
    public static LocalizationService Instance { get; } = new();
    private Dictionary<string, string> _translations = new();

    public void SetLanguage(string code)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Language", $"{code}.json");
        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
        OnPropertyChanged("Item[]");
    }

    public string this[string key] =>
        _translations.TryGetValue(key, out var val) ? val : key;
}