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



namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral
{
    public sealed partial class Delay : UserControl
    {
        public MediaProperties.AudioProperties.Delay Delayvalue => MediaProperties.AudioProperties.Delay.Instance;

        public Delay()
        {
            InitializeComponent();


        }

        private void rptbtnDecreaseAudioDel_Click(object sender, RoutedEventArgs e)
        {
            numAudioDelay.Value -= 10;
            TagToDelay();
        }

        private void numAudioDelay_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (numAudioDelay.Value == 0)
            {
                MediaProperties.AudioProperties.Delay.Reset();
            }
            TagToDelay();
        }

        private void rptbtnIncreaseAudioDel_Click(object sender, RoutedEventArgs e)
        {
            numAudioDelay.Value += 10;
            TagToDelay();
        }
        public void TagToDelay()
        {
            string tagValue = ((int)numAudioDelay.Value).ToString();
            MediaProperties.AudioProperties.Delay.Apply(tagValue);
        }

        private void btnAudioDelayReset_Click(object sender, RoutedEventArgs e)
        {
            numAudioDelay.Value = 0;
            MediaProperties.AudioProperties.Delay.Reset();
        }
    }
}
