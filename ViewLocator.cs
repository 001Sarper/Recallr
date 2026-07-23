using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Recallr.ViewModels;

namespace Recallr;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        // Map view model full name to view full name, e.g.
        // Recallr.ViewModels.SettingsViewModel -> Recallr.Views.SettingsView
        var vmType = param.GetType();
        var vmFullName = vmType.FullName!;
        var viewFullName = vmFullName
            .Replace(".ViewModels.", ".Views.", StringComparison.Ordinal)
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        // Try to resolve the type from the current assembly first, then Type.GetType as fallback
        var type = vmType.Assembly.GetType(viewFullName) ?? Type.GetType(viewFullName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + viewFullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
