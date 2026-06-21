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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Subtitle.SubtitleGeneral
{
    public sealed partial class Delay : UserControl
    {
        public Configuration.Helper.SubtitlesProperties.Delay Delayvalue => Configuration.Helper.SubtitlesProperties.Delay.Instance;

        public Delay()
        {
            InitializeComponent();
        }

        private void rptbtnDecreaseSubDel_Click(object sender, RoutedEventArgs e)
        {
            numSubtitlesDelay.Value -= 10;
            TagToDelay();
        }

        private void numSubtitlesDelay_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (numSubtitlesDelay.Value == 0)
            {
                Configuration.Helper.SubtitlesProperties.Delay.Reset();
            }
            TagToDelay();
        }

        private void rptbtnIncreaseSubDel_Click(object sender, RoutedEventArgs e)
        {

            numSubtitlesDelay.Value += 10;
            TagToDelay();
        }
        public void TagToDelay()
        {
            string tagValue = ((int)numSubtitlesDelay.Value).ToString();
            Configuration.Helper.SubtitlesProperties.Delay.Apply(tagValue);
        }


        private void btnSubDelayReset_Click(object sender, RoutedEventArgs e)
        {
            numSubtitlesDelay.Value = 0;
            Configuration.Helper.SubtitlesProperties.Delay.Reset();
        }
    }
}
