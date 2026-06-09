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
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class Favourites : UserControl
    {
        public Favourites()
        {
            InitializeComponent();
            InitializeRecommendations();
        }
        ObservableCollection<FavouritesRecommend> FinalList = new();
        private async void InitializeRecommendations()
        {
            Random random = new Random();
            int numberofaudiorecommendations = random.Next(5); 
            int numberofvideorecommendations = random.Next(5);

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var randomvideos = new ObservableCollection<string>();
            var randomaudios = new ObservableCollection<string>();
            foreach (var item in favourites)
            {
                string filePath = item.FilePath; // Assuming this is a string

                if (string.IsNullOrEmpty(filePath)) continue;

                // Path.GetExtension returns the extension including the dot (e.g., ".mp4")
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
          
                if (VideoExtensions.List.Contains(extension))
                {
                    randomvideos.Add(item.FilePath);
                    continue;
                }
                else if (AudioExtensions.List.Contains(extension))
                {
                    randomaudios.Add(item.FilePath);
                    continue;
                }
              
            }
            var ObservableFinal = new ObservableCollection<FavouritesRecommend>();
            foreach(var item in randomvideos)
            {
                ObservableFinal.Add(new FavouritesRecommend { FilePath = item, FileName = Path.GetFileNameWithoutExtension(item), Thumbnail = await FileThumbnailObtain.GetVideoFrameAsync(item) });
            }
            foreach (var item in randomaudios)
            {
                ObservableFinal.Add(new FavouritesRecommend { FilePath = item, FileName = Path.GetFileNameWithoutExtension(item), Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(item) });
            }
            if (ObservableFinal.Count == 0) return;
            FinalList.Clear();
            Random rng = new Random();
            var randomFive = ObservableFinal.OrderBy(item => rng.Next()).Take(5);
            foreach(var item in randomFive)
            {
                FinalList.Add(item);
            }
            if(FinalList.Count != 0)
            {
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
                grdRecents.Visibility = Visibility.Visible;
                grdvFavourites.ItemsSource = FinalList;
            }
            else
            {
                grdEmptySuggestions.Visibility = Visibility.Visible;
                grdRecents.Visibility = Visibility.Collapsed;
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            grdvFavourites.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked == true) grdvFavourites.SelectAll();
            else grdvFavourites.SelectedItems.Clear();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void grdvFavourites_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void grdvFavourites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
