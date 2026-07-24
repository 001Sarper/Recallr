using System.Collections.Generic;

namespace Recallr.Models.Configuration;

public class ConfigManager
{
    public List<ClientSettings> ClientSettings { get; set; } = new();
    public List<ClientCourses> ClientCourses { get; set; } = new();

}