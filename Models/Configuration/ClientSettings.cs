namespace Recallr.Models.Configuration;

public class ClientSettings
{
    //Account Settings
    public string OpenaiKey { get; set; }
    public string ProfileName { get; set; }
    public string ProfileMail { get; set; }
    public string AiModel { get; set; }
    
    //View Settings
    public int Theme { get; set; }
    public int Language { get; set; }
    public int FontSize { get; set; }
    
    //AI-Behaviour
    public int SummaryStyle { get; set; }
    public int Difficulty { get; set; }
    public int QuestionType { get; set; }
    
}