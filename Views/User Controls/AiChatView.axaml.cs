using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Recallr.ViewModels;

namespace Recallr.Views;

public partial class AiChatView : UserControl
{
    public AiChatView()
    {
        InitializeComponent();
        this.DataContextChanged += AiChatView_DataContextChanged;
    }

    private void AiChatView_DataContextChanged(object sender, System.EventArgs e)
    {
        if (DataContext is AiChatViewModel vm)
        {
            vm.Messages.CollectionChanged += (s, ev) =>
            {
                var scrollViewer = this.FindControl<Avalonia.Controls.ScrollViewer>("ChatScrollViewer");
                scrollViewer?.ScrollToEnd();
            };
        }
    }
}