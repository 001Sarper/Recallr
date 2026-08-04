using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Media;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recallr.Models.Services;

public partial class LocalizationService : ObservableObject
{
    public static LocalizationService Instance { get; } = new();
    private Dictionary<string, string> _translations = new();

    private static AppStateService AppState => AppStateService.Instance;

    public void SetLanguage(string code)
    {
        try
        {
            var uri = new Uri($"avares://Recallr/Assets/Language/{code}.json");
            using var stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())!;

            OnPropertyChanged("Item");
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
    }

    public string this[string key] =>
        _translations.TryGetValue(key, out var val) ? val : key;
}