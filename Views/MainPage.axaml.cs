
using SkyLauncher.ViewModels;
using Avalonia.Controls;
using System.Linq;

namespace SkyLauncher.Views;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = new MainPageViewModel();

    }

    
}