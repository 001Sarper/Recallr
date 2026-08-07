using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Recallr.Views;

public partial class LearningsheetDetailedView : UserControl
{
    public LearningsheetDetailedView()
    {
        InitializeComponent();
        ChatScrollViewer.ScrollChanged += (s, e) =>
        {
            if (e.ExtentDelta.Y > 0)
                ChatScrollViewer.ScrollToEnd();
        };
    }
}