using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Recallr.Models.Configuration;

public class ClientCourses
{
    public Guid ID { get; set; }
    public int Icon { get; set; }
    public string Name { get; set; }
    public string TeacherName { get; set; }
    public List<Learnsheet> learnsheets { get; set; } = new();
    
    [JsonIgnore] // nicht mit serialisieren, nur zur Anzeige berechnet
    public string IconDisplay => Icon switch
    {
        0 => "📚",
        1 => "🧮",
        2 => "🔬",
        3 => "🌍",
        4 => "🎨",
        _ => "📘"
    };
}