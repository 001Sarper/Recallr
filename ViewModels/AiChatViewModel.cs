using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Models.Configuration;
using Recallr.Models.Models;
using Recallr.Models.Services;
using Recallr.Models.Services.Interfaces;

namespace Recallr.ViewModels;

public partial class AiChatViewModel : ViewModelBase
{
    private readonly IFilePickerService _filePickerService;
    private static AIService AI => AIService.Instance;

    public ObservableCollection<ChatEntry> Messages { get; } = new();

    [ObservableProperty] private string _currentInput = string.Empty;

    public ObservableCollection<string> AttachedFiles { get; } = new();

    [ObservableProperty]
    private ObservableCollection<ChatSession> _sessions = new();

    [ObservableProperty]
    private ChatSession _selectedSession;

    public AiChatViewModel()
    {
        _filePickerService = new FilePickerService(() =>
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        LoadSessions();
    }

    private void LoadSessions()
    {
        var configSessions = CoursesService.configManager?.ChatSessions;
        if (configSessions != null)
        {
            Sessions = new ObservableCollection<ChatSession>(configSessions.OrderByDescending(s => s.CreatedAt));
        }

        if (Sessions.Count == 0)
        {
            CreateNewSession();
        }
        else
        {
            SelectedSession = Sessions.First();
        }
    }

    private void SaveSessions()
    {
        if (CoursesService.configManager != null)
        {
            CoursesService.configManager.ChatSessions = Sessions.ToList();
            CoursesService.SaveConfig();
        }
    }

    partial void OnSelectedSessionChanged(ChatSession value)
    {
        Messages.Clear();
        if (value != null)
        {
            foreach (var msg in value.Messages)
            {
                Messages.Add(msg);
            }
        }
    }

    [RelayCommand]
    private void NewChat()
    {
        CreateNewSession();
    }

    private void CreateNewSession()
    {
        var newSession = new ChatSession
        {
            Title = "Neuer Chat",
            CreatedAt = DateTime.Now
        };

        // Optional: initial greeting
        newSession.Messages.Add(new ChatEntry
        {
            Sender = ChatSender.Ai,
            Text = "Hi! Ich bin der Recallr AI Tutor. Wie kann ich dir heute beim Lernen helfen?",
            ResponseGenerated = true
        });

        Sessions.Insert(0, newSession);
        SelectedSession = newSession;
        SaveSessions();
    }

    [RelayCommand]
    private void DeleteSession(ChatSession session)
    {
        if (session != null && Sessions.Contains(session))
        {
            Sessions.Remove(session);
            if (SelectedSession == session)
            {
                SelectedSession = Sessions.FirstOrDefault();
                if (SelectedSession == null)
                {
                    CreateNewSession();
                }
            }
            SaveSessions();
        }
    }

    [RelayCommand]
    private async Task AttachFileAsync()
    {
        var files = await _filePickerService.PickFilesAsync("Dateien anhängen", allowMultiple: true);
        if (files != null)
        {
            foreach (var file in files)
            {
                if (!AttachedFiles.Contains(file))
                    AttachedFiles.Add(file);
            }
        }
    }

    [RelayCommand]
    private void RemoveAttachedFile(string file)
    {
        if (AttachedFiles.Contains(file))
            AttachedFiles.Remove(file);
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (SelectedSession == null) return;
        if (string.IsNullOrWhiteSpace(CurrentInput) && !AttachedFiles.Any())
            return;

        var userMessage = new ChatEntry
        {
            Sender = ChatSender.User,
            Text = CurrentInput,
            ResponseGenerated = true,
            AttachedFiles = AttachedFiles.ToList() // Snapshot
        };

        Messages.Add(userMessage);
        SelectedSession.Messages.Add(userMessage);

        var historyForAi = SelectedSession.Messages.ToList(); // Capture before adding the pending AI message

        // Generate Title if this is the first user message
        if (SelectedSession.Messages.Count(m => m.Sender == ChatSender.User) == 1)
        {
            SelectedSession.Title = string.IsNullOrWhiteSpace(CurrentInput) ? "Dateianhang" : 
                (CurrentInput.Length > 25 ? CurrentInput.Substring(0, 25) + "..." : CurrentInput);
            
            // Trigger UI update for Title
            var idx = Sessions.IndexOf(SelectedSession);
            if (idx >= 0)
            {
                Sessions[idx] = SelectedSession;
                SelectedSession = Sessions[idx];
            }
        }

        CurrentInput = string.Empty;
        AttachedFiles.Clear();
        SaveSessions();

        var aiMessage = new ChatEntry
        {
            Sender = ChatSender.Ai,
            Text = string.Empty,
            ResponseGenerated = false
        };
        Messages.Add(aiMessage);
        SelectedSession.Messages.Add(aiMessage);

        await foreach (var chunk in AI.GetGeneralChatResponseStreamAsync(historyForAi))
        {
            if (!aiMessage.ResponseGenerated)
                aiMessage.ResponseGenerated = true;
            
            aiMessage.Text += chunk;
        }
        
        if (!aiMessage.ResponseGenerated)
            aiMessage.ResponseGenerated = true;
            
        SaveSessions();
    }
}
