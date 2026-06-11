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
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{

    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
                        //ObservableCollection<SongModel> single = new();
                        //string Title = Path.GetFileNameWithoutExtension(media.Path);

                        //single.Add(new SongModel { FilePath = media.Path, Title = Title, AlbumName = AudioMetadata.Album(media.Path), Artist = AudioMetadata.Artist(media.Path), SongDuration = await AudioMetadata.GetTimeSpanDuration(media.Path) });
                        //QueueService.PlayMedia(single, false, false);
                    }
                }
                else if (isVideo)
                {
                    if (File.Exists(media.Path))
                        Frame.Navigate(typeof(VideoPlayer), media.Path);
                    //PlayerService.SendPath = media.Path;
                    //PlayerService.isProgress = false;
                    //PlayerService.VideoInvoke();
                }
                else
                {
                    //Handle other cases
                }
            }
        }

        private async void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Open Folder", Windows.Storage.Pickers.PickerLocationId.Downloads);
            if(folder != null)
            {
                if(App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = Path.GetFileName(folder.Path), FolderPath = folder.Path });
                }
            }
        }

        private void expRecentsVideo_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (!this.IsLoaded) return;

            if (sender != expRecentsVideo) expRecentsVideo.IsExpanded = false;

            if (sender != expRecentMusic) expRecentMusic.IsExpanded = false;

            if (sender != expMusicMix) expMusicMix.IsExpanded = false;

            if (sender != expFav) expFav.IsExpanded = false;
        }
    }
}
