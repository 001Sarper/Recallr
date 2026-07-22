using System.Collections.Generic;

namespace Recallr.Models.Settings;

public class SettingsManager
{
    public List<ClientSettings> ClientSettings { get; set; } = new();
}