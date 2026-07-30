using Avalonia.Data.Converters;

namespace Recallr.Models.Models;

public static class SenderConverters
{
    public static readonly IValueConverter IsAi =
        new FuncValueConverter<ChatSender, bool>(s => s == ChatSender.Ai);

    public static readonly IValueConverter IsUser =
        new FuncValueConverter<ChatSender, bool>(s => s == ChatSender.User);
}