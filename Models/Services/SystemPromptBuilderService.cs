using System;

namespace Recallr.Models.Services;

public class SystemPromptBuilderService
{
    private static string GetDifficultyBlock(int difficulty) => difficulty switch
        {
            0 =>
                """
                Stelle Fragen, die Transfer, Anwendung oder Vergleich verlangen (z.B. "was wäre wenn...", Fallbeispiele, Abgrenzung ähnlicher Konzepte). Der Nutzer muss das Konzept wirklich verstanden haben, reines Textscanning darf nicht reichen. Bleib hartnäckig bei Lücken, auch über mehrere Runden.
                """,
            1 =>
                """
                Stelle Fragen, die ein echtes Verständnis der Zusammenhänge zeigen, nicht nur Wortlaut-Wiedererkennung. Verknüpfe wo sinnvoll 2 Begriffe/Konzepte aus dem Lernzettel. Schwierigkeit passt sich an: Basics sitzen → tiefer werden; Lücke erkannt → beim Punkt bleiben statt weiterzuspringen.
                """,
            2 =>
                """
                Stelle einfache, direkte Fragen, die 1:1 im Lernzettel beantwortet werden (Begriffe, Definitionen, einzelne Fakten). Keine Verknüpfung mehrerer Konzepte nötig. Baue Schwierigkeit langsam auf, wenn der Nutzer sicher antwortet.
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unbekannter Schwierigkeitsgrad")
        };

        private static string GetLearnsheetSummaryType(int learnsheetType) => learnsheetType switch
        {
            0 =>
                """
                Gründlich und ausführlich erklären, nie oberflächlich. Jeden Fachbegriff mit eigenem Absatz einführen, Zusammenhänge zwischen Konzepten explizit herausarbeiten, wo sinnvoll Beispiele/Alltagsvergleiche ergänzen. Lieber einen Punkt zu Ende erklären als viele nur anreißen.
                """,
            1 =>
                """
                Kurz und knapp halten. Nur die prüfungsrelevanten Kernpunkte, keine ausschweifenden Erklärungen oder mehrere Beispiele pro Begriff. Ein Satz pro Fakt, wo möglich Stichpunkte statt Fließtext. Trotzdem nichts Wichtiges weglassen – knapp heißt komprimiert, nicht unvollständig.
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(learnsheetType), learnsheetType,
                "Unbekannter Zusammenfassungsstil")
        };




        public static string BuildChat(string learnsheetLanguage, string lernzettelContent, int difficulty)
        {
            var forcedMultipleChoice = Random.Shared.Next(2) == 0;
            var forcedType = forcedMultipleChoice
                ? "eine MULTIPLE-CHOICE-Frage (multipleChoice: true)"
                : "eine OFFENE Frage (multipleChoice: false)";
            
            Console.WriteLine(forcedType);
            
            return SystemPrompts.Template
                .Replace("%%LANG%%", learnsheetLanguage)
                .Replace("%%QUESTIONTYPE%%", forcedType)
                .Replace("%%DIFFICULTY%%", GetDifficultyBlock(difficulty))
                .Replace("%%CONTEXT%%", lernzettelContent);

        }
            
        
        public static string BuildLearnsheet(string learnsheetLanguage, int learnsheetType) 
            => string.Format(SystemPrompts.LearnsheetSystemPrompt, learnsheetLanguage, GetLearnsheetSummaryType(learnsheetType));
}