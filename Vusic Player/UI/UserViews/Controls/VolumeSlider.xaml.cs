using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
