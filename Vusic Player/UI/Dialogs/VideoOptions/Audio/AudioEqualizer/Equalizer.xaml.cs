using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.MediaProperties.AudioProperties;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioEqualizer
{
    public sealed partial class Equalizer : UserControl
    {
        public Equalizer()
        {
            InitializeComponent();
        }

        private void OceanSlider_ValueChanged(double obj)
        {
  
        }

        private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            double[] gains = [0, gain63.Value, 0, 0, 0, 0, 0, 0, 0, 0];
            EqualizerService.Create(gains);
        }

        private void gainCommon_ValueChanged(double obj)
        {

        }
    }
}
