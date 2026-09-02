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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.VideoLibraryControls
{
    public sealed partial class UserShows : UserControl
    {
        private bool _isLoadingData = false;

        public UserShows()
        {
            InitializeComponent();
            LoadShows();
            PlaylistCreation.ShowCreationCallAdd -= PlaylistCreation_ShowCreationCallAdd;
            PlaylistCreation.ShowCreationCallAdd += PlaylistCreation_ShowCreationCallAdd;
        }
        private async void LoadShows()
        {
            if (_isLoadingData) return;
            _isLoadingData = true;
            try
            {
                Debug.WriteLine("Load Shows");
                ShowsList.Clear();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                foreach (var show in currentSettings.Shows)
                {

                    ShowsList.Add(show);

                }
                grdViewShows.ItemsSource = ShowsList;
                ShowsList.CollectionChanged += ShowsList_CollectionChanged; ;
                UpdateUI();
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        private async void ShowsList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isLoadingData || _isSavingShow) return;
            if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Move)
            {
                Debug.WriteLine("Moved");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.Shows = ShowsList;
                MasterSearchIndex.ShowsMaster = ShowsList;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                UpdateUI();

            }
        }

        public ObservableCollection<Show> ShowsList { get; set; } = new();

        private void UpdateUI()
        {
            if (ShowsList.Count == 0)
            {
                grdRecents.Visibility = Visibility.Collapsed;
                grdEmptySuggestions.Visibility = Visibility.Visible;
            }
            else
            {
                grdRecents.Visibility = Visibility.Visible;
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
            }
        }
        private bool _isSavingShow = false;
        private async void PlaylistCreation_ShowCreationCallAdd()
        {
            if (_isSavingShow) return;
            _isSavingShow = true;
            try
            {
                Debug.WriteLine("Called");
                if (PlaylistCreation.showitem != null)
                {
                    Debug.WriteLine("Called2");

                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    if (PlaylistCreation.showitem.Name is string name)
                    {
                        string baseName = name.Trim();

                        if (string.IsNullOrEmpty(baseName)) baseName = "Show";

                        string finalName = baseName;
                        int counter = 1;
                        while (currentSettings.Shows.Any(p =>
                            string.Equals(p.Name, finalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            finalName = $"{baseName} ({counter++})";
                        }
                        PlaylistCreation.showitem.Name = finalName;
                    }
                    ShowsList.Add(PlaylistCreation.showitem);

                    currentSettings.Shows.Add(PlaylistCreation.showitem);
                    MasterSearchIndex.ShowsMaster.Add(PlaylistCreation.showitem);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                }
                UpdateUI();
            }
            finally
            {
                _isSavingShow = false;
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chckSelectAll_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRemoveShows_Click(object sender, RoutedEventArgs e)
        {

        }

        private void grdViewShows_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemclicked = e.ClickedItem as Show;
            if (itemclicked == null) return;
            if (chkSelect.IsChecked == false)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(ShowModel), itemclicked);
                }
            }
        }

        private void grdViewShows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MenuFlyout_Opened(object sender, object e)
        {

        }

        private void mnftOpenShow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPlayShow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftEditShow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftDeleteShow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNewShow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chckSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
