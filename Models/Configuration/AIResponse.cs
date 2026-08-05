using System.Collections.Generic;

namespace Recallr.Models.Configuration;

public class AIResponse
{
    public string type  { get; set; }
    public bool correct { get; set; }
    public string explanation { get; set; }
    public string question { get; set; }
    public bool multipleChoice { get; set; }
    public List<string> answers { get; set; }
}