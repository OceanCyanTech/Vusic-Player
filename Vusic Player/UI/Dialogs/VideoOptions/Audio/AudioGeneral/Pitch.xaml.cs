using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.Playback;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

        private void btnPitchReset_Click(object sender, RoutedEventArgs e)
        {
            PitchSlider.Value = 1;
            MediaProperties.AudioProperties.Pitch.Apply(1);
        }
    }
}
