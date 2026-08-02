using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using Recallr.Models.Models;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace Recallr.Models.Services;

public partial class AIService : ObservableObject
{
    public static AIService Instance { get; } = new AIService();
    public static SettingsService Settings => SettingsService.Instance;

    private static ChatClient _chatClient;
    private static OpenAIClient _openAiClient;

    public static void InitialiazeClient()
    {
        _chatClient = new(model: Settings.AiModel, apiKey: Settings.OpenaiKey);
    }

    public static void InitialiazeOpenAiClient()
    {
        _openAiClient = new OpenAIClient(apiKey: Settings.OpenaiKey);
    }
    

#pragma warning disable OPENAI001
    private ChatMessageContentPart CreateContentPartForFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        BinaryData data = BinaryData.FromBytes(bytes);
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => ChatMessageContentPart.CreateFilePart(data, "application/pdf", Path.GetFileName(path)),
            ".png" => ChatMessageContentPart.CreateImagePart(data, "image/png"),
            ".jpg" or ".jpeg" => ChatMessageContentPart.CreateImagePart(data, "image/jpeg"),
            ".webp" => ChatMessageContentPart.CreateImagePart(data, "image/webp"),
            _ => throw new NotSupportedException($"Dateityp '{extension}' wird nicht unterstützt.")
        };
    }

    public async Task<string> CreateLearningsheetAsync(List<string> paths)
    {
        try
        {
            InitialiazeClient();
            var contentParts = new List<ChatMessageContentPart>
            {
                ChatMessageContentPart.CreateTextPart(
                    "Fasse den Inhalt dieser Fotos gemeinsam als einen zusammenhängenden Lernzettel zusammen.")
            };

            foreach (var path in paths)
            {
                contentParts.Add(CreateContentPartForFile(path));
            }

            List<ChatMessage> messages =
            [
                new SystemChatMessage(SystemPromptBuilderService.BuildLearnsheet(Settings.SummaryStyle)),
                new UserChatMessage(contentParts)
            ];

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
        }
    }

    public async Task<ObservableCollection<string>> GetOpenAiModels()
    {
        ObservableCollection<string> collection = new();

        try
        {
            InitialiazeOpenAiClient();

            OpenAIModelClient modelClient = _openAiClient.GetOpenAIModelClient();
            var models = await modelClient.GetModelsAsync();

            foreach (var model in models.Value.OrderBy(m => m.Id))
            {
                collection.Add(model.Id);
            }

            return collection;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            collection.Add("Modelle konten nicht geladen werden");
            return collection;
        }
    }

    public async Task<string> CreateLearnsheetMetaData(string learnsheet)
    {
        try
        {
            InitialiazeClient();
            List<ChatMessage> messages =
            [
                new SystemChatMessage(SystemPrompts.MetaDataSystemPrompt),
                new UserChatMessage("Hier ist der Lernzettel \n" +  learnsheet)
            ];
            
            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
        }
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<Recallr.Models.Models.ChatEntry> conversationHistory,
        string lernzettelContent)
    {
        InitialiazeClient();
        var systemPrompt = SystemPromptBuilderService.BuildChat(
            lernzettelContent: lernzettelContent,
            difficulty: SettingsService.Instance.Difficulty,
            questionType: SettingsService.Instance.QuestionType
        );

        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };

        messages.AddRange(conversationHistory.Select(m => m.Sender == ChatSender.User
            ? (ChatMessage)new UserChatMessage(m.Text)
            : new AssistantChatMessage(m.Text)));

        await foreach (StreamingChatCompletionUpdate update in
                       _chatClient.CompleteChatStreamingAsync(messages))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                    yield return part.Text;
            }
        }
    }
}