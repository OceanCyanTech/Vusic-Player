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
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavouritesView : Page
    {
        ObservableCollection<SongModel> FavouritesItemsCol = new ObservableCollection<SongModel>();
        public FavouritesView()
        {
            InitializeComponent();
            lstViewFav.FavouritesRemoved -= LstViewFav_FavouritesRemoved; 
            lstViewFav.FavouritesRemoved += LstViewFav_FavouritesRemoved; 
        }

        private void LstViewFav_FavouritesRemoved(SongModel obj)
        {
            var exist = FavouritesItemsCol.ToList().FirstOrDefault(p => p.FilePath == obj.FilePath);
            if(exist != null)
            {
                FavouritesItemsCol.Remove(exist);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            foreach (var favourite in favourites)
            {
                if (File.Exists(favourite.FilePath))
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(favourite.FilePath);
                    MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();


                    string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : file.DisplayName;
                    string album = !string.IsNullOrWhiteSpace(properties.Album) ? properties.Album : "Unknown Album";
                    string artist = !string.IsNullOrWhiteSpace(properties.Artist) ? properties.Artist : "Unknown Artist";
                    var glyph = "\uEC4F";

                    string fileExtension = file.FileType.ToLowerInvariant();
                    Visibility visibility = Visibility.Visible;
                    Visibility visibilityofvidtext = Visibility.Collapsed;
                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {

                        glyph = "\uE8B2";
                        visibility = Visibility.Collapsed;
                        visibilityofvidtext = Visibility.Visible;
                    }
                    double opac = 1.0;
                    string text = "Remove from Favourites";
                    FavouritesItemsCol.Add(new SongModel
                    {
                        Title = title,
                        AlbumName = album,
                        Artist = artist,
                        SongDuration = properties.Duration,
                        FilePath = file.Path,
                        FavOpacity = opac,
                        FavString = text,
                        VisibilityofAudioMeta = visibility,
                        VisibilityofVideoInfo = visibilityofvidtext,
                        IsFavourite = true,
                        Glyph = glyph,
                    });
                }
            }
            base.OnNavigatedTo(e);
        }
        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FavouritesItemsCol)
            {
                item.IsCompleted = false;
            }

            QueueService.PlayMedia(FavouritesItemsCol,  false, false);
        }
    }
}
