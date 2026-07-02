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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.Pages.Views
{
    public sealed partial class LyricViewer : Page
    {
        public LyricViewer()
        {
            InitializeComponent();
        }
        private async void btnOpenMedia_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var media = await MediaPicker.PickSingle(App.MainWindowInstance, "Open Media");
            if (media != null)
            {
                bool isAudio = AudioExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                bool isVideo = VideoExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                if (isAudio)
                {

                    if (File.Exists(media.Path))
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(media.Path);

                        single.Add(new SongModel { FilePath = media.Path, Title = Title, AlbumName = AudioMetadata.Album(media.Path), Artist = AudioMetadata.Artist(media.Path), SongDuration = await AudioMetadata.GetTimeSpanDuration(media.Path) });
                        QueueService.PlayMedia(single, false, false);
                    }
                }
                else if (isVideo)
                {
                    if (PlayerService.InVideoPage == false)
                    {
                        if (File.Exists(media.Path))
                            Frame.Navigate(typeof(VideoPlayer), media.Path);
                    }
                    else
                    {
                        PlayerService.OpenPath(media.Path);
                    }

                }
                else
                {
                    //Handle other cases
                }
            }
        }
    }
}
