using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;




namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class VolumeSlider : UserControl
    {
        MediaPlaybackController mediaPlaybackController => MediaPlaybackController.Instance;
        public VolumeSlider()
        {
            InitializeComponent();
        }

        private void OceanSlider_ValueChanged(double obj)
        {
            PlayerService.VolumeChange(obj);
        }
    }
}
