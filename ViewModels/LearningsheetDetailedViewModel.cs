using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

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

    [ObservableProperty] private ObservableCollection<Border> _chatlog = new();
    
    [ObservableProperty] private double[] _values1  = [2, 1, 3, 5, 3, 4, 6];
    [ObservableProperty] private double[] _values2  = [4, 2, 5, 2, 4, 5, 3];
    
    [ObservableProperty] private double[] _values3  = [20, 50, 40, 20, 40, 30, 50, 20, 50, 40];
    [ObservableProperty] private double[] _values4  = [3, 10, 5, 3, 7, 3, 8];

    
    
    [ObservableProperty] private PieData[] _data  = [
        new("Mary", 10),
        new("John", 20),
        new("Alice", 30),
        new("Bob", 40),
        new("Charlie", 50)
    ];
    
    [ObservableProperty] private double _value = 30;


}

public class PieData(string name, double value)
{
    public string Name { get; set; } = name;
    public double[] Values { get; set; } = [value];
}