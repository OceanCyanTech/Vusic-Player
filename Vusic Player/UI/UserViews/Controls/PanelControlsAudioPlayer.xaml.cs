using FlyleafLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
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
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.CurrentPlayingPath == "") return;
            PictureInPicture pictureinPicture = new PictureInPicture();
            pictureinPicture.Activate();
            MainWindow.HideWindow();
        }

        private void btnFullView_Click(object sender, RoutedEventArgs e)
        {
            if (ExpandedViewButtonToolTip == "Normal View")
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.GoBack();
                }
            }
            else
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(MusicPlayerFull));
                }
            }
        }
        bool isFullView = false;
        private void btnLyrics_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.CurrentPlayingPath == "") return;
            if (PlayerService.Masterplayer == null) return;

            if (ExpandedViewButtonToolTip != "Normal View")
            {

                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(MusicPlayerFull));
                }
            }
        }

        private async void btnAddtoFavourites_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.CurrentPlayingPath == "") return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var existingfav = favourites.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (existingfav != null)
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
            if (PlayerService.CurrentPlayingPath == "") return;
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

  
        ObservableCollection<DeviceOutputShow> AudioDevices = new ObservableCollection<DeviceOutputShow>();
        private void mnftMultiDevice_Click(object sender, RoutedEventArgs e)
        {

            AudioDevices.Clear();
            ttMultiDeviceOutput.IsOpen = true;
            multiOutputMixer.ItemsSource = AudioDevices;
            if(PlayerService.Masterplayer == null)
            {
                grdNoMediaPlaying.Visibility = Visibility.Visible;
                multiOutputMixer.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                grdNoMediaPlaying.Visibility = Visibility.Collapsed;
                multiOutputMixer.Visibility = Visibility.Visible;
            }
            foreach (var device in Engine.Audio.Devices)
            {
                bool isDefault = (device.Name?.Contains("Default", StringComparison.OrdinalIgnoreCase) ?? false);
                if (!isDefault)
                {
                    var volume = PlayerService.GetVolumeOfDevice(device.Id);
                    AudioDevices.Add(new DeviceOutputShow { DeviceID = device.Id, DeviceName = device.Name ?? "Unknown Device", DeviceVolume = $"{volume*100.0f}%", Volume = volume*100.0f });
                }
            }
        }

        private void btnRefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null)
            {
                grdNoMediaPlaying.Visibility = Visibility.Visible;
                multiOutputMixer.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                grdNoMediaPlaying.Visibility = Visibility.Collapsed;
                multiOutputMixer.Visibility = Visibility.Visible;
            }
            multiOutputMixer.ItemsSource = AudioDevices;

            foreach (var device in Engine.Audio.Devices)
            {
                bool isDefault = (device.Name?.Contains("Default", StringComparison.OrdinalIgnoreCase) ?? false);
                if (!isDefault)
                {
                    var volume = PlayerService.GetVolumeOfDevice(device.Id);
                    AudioDevices.Add(new DeviceOutputShow { DeviceID = device.Id, DeviceName = device.Name ?? "Unknown Device", DeviceVolume = $"{volume * 100.0f}%", Volume = volume * 100.0f });
                }
            }
        }
    }
}
