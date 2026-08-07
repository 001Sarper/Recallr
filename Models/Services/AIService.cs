using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using Recallr.Models.Configuration;
using Recallr.Models.Models;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace Recallr.Models.Services;

public partial class AIService : ObservableObject
{
    public static AIService Instance { get; } = new AIService();
    private static SettingsService Settings => SettingsService.Instance;
    private static AppStateService AppState => AppStateService.Instance;

    private static ObservableCollection<string> modelsCollection = new();

    private static ChatClient _chatClient;
    private static OpenAIClient _openAiClient;

    private static void InitialiazeClient()
    {
        try
        {
            _chatClient = new(model: Settings.AiModel, apiKey: Settings.OpenaiKey);
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
    }

    private static void InitialiazeOpenAiClient()
    {
        try
        {
            _openAiClient = new OpenAIClient(apiKey: Settings.OpenaiKey);
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
        }
    }


#pragma warning disable OPENAI001
    private ChatMessageContentPart CreateContentPartForFile(string path)
    {
        try
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
        catch (Exception ex)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"], ex.Message, Brushes.Red);
            return ChatMessageContentPart.CreateTextPart(ex.Message);
        }
    }
    private static readonly ChatCompletionOptions LearnsheetOptions = new ()
    {
        ReasoningEffortLevel = ChatReasoningEffortLevel.High
    };
    

    public async Task<string> CreateLearningsheetAsync(List<string> paths)
    {
        try
        {
            var languageName = Settings.Language switch
            {
                0 => "Deutsch",
                1 => "Englisch",
                _ => "Englisch"
            };

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
                new SystemChatMessage(SystemPromptBuilderService.BuildLearnsheet(languageName, Settings.SummaryStyle)),
                new UserChatMessage(contentParts)
            ];

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, LearnsheetOptions);
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
            return string.Empty;
        }
    }

    public async Task<ObservableCollection<string>> GetOpenAiModels()
    {
        try
        {
            if (!modelsCollection.Any(model => model.Contains("gpt")))
            {
                InitialiazeOpenAiClient();

                OpenAIModelClient modelClient = _openAiClient.GetOpenAIModelClient();
                var models = await modelClient.GetModelsAsync();

                foreach (var model in models.Value.OrderBy(m => m.Id))
                {
                    modelsCollection.Add(model.Id);
                }
            }

            return modelsCollection;
        }
        catch (Exception e)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{e.Message}", Brushes.Red);
            return modelsCollection;
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
                new UserChatMessage("Hier ist der Lernzettel \n" + learnsheet)
            ];

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
        }
    }

    // Einmal definieren (z.B. als static readonly Feld der Klasse), nicht bei jedem Call neu bauen
    private static readonly ChatCompletionOptions ChatOptions = new()
    {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "recallr_chat_response",
            jsonSchema: BinaryData.FromString("""
                                              {
                                                "type": "object",
                                                "properties": {
                                                  "type": { "type": "string", "enum": ["question", "feedback", "message"] },
                                                  "topic": {"type": ["string", "null"] },
                                                  "question": { "type": ["string", "null"] },
                                                  "multipleChoice": { "type": ["boolean", "null"] },
                                                  "answers": { "type": ["array", "null"], "items": { "type": "string" } },
                                                  "correct": { "type": ["boolean", "null"] },
                                                  "explanation": { "type": ["string", "null"] },
                                                  "message": { "type": ["string", "null"] },
                                                  "nextQuestion": {
                                                    "type": ["object", "null"],
                                                    "properties": {
                                                      "question": { "type": "string" },
                                                      "multipleChoice": { "type": "boolean" },
                                                      "answers": { "type": ["array", "null"], "items": { "type": "string" } },
                                                      "topic": { "type": "string" }
                                                    },
                                                    "required": ["question", "multipleChoice", "answers", "topic"],
                                                    "additionalProperties": false
                                                  }
                                                },
                                                "required": ["type", "topic", "question", "multipleChoice", "answers", "correct", "explanation", "message", "nextQuestion"],
                                                "additionalProperties": false
                                              }
                                              """),
            jsonSchemaFormatDescription: "Antwort des KI-Lernbuddys: entweder eine Frage, Feedback oder eine normale Nachricht",
            jsonSchemaIsStrict: true),
        ReasoningEffortLevel = ChatReasoningEffortLevel.High,
        MaxOutputTokenCount = 2000 // JSON-Antworten sind klein, harte Obergrenze gegen abgeschnittenes JSON
    };

    public async Task<AIResponse?> GetResponseAsync(
        IEnumerable<ChatEntry> conversationHistory,
        string lernzettelContent)
    {
        IAsyncEnumerator<StreamingChatCompletionUpdate> enumerator = null;
        try
        {
            var languageName = Settings.Language switch
            {
                0 => "Deutsch",
                1 => "Englisch",
                _ => "Englisch"
            };
            InitialiazeClient();
            var systemPrompt = SystemPromptBuilderService.BuildChat(
                conversationHistory,
                languageName,
                lernzettelContent: lernzettelContent,
                difficulty: SettingsService.Instance.Difficulty
            );
            var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };
            messages.AddRange(conversationHistory.Select(m => m.Sender == ChatSender.User
                ? (ChatMessage)new UserChatMessage(m.Text)
                : new AssistantChatMessage(m.Text)));

            enumerator = _chatClient.CompleteChatStreamingAsync(messages, ChatOptions).GetAsyncEnumerator();
        }
        catch (Exception ex)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{ex.Message}", Brushes.Red);
            return null;
        }

        var buffer = new StringBuilder();
        var refusal = new StringBuilder();
        try
        {
            while (true)
            {
                StreamingChatCompletionUpdate update;
                try
                {
                    if (!await enumerator.MoveNextAsync())
                        break;
                    update = enumerator.Current;
                }
                catch (Exception ex)
                {
                    AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                        LocalizationService.Instance["general_error_message"] + $"\n{ex.Message}", Brushes.Red);
                    return null;
                }

                foreach (var part in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                        buffer.Append(part.Text);
                }

                // Modell kann bei strict-Schema statt Inhalt eine Ablehnung streamen (Safety-Refusal)
                foreach (var part in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(part.Refusal))
                        refusal.Append(part.Refusal);
                }
            }
        }
        finally
        {
            if (enumerator != null)
                await enumerator.DisposeAsync();
        }

        if (refusal.Length > 0)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                refusal.ToString(), Brushes.Red);
            return null;
        }

        var fullResponse = buffer.ToString();
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<AIResponse>(fullResponse, options);
        }
        catch (JsonException ex)
        {
            AppState.ShowMessageOverlay(LocalizationService.Instance["errormessage_title"],
                LocalizationService.Instance["general_error_message"] + $"\n{ex.Message}", Brushes.Red);
            return null;
        }
    }
}