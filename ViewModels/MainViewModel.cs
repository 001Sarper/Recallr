using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Recallr.Views;
using Avalonia.Platform.Storage;
using OpenAI.Chat;
using Recallr.Models.Services;

namespace Recallr.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public SettingsService Settings => SettingsService.Instance;
    public OverlayService Overlay => OverlayService.Instance;
    
    [ObservableProperty] private string _lernzettel = "";

    private readonly Window _window;
    
    [ObservableProperty] private string _currentOption = "Meine Fächer";
    [ObservableProperty] private string _currentCourse = "";
    [ObservableProperty] private string _currentLearningsheet = "";
    
    [ObservableProperty] private bool _optionArrow1 = false;
    [ObservableProperty] private bool _optionArrow2 = false;
    
    [ObservableProperty] private string _profileName = "";
    [ObservableProperty] private string _profileMail = "";
    
    [ObservableProperty]
    private int _selectedNavIndex = 0;
    

    [ObservableProperty] private List<string> _paths = new();

    [ObservableProperty] private bool _isLoading = false;

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

    private readonly ChatClient _chatClient =
        new(model: "gpt-5.4-mini", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    public MainViewModel(Window window)
    {
        _window = window;
        Overlay.CurrentPageViewModel = new CourseViewModel();
        App.Instance.SetTheme(Settings.Theme); 
    }
    

    [RelayCommand]
    private async Task PickFileAsync()
    {
        var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Foto auswählen",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Dateien") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.pdf", "*.webp" } }
            }
        });

        if (files.Count >= 1)
        {
            Paths = files.Select(f => f.Path.LocalPath).ToList();
        }
    }

    partial void OnSelectedNavIndexChanged(int selectedNavIndex)
    {
        switch (selectedNavIndex)
        {
            case 0:
                ClearBreadcrumbs();
                CurrentOption = "Meine Fächer";
                if(OverlayService.Instance.IsOverlayVisible) OverlayService.Instance.CloseOverlay();
                Overlay.ShowCourseView();
                break;
            case 1:
                ClearBreadcrumbs();
                CurrentOption = "Einstellungen";
                if(OverlayService.Instance.IsOverlayVisible) OverlayService.Instance.CloseOverlay();
                Overlay.ShowSettingsView();
                break;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        
    }

    public void ClearBreadcrumbs()
    {
        CurrentOption = "";
        CurrentCourse = "";
        CurrentLearningsheet = "";
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


    [RelayCommand]
    private async Task CreateLearningsheetAsync()
    {
        if (Paths.Count == 0) return;

        IsLoading = true;
        try
        {
            var contentParts = new List<ChatMessageContentPart>
            {
                ChatMessageContentPart.CreateTextPart(
                    "Fasse den Inhalt dieser Fotos gemeinsam als einen zusammenhängenden Lernzettel zusammen.")
            };

            foreach (var path in Paths)
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
            Lernzettel = completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            Lernzettel = $"❌ Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
}