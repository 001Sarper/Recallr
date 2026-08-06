using System.Collections.Generic;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Recallr.Models.Configuration;

namespace Recallr.Models.Models;

public partial class ChatEntry : ObservableObject
{
    public ChatSender Sender { get; init; }

    // Bleibt roher JSON-String, damit die Conversation-History (m.Text) beim nächsten
    // Aufruf konsistent als AssistantChatMessage(m.Text) an das Modell zurückgeht.
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _responseGenerated = false;
    
    [ObservableProperty]
    private string _topic = string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private bool _correct;

    [ObservableProperty]
    private string _explanation = string.Empty;

    [ObservableProperty]
    private string _question = string.Empty;

    [ObservableProperty]
    private bool _multipleChoice;

    [ObservableProperty]
    private List<string> _answers = new();

    public void SetResponse(AIResponse response)
    {
        switch (response.Type)
        {
            case "question":
                ResponseGenerated = true;
                Text = response.Question ?? string.Empty;
                Topic = response.Topic ?? string.Empty;
                if (response.MultipleChoice == true)
                {
                    MultipleChoice = true;
                    Answers = response.Answers ?? new();
                }
                break;
            case "feedback":
                ResponseGenerated = true;
                Text = response.Explanation ?? string.Empty;
                Text += $"\n\n{response.NextQuestion?.Question}";
                if (response.NextQuestion?.MultipleChoice ?? false)
                {
                    MultipleChoice = true;
                    Topic = response.NextQuestion.Topic ?? string.Empty;
                    Answers = response.NextQuestion.Answers ?? new();
                }
                else
                {
                    Topic = response.Topic ?? string.Empty;
                }
                break;
            case "message":
                ResponseGenerated = true;
                Text = response.Message ?? string.Empty;
                break;
        }
    }
}