using System.Collections.Generic;

namespace Recallr.Models.Configuration;

public class ClientCourses
{
    public string ID { get; set; }
    public int Icon { get; set; }
    public string Name { get; set; }
    public string TeacherName { get; set; }
    public List<Learnsheet> learnsheets { get; set; } = new();
}