using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Vusic_Player.Pages.Views.Onboarding;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SplashScreen : Page
{
    DispatcherTimer timer = new DispatcherTimer();
    public SplashScreen()
    {
        InitializeComponent();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }
    int counter = 2;
    private void Timer_Tick(object? sender, object e)
    {
        counter--;
        if (counter == 0)
        {
            Frame.Navigate(typeof(OptimizeApp));
        }
    }
}
