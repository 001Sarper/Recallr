using System;

namespace Recallr.Models.Configuration;

public class Learnsheet
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public string TestDate { get; set; }
    public string Description { get; set; }
}