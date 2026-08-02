using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Recallr.Models.Services;

namespace Recallr.Models.Models;

public class TrExtension : MarkupExtension
{
    public string Key { get; set; }
    public TrExtension(string key) => Key = key;
    public override object ProvideValue(IServiceProvider sp) =>
        new Binding($"[{Key}]") { Source = LocalizationService.Instance, Mode = BindingMode.OneWay };
}