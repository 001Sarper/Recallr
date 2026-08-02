using System;
using System.IO;

namespace Recallr.Models.Services;

public class FileSystemPaths
{
    public static string coursesDirectoryPath { get; }
    public static string chatlogDirectoryPath { get; }
    public static string settingsFilePath { get; }
    public static string coursesFilePath  {get; }
    public static DirectoryInfo dataprotectionKeysDirectory { get;  }
    
    static FileSystemPaths()
    {
        coursesDirectoryPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Courses");
        chatlogDirectoryPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "ChatLogs");
        settingsFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientSettings.json");
        coursesFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Recallr", "Config", "ClientCourses.json");
        dataprotectionKeysDirectory =
            new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Recallr", "Config", "DataProtectionKeys"));
    }
}