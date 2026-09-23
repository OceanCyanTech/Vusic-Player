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
using System.Threading.Tasks;
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
    public class FavToItems : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} favourite" : $"• {count} favourites";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
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
            Debug.WriteLine("CALLED");
            var exist = FavouritesItemsCol.ToList().FirstOrDefault(p => p.FilePath == obj.FilePath);
            if(exist != null)
            {
                Debug.WriteLine("EXIST CALLED");

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

        private async void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FavouritesItemsCol)
            {
                QueueService.VusicQueue.Add(item);
                QueueService.VusicQueueNext.Add(item);
            }
            ttAddedtoQueue.IsOpen = true;
            await Task.Delay(2000);
            ttAddedtoQueue.IsOpen = false;
        }

        private async void btnAddItemsToFav_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var media = await FilePickers.MediaPicker.PickMultipleMediaFilesAsync(App.MainWindowInstance, "Select Media to add to Favourites");
            if(media != null)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var favourites = currentSettings.Favourites;
                foreach(var item in media)
                {
                    var exist = favourites.FirstOrDefault(p => p.FilePath == item.Path);
                    if(exist == null)
                    {
                        MusicProperties properties = await item.Properties.GetMusicPropertiesAsync();
                        favourites.Add(new FavouriteItems { FilePath = item.Path });

                        string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : item.DisplayName;
                        string album = !string.IsNullOrWhiteSpace(properties.Album) ? properties.Album : "Unknown Album";
                        string artist = !string.IsNullOrWhiteSpace(properties.Artist) ? properties.Artist : "Unknown Artist";
                        var glyph = "\uEC4F";

                        string fileExtension = item.FileType.ToLowerInvariant();
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
                            FilePath = item.Path,
                            FavOpacity = opac,
                            FavString = text,
                            VisibilityofAudioMeta = visibility,
                            VisibilityofVideoInfo = visibilityofvidtext,
                            IsFavourite = true,
                            Glyph = glyph,
                        });
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }
    }
}
