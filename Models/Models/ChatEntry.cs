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
        if (response.type == "question")
        {
            Text = response.question;
            if (response.multipleChoice)
            {
                MultipleChoice = response.multipleChoice;
                Answers = response.answers ?? new();
            }
        } else if (response.type == "feedback")
        {
            Text = (response.correct) ? "✔️" : "❌";
            Text += $"\n{response.explanation}";
        }
    }
}