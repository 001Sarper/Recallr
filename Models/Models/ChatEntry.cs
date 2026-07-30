using CommunityToolkit.Mvvm.ComponentModel;

namespace Recallr.Models.Models;

public partial class ChatEntry : ObservableObject
{
    public ChatSender Sender { get; init; }

    [ObservableProperty]
    private string _text = string.Empty;

    // optional: praktisch für Streaming
    public void AppendToken(string token)
    {
        Text += token;
    }
}