using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral
{
    public sealed partial class Volume : UserControl
    {
        public Volume()
        {
            InitializeComponent();
        }
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;


        private void btnVolumeReset_Click(object sender, RoutedEventArgs e)
        {
            VolumeSlider.Value = 100;
            numVolume.Value = 100;
            PlayerService.VolumeChange(100);
        }


        private void numVolume_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            VolumeSlider.Value = numVolume.Value;
            PlayerService.VolumeChange(numVolume.Value);
        }

        private void VolumeSlider_ValueChanged(double obj)
        {
            PlayerService.VolumeChange(VolumeSlider.Value);
        }
    }
}
