using System.Collections.Generic;

namespace Recallr.Models.Configuration;


public class AIResponse
{
    public string Type { get; set; } = string.Empty;
    
    public string Topic { get; set; } = string.Empty;

    public string? Question { get; set; }

    public bool? MultipleChoice { get; set; }

    public List<string>? Answers { get; set; }

    public bool? Correct { get; set; }

    public string? Explanation { get; set; }

    public string? Message { get; set; }

    public NextQuestion? NextQuestion { get; set; }
}

public class NextQuestion
{
    public string Question { get; set; } = string.Empty;

    public bool MultipleChoice { get; set; }

    public List<string>? Answers { get; set; }
    
    public string Topic { get; set; } = string.Empty;
}