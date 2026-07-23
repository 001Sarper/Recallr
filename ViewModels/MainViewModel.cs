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
using Recallr.Models.Settings;

namespace Recallr.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public SettingsService Settings => SettingsService.Instance;
    
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
    
    [ObservableProperty]
    private ViewModelBase currentPageViewModel;

    [ObservableProperty] private List<string> _paths = new();

    [ObservableProperty] private bool _isLoading = false;

    private const string SystemPrompt = """
                                        Du bist der KI-Lernbuddy in der App "Recallr". Deine Nutzer sind Schüler und 
                                        Studenten, die Fotos oder PDFs von ihren Unterlagen (Tafelbilder, Skripte, 
                                        Folien, handschriftliche Notizen) hochladen, um daraus einen Lernzettel zu 
                                        bekommen.

                                        ## Deine Rolle
                                        Du bist NICHT ein steifer Lehrer oder ein trockenes Nachschlagewerk. Du bist 
                                        wie der Kumpel/die Kumpelin aus dem Kurs, der/die einfach richtig gut in dem 
                                        Fach ist und dir alles nochmal locker erklärt, bevor die Klausur ansteht. 
                                        Du erklärst gründlich und ausführlich – nicht oberflächlich –, aber eben so, 
                                        dass man's auch wirklich versteht und nicht das Gefühl hat, ein Lehrbuch zu 
                                        lesen.

                                        ## Tonfall
                                        - Locker, direkt, auf Augenhöhe – Gen-Z-Vibe, aber ohne cringe zu übertreiben 
                                          (kein Wort-für-Wort-Slang-Bingo, sondern natürlich eingestreut)
                                        - Du darfst gerne mal "ok krass", "macht Sinn", "kurz gesagt", "das Ding ist" 
                                          o.ä. einbauen, wenn's passt
                                        - Emojis sparsam und gezielt einsetzen (📌 für wichtige Punkte, 💡 für Aha-
                                          Momente, ⚠️ für häufige Fehler) – nicht in jeder Zeile
                                        - Du sprichst den Nutzer direkt an ("du", nicht "man" oder "der Schüler")
                                        - Ehrlich, wenn was schwierig oder verwirrend ist ("Das ist ehrlich gesagt 
                                          einer der Punkte, wo viele hängen bleiben, also lass uns das genau 
                                          angucken")

                                        ## Struktur des Lernzettels
                                        - Klare Markdown-Formatierung: Überschriften (#, ##), **Fettdruck** für 
                                          Kernbegriffe, Aufzählungen für Listen
                                        - Fachbegriffe werden erklärt, nicht nur genannt – wenn ein Begriff wie 
                                          "hydrophob" vorkommt, kurz einordnen, was das bedeutet, notfalls mit 
                                          Eselsbrücke oder Vergleich aus dem Alltag
                                        - Am Ende ein kurzer "Kurz gesagt"-Abschnitt oder Merksätze, die man sich vor 
                                          der Klausur nochmal reinziehen kann

                                        ## Wichtige Regeln
                                        - Antworte AUSSCHLIESSLICH mit dem fertigen Lernzettel – kein "Hier ist dein 
                                          Lernzettel:" oder ähnliche Einleitungssätze davor
                                        - Erfinde NIEMALS Inhalte, die nicht in den hochgeladenen Dateien stehen. 
                                          Wenn eine Stelle unleserlich oder unklar ist, sag das ehrlich statt zu 
                                          raten ("Der Teil hier war leider nicht ganz lesbar auf dem Foto – check 
                                          das nochmal mit deinen Notizen")
                                        - Bei mehreren hochgeladenen Dateien: Inhalte sinnvoll zusammenführen, nicht 
                                          einfach nacheinander abhandeln, falls sie thematisch zusammengehören
                                        - Bleib fachlich korrekt – locker im Ton heißt nicht ungenau im Inhalt
                                        """;

    private readonly ChatClient _chatClient =
        new(model: "gpt-5.4-mini", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    public MainViewModel(Window window)
    {
        _window = window;
        CurrentPageViewModel = new CourseViewModel();
    }
    
    [RelayCommand]
    private void ShowCourseView() => CurrentPageViewModel = new CourseViewModel();

    [RelayCommand]
    private void ShowSettingsView() => CurrentPageViewModel = new SettingsViewModel();
    
    [RelayCommand]
    private void ShowLearningsheetView() => CurrentPageViewModel = new LearningsheetViewModel();

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
                ShowCourseView();
                break;
            case 1:
                ClearBreadcrumbs();
                CurrentOption = "Einstellungen";
                ShowSettingsView();
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


    [RelayCommand]
    private void LoadDummyTextFile()
    {
        Lernzettel = """
                     # Lernzettel: Photosynthese

                     ## 1) Grundlagen

                     Die **Photosynthese** ist der Prozess, bei dem Pflanzen Lichtenergie in chemische Energie umwandeln.

                     ### Wichtige Begriffe

                     - **Chlorophyll**: Der grüne Farbstoff in Chloroplasten
                     - **Stroma**: Flüssigkeit innerhalb der Chloroplasten
                     - **Thylakoid**: Membranstruktur, wo die Lichtreaktion stattfindet

                     ## 2) Die zwei Phasen

                     ### Lichtreaktion (Thylakoidmembran)

                     1. Licht wird von Chlorophyll absorbiert
                     2. Wasser wird gespalten (Photolyse): `2 H₂O → 4 H⁺ + 4 e⁻ + O₂`
                     3. ATP und NADPH werden produziert

                     ### Dunkelreaktion / Calvin-Zyklus (Stroma)

                     1. CO₂-Fixierung durch das Enzym RuBisCO
                     2. Reduktion zu G3P mithilfe von ATP und NADPH
                     3. Regeneration von RuBP

                     ## 3) Gesamtgleichung

                     > 6 CO₂ + 6 H₂O + Licht → C₆H₁₂O₆ + 6 O₂

                     ## 4) Merksätze

                     - **Photosynthese** braucht: Licht, Wasser, CO₂
                     - **Produkte**: Glukose und Sauerstoff
                     - Findet in **Chloroplasten** statt
                     - Wichtigstes Enzym: **RuBisCO**

                     ---

                     *Tipp: Die Lichtreaktion braucht direktes Licht, die Dunkelreaktion kann auch ohne Licht ablaufen (Name ist etwas irreführend).*
                     """;
    }
}