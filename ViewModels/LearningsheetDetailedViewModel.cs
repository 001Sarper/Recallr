using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Recallr.ViewModels;

public partial class LearningsheetDetailedViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isFilesSelected = true;
    [ObservableProperty] private bool _isSummarySelected = false;
    [ObservableProperty] private bool _isChatSelected = false;
    [ObservableProperty] private bool _isKnownledgeSelected = false;

    [ObservableProperty] private string _learnsheetContent = """
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