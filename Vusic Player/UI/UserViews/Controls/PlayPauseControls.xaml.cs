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
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class PlayPauseControls : UserControl
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public PlayPauseControls()
        {
            InitializeComponent();
        }
        #region Media Transport Controls
        private void btnPrevious_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
           // QueueService.PlayPrevious();
        }

        private void btnPlayPause_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PlayerService.PlayPause();
        }


        private void btnNext_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
       //     QueueService.PlayNext();
        }

        #endregion
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(
                nameof(ImageWidth),
                typeof(double),
                typeof(PlayPauseControls),
                new PropertyMetadata(26.0) // Default value
            );
        public double SpaceWidth
        {
            get { return (double)GetValue(SpaceWidthProperty); }
            set { SetValue(SpaceWidthProperty, value); }
        }

        public static readonly DependencyProperty SpaceWidthProperty =
            DependencyProperty.Register(
                nameof(SpaceWidth),
                typeof(double),
                typeof(PlayPauseControls),
                new PropertyMetadata(5.0) // Default value
            );
        private void btnSeekBefore_Click(object sender, RoutedEventArgs e)
        {
            PlayerService.SeekBefore();
        }

        private void btnSeekAfter_Click(object sender, RoutedEventArgs e)
        {
            PlayerService.SeekAhead();
        }

    }
}
