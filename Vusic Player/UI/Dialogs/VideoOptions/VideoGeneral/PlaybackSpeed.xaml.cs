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
using Vusic_Player.MediaProperties.PlaybackProperties;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoGeneral
{
    public sealed partial class PlaybackSpeed : UserControl
    {
        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }

        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register(
                nameof(SliderWidth),
                typeof(double),
                typeof(PlaybackSpeed),
                new PropertyMetadata(200.0) // Default value
            );
        public PlaybackSpeed()
        {
            InitializeComponent();
        }
        public string FormatSpeed(double speed)
        {
            return $"{speed:F1}x";
        }
        private void sldSpeed_ValueChanged(double obj)
        {
            if (numSpeed != null && numSpeed.Value != obj)
            {
                numSpeed.Value = obj;
            }
            SpeedService.Set(obj);
        }

        private void numSpeed_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sldSpeed != null && sldSpeed.Value != args.NewValue)
            {
                sldSpeed.Value = args.NewValue;
                SpeedService.Set(sldSpeed.Value);
            }
        }

        private void tsReverse_Toggled(object sender, RoutedEventArgs e)
        {
            ReverseService.Reverse(tsReverse.IsOn);
        }

        private void tsLoop_Toggled(object sender, RoutedEventArgs e)
        {
            QueueService.IsLoopTrue = tsLoop.IsOn;
        }
        public  MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        private void btnDefaultSpeed_Click(object sender, RoutedEventArgs e)
        {
            sldSpeed.Value = 1;
            numSpeed.Value = 1;
            SpeedService.Set(1);
        }
    }

}
