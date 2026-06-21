using FlyleafLib.MediaFramework.MediaStream;
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
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.MediaProperties.AudioProperties;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral
{
    public sealed partial class General : UserControl
    {
        public Stream ViewModel { get; } = new();

        public General()
        {
            InitializeComponent();
        }

        private void srchAudio_CloseSearch(object sender, EventArgs e)
        {
            srchAudio.Visibility = Visibility.Collapsed;

        }

        private void srchAudio_ItemSelected(object sender, SearchModel e)
        {
            if (e is SearchModel srchmod && e.Carrier is AudioStream stream)
            {
                GeneralInfoService.ShowInfo($"Audio set to {stream.Language}");
                Stream.Set(stream);
            }
        }

        private void cmbAudioTrack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tglDisableAudioTrack_Checked(object sender, RoutedEventArgs e)
        {
            Stream.Disable(tglDisableAudioTrack.IsChecked ?? false);

        }

        private void tglDisableAudioTrack_Unchecked(object sender, RoutedEventArgs e)
        {
            Stream.Disable(tglDisableAudioTrack.IsChecked ?? false);
        }

        private void btnSearchAudio_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;

            ObservableCollection<SearchModel> searchres = new();
            foreach (var sub in PlayerService.Masterplayer.Audio.Streams)
            {
                searchres.Add(new SearchModel { ResultString = $"{sub.StreamIndex}. {sub.Language}", Carrier = sub });
            }
            srchAudio.ItemsSource = searchres;
            srchAudio.Visibility = Visibility.Visible;
        }
    }
}
