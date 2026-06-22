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
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class PanelControlsAudioPlayer : UserControl
    {
        public string ExpandedViewButtonToolTip
        {
            get { return (string)GetValue(ExpandedViewButtonToolTipProperty); }
            set { SetValue(ExpandedViewButtonToolTipProperty, value); }
        }

        public static readonly DependencyProperty ExpandedViewButtonToolTipProperty =
            DependencyProperty.Register(
                nameof(ExpandedViewButtonToolTip),
                typeof(string),
                typeof(PanelControlsAudioPlayer),
                new PropertyMetadata("Expanded View") // Default value
            );
        public PanelControlsAudioPlayer()
        {
            InitializeComponent();
        }
        private void btnSpeedfly_Click(object sender, RoutedEventArgs e)
        {
            //ttSpeedCustom.IsOpen = false;
            //var menuflyoutitem = (RadioMenuFlyoutItem)sender;

            //string speed = menuflyoutitem.Text;
            //videospeed = speed;
            //if (PlayerService.MasterPlayer != null)
            //{
            //    if (menuflyoutitem != null)
            //    {

            //        if (double.TryParse(speed, System.Globalization.CultureInfo.InvariantCulture, out double speedfloat))
            //        {
            //            PlayerService.MasterPlayer.Speed = speedfloat;
            //        }
            //    }
            //}
            ////ttSpeedCustom.IsOpen = false;
            ////var menuflyoutitem = (RadioMenuFlyoutItem)sender;

            ////string speed = menuflyoutitem.Text;
            ////videospeed = speed;
            ////if (player != null && maintimer != null)
            ////{


            ////    }

            ////}
        }

        private void btnSetCustomSpeed_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEffects_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMiniPlayer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFullView_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(MusicPlayerFull));
                
            }
        }

        private void btnLyrics_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddtoFavourites_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPitch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void customSpeed_Click(object sender, RoutedEventArgs e)
        {
            ttSpeedCustom.IsOpen = true;
        }
        public MediaPlaybackController media => MediaPlaybackController.Instance;
        private async void btnInfo_Click(object sender, RoutedEventArgs e)
        {

            if (App.MainWindowInstance == null) return;
            if (PlayerService.CurrentPlayingPath == null) return;
            //Handle file not exist
            if (App.MainWindowInstance is MainWindow wind)
            {

                wind.ShowFileInfo(PlayerService.CurrentPlayingPath);
            }
        }

        private void mnftReverb_Click(object sender, RoutedEventArgs e)
        {
            ttReverb.IsOpen = true;
        }
    }
}
