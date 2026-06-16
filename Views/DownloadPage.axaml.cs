using Avalonia.Controls;
using SkyLauncher.ViewModels;

namespace SkyLauncher.Views;

public partial class DownloadPage : UserControl
{
    public DownloadPage()
    {
        InitializeComponent();
        DataContext = new DownloadViewModel();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

        var tabControl = sender as TabControl;
        var selectedTabItem = tabControl?.SelectedItem as TabItem;

        if (selectedTabItem != null)
        {

            var transitionControl = FindTransitioningContentControl(selectedTabItem);

            if (transitionControl != null)
            {
                var content = transitionControl.Content;
                transitionControl.Content = null;
                transitionControl.Content = content;
            }
        }
    }

    private TransitioningContentControl? FindTransitioningContentControl(TabItem tabItem)
    {
        if (tabItem.Content is TransitioningContentControl transition)
        {
            return transition;
        }
        return null;
    }
}