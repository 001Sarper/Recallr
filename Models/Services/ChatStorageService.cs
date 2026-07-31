using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Recallr.Models.Models;

namespace Recallr.Models.Services;

public class ChatStorageService
{
    private static readonly Lazy<ChatStorageService> _instance = new(() => new ChatStorageService());
    public static ChatStorageService Instance => _instance.Value;
    
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
        var dtoList = messages.Select(m => new ChatMessageDto
        {
            Sender = m.Sender,
            Text = m.Text
        });

        var json = JsonSerializer.Serialize(dtoList, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(GetFilePath(fachId, lernzettelId), json);
    }

    public async Task<List<ChatEntry>> LoadMessagesAsync(string fachId, string lernzettelId)
    {
        var path = GetFilePath(fachId, lernzettelId);

        if (!File.Exists(path))
            return new List<ChatEntry>();

        var json = await File.ReadAllTextAsync(path);
        var dtoList = JsonSerializer.Deserialize<List<ChatMessageDto>>(json) ?? new();

        return dtoList.Select(dto => new ChatEntry
        {
            Sender = dto.Sender,
            Text = dto.Text
        }).ToList();
    }
}

public class ChatMessageDto
{
    public ChatSender Sender { get; set; }
    public string Text { get; set; } = string.Empty;
}