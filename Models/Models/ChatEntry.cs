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

    public List<string> AttachedFiles { get; set; } = new();

    public System.Collections.ObjectModel.ObservableCollection<MessagePart> Parts { get; } = new();

    partial void OnTextChanged(string value)
    {
        ParseTextToParts(value);
    }

    private void ParseTextToParts(string input)
    {
        // To avoid flickering, only update parts if they actually change structurally,
        // but for simplicity and streaming we can just rebuild the collection.
        // Avalonia ItemsControl handles simple rebuilds reasonably well.
        var newParts = new List<MessagePart>();
        
        if (string.IsNullOrEmpty(input))
        {
            Parts.Clear();
            return;
        }

        int currentIndex = 0;
        
        while (currentIndex < input.Length)
        {
            int blockMathStart = input.IndexOf("\\[", currentIndex);
            int blockMathStart2 = input.IndexOf("$$", currentIndex);
            
            // Find the earliest math start
            int mathStart = -1;
            string startDelimiter = "";
            string endDelimiter = "";
            
            if (blockMathStart != -1 && (blockMathStart2 == -1 || blockMathStart < blockMathStart2))
            {
                mathStart = blockMathStart;
                startDelimiter = "\\[";
                endDelimiter = "\\]";
            }
            else if (blockMathStart2 != -1)
            {
                mathStart = blockMathStart2;
                startDelimiter = "$$";
                endDelimiter = "$$";
            }

            if (mathStart == -1)
            {
                // No more math, add remaining text
                string textContent = CleanInlineMath(input.Substring(currentIndex));
                newParts.Add(new TextPart { Content = textContent });
                break;
            }

            // Add text before math
            if (mathStart > currentIndex)
            {
                string textContent = CleanInlineMath(input.Substring(currentIndex, mathStart - currentIndex));
                newParts.Add(new TextPart { Content = textContent });
            }

            int mathEnd = input.IndexOf(endDelimiter, mathStart + startDelimiter.Length);
            
            if (mathEnd == -1)
            {
                // Unclosed math, treat as math for now
                string mathContent = input.Substring(mathStart + startDelimiter.Length);
                newParts.Add(new MathPart { LatexCode = CleanLatex(mathContent) });
                break;
            }
            else
            {
                string mathContent = input.Substring(mathStart + startDelimiter.Length, mathEnd - (mathStart + startDelimiter.Length));
                newParts.Add(new MathPart { LatexCode = CleanLatex(mathContent) });
                currentIndex = mathEnd + endDelimiter.Length;
            }
        }
        
        // Sync newParts to Parts
        Parts.Clear();
        foreach (var p in newParts)
        {
            Parts.Add(p);
        }
    }

    private string CleanInlineMath(string input)
    {
        // Converts simple inline math like \( x \) or $ y $ to readable typography
        if (string.IsNullOrEmpty(input)) return input;
        
        return input.Replace("\\(", "")
                    .Replace("\\)", "")
                    .Replace("$", "");
    }

    private string CleanLatex(string input)
    {
        // Advanced cleanup for CSharpMath compatibility
        if (string.IsNullOrWhiteSpace(input)) return " ";
        
        var cleaned = input.Trim()
            .Replace("\\\\", "\\")
            .Replace("\n", " ")
            .Replace("\\bigl(", "\\left(")
            .Replace("\\Bigl(", "\\left(")
            .Replace("\\biggl(", "\\left(")
            .Replace("\\bigr)", "\\right)")
            .Replace("\\Bigr)", "\\right)")
            .Replace("\\biggr)", "\\right)")
            .Replace("\\bigl[", "\\left[")
            .Replace("\\Bigl[", "\\left[")
            .Replace("\\biggl[", "\\left[")
            .Replace("\\bigr]", "\\right]")
            .Replace("\\Bigr]", "\\right]")
            .Replace("\\biggr]", "\\right]")
            .Replace("\\quad", " ")
            .Replace("\\qquad", "  ");

        // Validate if CSharpMath can parse it. If not, we could fall back, but returning cleaned is usually enough.
        // We will catch exceptions in SafeMathView anyway.
        return cleaned;
    }

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