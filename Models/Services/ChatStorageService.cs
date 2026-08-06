using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using Recallr.Models.Models;

namespace Recallr.Models.Services;

public class ChatStorageService
{
    private static readonly Lazy<ChatStorageService> _instance = new(() => new ChatStorageService());
    public static ChatStorageService Instance => _instance.Value;
    
    private static AppStateService AppState => AppStateService.Instance;
    
    
    private ChatStorageService() { }
    
    private static string GetFilePath(string fachId, string lernzettelId)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Recallr", "ChatLogs", fachId);

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, $"{lernzettelId}.json");
    }

    public async Task SaveMessagesAsync(string fachId, string lernzettelId, IEnumerable<ChatEntry> messages)
    {
        try
        {
            var dtoList = messages.Select(m => new ChatMessageDto
            {
                Sender = m.Sender,
                Topic = m.Topic,
                Text = m.Text,
                ResponseGenerated =  m.ResponseGenerated,
                MultipleChoice = m.MultipleChoice,
                Answers = m.Answers,
            });

            var json = JsonSerializer.Serialize(dtoList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(GetFilePath(fachId, lernzettelId), json);
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
    }

    public async Task ResetMessagesAsync(string fachId, string lernzettelId)
    {
        try
        {
            await File.WriteAllTextAsync(GetFilePath(fachId, lernzettelId), "[]");
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
    }

    public async Task<List<ChatEntry>> LoadMessagesAsync(string fachId, string lernzettelId)
    {
        try
        {
            var path = GetFilePath(fachId, lernzettelId);

            if (!File.Exists(path))
                return new List<ChatEntry>();

            var json = await File.ReadAllTextAsync(path);
            var dtoList = JsonSerializer.Deserialize<List<ChatMessageDto>>(json) ?? new();

            return dtoList.Select(dto => new ChatEntry
            {
                Sender = dto.Sender,
                Topic = dto.Topic,
                Text = dto.Text,
                ResponseGenerated = dto.ResponseGenerated,
                MultipleChoice = dto.MultipleChoice,
                Answers = dto.Answers,
            }).ToList();
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
            return new List<ChatEntry>();
        }
        
        
    }
}

public class ChatMessageDto
{
    public ChatSender Sender { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Topic {get; set;} = string.Empty;
    public bool ResponseGenerated { get; set; }
    public bool MultipleChoice { get; set; } = false;
    public List<string> Answers { get; set; } = new List<string>();
}