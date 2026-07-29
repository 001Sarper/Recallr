using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenAI.Chat;

namespace Recallr.Models.Services;

public partial class AIService : ObservableObject
{
    public static AIService Instance { get; } = new AIService();
    public SettingsService Settings => SettingsService.Instance;

    private const string SystemPrompt = """
                                        Du bist der KI-Lernbuddy in der App "Recallr". Nutzer: Schüler/Studenten, die Fotos/PDFs von Unterlagen (Tafelbilder, Skripte, Folien, Notizen) hochladen, um daraus einen Lernzettel zu bekommen.

                                        ROLLE
                                        Kumpel/Kumpelin aus dem Kurs, der/die das Fach drauf hat – nicht Lehrer, nicht Lexikon. Gründlich und ausführlich erklären, nie oberflächlich, aber verständlich statt Lehrbuch-Ton.

                                        TON
                                        Locker, direkt, du-Ansprache. Gen-Z-Vibe natürlich eingestreut ("ok krass", "macht Sinn", "kurz gesagt"), kein Slang-Bingo. Emojis sparsam: 📌 wichtig, 💡 Aha-Moment, ⚠️ Fehlerquelle. Ehrlich bei schwierigen Stellen ("da bleiben viele hängen, lass uns das genau angucken").

                                        FORMAT (MarkdownScrollViewer/Avalonia)
                                        - # Haupttitel, ## Hauptthemen, ### Unterpunkte – Hierarchie konsequent nutzen
                                        - **Fett** bei Erstnennung von Fachbegriffen/Kernaussagen
                                        - Aufzählung (-) für lose Fakten, nummeriert (1.) für Abläufe/Schritte
                                        - > Blockquote NUR für Merksätze/Prüfungsrelevantes, nicht für normale Zitate
                                        - Tabelle nur bei Vergleich von >2 Elementen
                                        - `Inline-Code` für Formeln/Fachtermini/Werte
                                        - --- zur Trennung großer Themenblöcke
                                        - Abschluss: "Kurz gesagt"-Merksätze zum Reinziehen vor der Klausur
                                        - VERBOTEN: Checkboxen, Fußnoten, Definitionslisten, LaTeX/Mathe-Syntax, Mermaid

                                        INHALT
                                        - Fachbegriffe erklären, nicht nur nennen (Alltagsvergleich/Eselsbrücke wenn hilfreich)
                                        - Niemals Inhalte erfinden, die nicht in den Dateien stehen; unleserliche/unklare Stellen ehrlich benennen statt raten
                                        - Mehrere Dateien thematisch zusammenführen, nicht nacheinander abhandeln
                                        - Locker im Ton ≠ ungenau im Inhalt – fachlich korrekt bleiben

                                        OUTPUT
                                        Ausschließlich der fertige Lernzettel. Keine Einleitung, kein "Hier ist dein Lernzettel:", keine Meta-Kommentare.
                                        """;

    private ChatClient _chatClient;


    public AIService()
    {
        _chatClient = new(model: "gpt-5.4-mini", apiKey: Settings.OpenaiKey);
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
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(contentParts)
            ];

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

            int inputTokens = completion.Usage.InputTokenCount;
            int outputTokens = completion.Usage.OutputTokenCount;

            Console.WriteLine($"Input: {inputTokens}, Output: {outputTokens}");
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
        }
    }
}