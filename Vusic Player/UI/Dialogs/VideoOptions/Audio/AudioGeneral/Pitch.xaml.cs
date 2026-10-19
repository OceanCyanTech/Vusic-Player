using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Pages.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral
{
    public sealed partial class Pitch : UserControl
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public Pitch()
        {
            InitializeComponent();
        }

        private void PitchSlider_ValueChanged(double obj)
        {
            MediaProperties.AudioProperties.Pitch.Apply(obj);
        }
        public event Action? PitchExpanded;
        private void btnPitchReset_Click(object sender, RoutedEventArgs e)
        {
            PitchSlider.Value = 1;
            MediaProperties.AudioProperties.Pitch.Apply(1);
        }

        private void btnApplyPitchToMultiple_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                if (App.NavigationFrame.CurrentSourcePageType != typeof(PitchExport))
                {
                    PitchExpanded?.Invoke();
                    App.NavigationFrame.Navigate(typeof(PitchExport));
                }
            }
        }
    }
}
