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
using Vusic_Player.Configuration.UserSettings;
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
            ttSpeedCustom.IsOpen = false;
            if (sender is RadioMenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Text is string speed)
            {
                if (double.TryParse(speed, System.Globalization.CultureInfo.InvariantCulture, out double speeddouble))
                {
                    SpeedService.Set(speeddouble);
                }
            }

        }

     
        private void btnEffects_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMiniPlayer_Click(object sender, RoutedEventArgs e)
        {
            PictureInPicture pictureinPicture = new PictureInPicture();
            pictureinPicture.Activate();
            MainWindow.HideWindow();
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

        private async void btnAddtoFavourites_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.CurrentPlayingPath == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var existingfav = favourites.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if(existingfav != null)
            {
                favourites.Remove(existingfav);
                media.IsFavourite = false;
            }
            else
            {
                media.IsFavourite = true;
                favourites.Add(new Configuration.ClassModels.FavouriteItems { FilePath = PlayerService.CurrentPlayingPath });
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void mnftPitch_Click(object sender, RoutedEventArgs e)
        {
            ttPitch.IsOpen = true;
        }

        private void customSpeed_Click(object sender, RoutedEventArgs e)
        {
            ttSpeedCustom.IsOpen = true;
            media.AudioProperties = Visibility.Collapsed;
        }
        public string GetFavToolTip(bool isFav)
        {
            return isFav ? "Remove from Favorites" : "Add to Favorites";
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


        private void ttSpeedCustom_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            media.AudioProperties = Visibility.Visible;

        }
    }
}
