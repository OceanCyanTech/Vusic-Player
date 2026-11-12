using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;

namespace Vusic_Player.UI.Experience.Pages
{
    public sealed partial class DonateToVusicPlayer : Page
    {
        public DonateToVusicPlayer()
        {
            this.InitializeComponent();
        }

        private async void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("https://oceancyantech.github.io/Vusic-Player/donate.html");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}