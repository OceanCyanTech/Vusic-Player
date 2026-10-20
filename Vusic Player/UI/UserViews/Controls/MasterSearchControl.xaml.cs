// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Helper.UI.Navig;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Vusic_Player.Pages;
using Vusic_Player.Pages.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class MasterSearchControl : UserControl
    {
        List<AudioTrackLite> rawSongs = new List<AudioTrackLite>();
        public MasterSearchControl()
        {
            InitializeComponent();
            rawSongs = FilesInDatabase.rawSongs;
            lstViewSearchOptions.ItemsSource = searchResultsMaster;
            lstViewSearchOptions.AddHandler(
      UIElement.PreviewKeyDownEvent,
      new KeyEventHandler(lstViewSearchOptions_AlwaysPreviewKeyDown),
      true

 );
        }
        private void lstViewSearchOptions_AlwaysPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            int currentIndex = lstViewSearchOptions.SelectedIndex;
            int totalItems = lstViewSearchOptions.Items.Count;

            // 1. Handle Enter Key
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Debug.WriteLine("entersir");
                if (currentIndex >= 0 && currentIndex < totalItems)
                {
                    if (lstViewSearchOptions.Items[currentIndex] is MasterSearchModel selected)
                    {
                        CommitSelection(selected);
                    }
                }
                return;
            }

            // 2. Handle Up Arrow at the top
            if (e.Key == Windows.System.VirtualKey.Up && currentIndex == 0)
            {
                e.Handled = true;
                lstViewSearchOptions.SelectedIndex = -1;
                asbSearchOptions.Focus(FocusState.Programmatic);
                return;
            }

            // 3. Handle Down Arrow at the bottom (loop back to top)
            if (e.Key == Windows.System.VirtualKey.Down && currentIndex == totalItems - 1)
            {
                e.Handled = true;
                lstViewSearchOptions.SelectedIndex = 0;
                return;
            }

            // 4. Typing while list has focus: transfer input cleanly to AutoSuggestBox
            if ((e.Key >= Windows.System.VirtualKey.A && e.Key <= Windows.System.VirtualKey.Z) ||
                (e.Key >= Windows.System.VirtualKey.Number0 && e.Key <= Windows.System.VirtualKey.Number9) ||
                (e.Key >= Windows.System.VirtualKey.NumberPad0 && e.Key <= Windows.System.VirtualKey.NumberPad9) ||
                e.Key == Windows.System.VirtualKey.Space ||
                e.Key == Windows.System.VirtualKey.Back)
            {
                e.Handled = true; // Consume event to prevent focus conflict / double-firing

                lstViewSearchOptions.SelectedIndex = -1;
                asbSearchOptions.Focus(FocusState.Programmatic);

                if (e.Key == Windows.System.VirtualKey.Back)
                {
                    if (!string.IsNullOrEmpty(asbSearchOptions.Text))
                    {
                        asbSearchOptions.Text = asbSearchOptions.Text[..^1];
                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Space)
                {
                    asbSearchOptions.Text += " ";
                }
                else
                {
                    // Simple character conversion for basic letters/digits
                    char charToAdd = (char)e.Key;
                    asbSearchOptions.Text += char.ToLowerInvariant(charToAdd);
                }

                // Put cursor at end of text
                asbSearchOptions.SelectionStart = asbSearchOptions.Text.Length;
                asbSearchOptions.SelectionLength = 0;
            }
        }
        private void CommitSelection(MasterSearchModel selected)
        {
            popupSearch.IsOpen = false;

        }



        ObservableCollection<MasterSearchModel> searchResultsMaster = new ObservableCollection<MasterSearchModel>();

        private ObservableCollection<MasterSearchModel> GetFilteredResults(string query, int count = 5)
        {
            if (string.IsNullOrWhiteSpace(query)) return new ObservableCollection<MasterSearchModel>();

            var rawQuery = query.Trim();

            var returnableobservable = new ObservableCollection<MasterSearchModel>();

            var rawMedia = FilesInDatabase.rawSongs;
            var rawMediaTitleBoolCheck = rawMedia.All(p => p.Artist?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            Debug.WriteLine("raw media count: " + rawMedia.Count);
            Debug.WriteLine("raw query: " + rawQuery);
            var rawMediaArtist = rawMedia.Where(p => p.Title?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true).Take(count).ToList();
            foreach (var item in rawMediaArtist)
            {
                if (File.Exists(item.FilePath))
                {
                    var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
                    var imgicon = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "musicnoteicon" : "defaultvideo";
                    returnableobservable.Add(new MasterSearchModel { ImageThumbnail = $"ms-appx:///Assets/Themes/{ThemeManager.Instance.GlobalTheme}/{imgicon}.png", ResultMain = item.Title, SubInformation = subinfo, FilePath = item.FilePath });
                }
            }

            //if (rawMediaTitleBoolCheck)
            //{
            //    Debug.WriteLine("TITLE CHECK");
            //    var rawMediaTitle = rawMedia.Where(p => p.Title?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            //    foreach (var item in rawMediaTitle)
            //    {
            //        var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
            //        returnableobservable.Add(new MasterSearchModel { ImageThumbnail = IconHolder.Instance.DefaultArtist, ResultMain = item.Title, SubInformation = subinfo });
            //    }
            //}
            return returnableobservable;
        }

        private async void asbSearchOptions_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = asbSearchOptions.Text.ToLower();

            searchResultsMaster.Clear();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var suggestions = await MasterSearchIndex.ProcessFindQueryAsync(query);
                foreach (var item in suggestions)
                {
                    //     bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
                    var existingitem = searchResultsMaster.FirstOrDefault(p => p.FilePath == item.FilePath);
                    if (existingitem == null)
                    {
                        searchResultsMaster.Add(new MasterSearchModel { FilePath = item.FilePath, ResultMain = item.ResultMain, SubInformation = item.SubInformation, ImageThumbnail = item.ImageThumbnail });
                    }
                }
            }
            else
            {
                btnSearchQuery.Margin = new Thickness(0, 0, 0, 0);
            }
            if (searchResultsMaster.Count == 0 || string.IsNullOrWhiteSpace(query))
            {
                popupSearch.IsOpen = false;
                lstViewSearchOptions.Visibility = Visibility.Collapsed;
            }
            else
            {
                popupSearch.IsOpen = true;
                lstViewSearchOptions.Visibility = Visibility.Visible;

                if (query.Length == 1)
                {
                    asbSearchOptions.Focus(FocusState.Programmatic);

                    // Keep the cursor flashing at the end of the single letter
                    asbSearchOptions.SelectionStart = asbSearchOptions.Text.Length;
                    asbSearchOptions.SelectionLength = 0;
                }
            }
        }


        private void btnSearchQuery_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void lstViewSearchOptions_ItemClick(object sender, ItemClickEventArgs e)
        {
            popupSearch.IsOpen = false;

            var selected = e.ClickedItem as MasterSearchModel;
            if (selected != null)
            {
                Debug.WriteLine("yesssssdshs");
                if (selected.SubInformation == "Artist")
                {
                    Debug.WriteLine("ARTISARTHUR " + selected.ResultMain);
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(ArtistView), selected.ResultMain);
                    }
                }
                else if (selected.SubInformation == "Song")
                {
                    Debug.WriteLine("ARTISARTHUR " + selected.ResultMain);
                    if (File.Exists(selected.FilePath))
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(selected.FilePath);

                        single.Add(new SongModel { FilePath = selected.FilePath, Title = Title, AlbumName = AudioMetadata.Album(selected.FilePath), Artist = AudioMetadata.Artist(selected.FilePath), SongDuration = await AudioMetadata.GetTimeSpanDuration(selected.FilePath) });
                        QueueService.PlayMedia(single, false, false);
                    }
                }
                else if (selected.SubInformation == "Video")
                {
                    Debug.WriteLine("ARTISARTHUR " + selected.ResultMain);
                    if (File.Exists(selected.FilePath))
                    {
                        if (PlayerService.InVideoPage == false)
                        {
                            if (App.NavigationFrame != null)
                                App.NavigationFrame.Navigate(typeof(VideoPlayer), selected.FilePath);
                        }
                        else
                        {
                            PlayerService.OpenPath(selected.FilePath);
                        }
                    }
                }
                else if (selected.SubInformation == "Album")
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(AlbumView), selected.ResultMain);
                    }
                }
                else if (selected.SubInformation == "Genre")
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(GenreView), selected.ResultMain);
                    }
                }
                else if (selected.SubInformation == "Playlist")
                {
                    System.Diagnostics.Debug.WriteLine($"Selected ID: '{selected.PlaylistID}', Name: '{selected.ResultMain}'");
                    var existingplaylist = PlaylistCreationValues.Instance.PlaylistsMaster.FirstOrDefault(p => p.PlaylistId == selected.PlaylistID);
                    if (App.NavigationFrame != null)
                    {
                        if (existingplaylist != null)
                            App.NavigationFrame.Navigate(typeof(PlaylistView), existingplaylist);
                    }
                }
                else if (selected.SubInformation == "Show")
                {
                    System.Diagnostics.Debug.WriteLine($"Selected ID: '{selected.ShowID}', Name: '{selected.ResultMain}'");
                    var existingshow = ShowCreationValues.Instance.ShowsMaster.FirstOrDefault(p => p.ShowID == selected.ShowID);
                    if (App.NavigationFrame != null)
                    {
                        if (existingshow != null)
                            App.NavigationFrame.Navigate(typeof(Show), existingshow);
                    }
                }
                else if (selected.SubInformation == "Folder")
                {
                    System.Diagnostics.Debug.WriteLine($"Selected PATH: '{selected.FolderPath}', Name: '{selected.ResultMain}'");

                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = Path.GetFileName(selected.FolderPath), FolderPath = selected.FolderPath });
                    }
                }
                else if (selected.SubInformation == "Page")
                {
                    if (selected.ResultMain == "Home")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(HomeView));
                        }
                    }
                    else if (selected.ResultMain == "Music Library")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(MusicLibrary));
                        }
                    }
                    else if (selected.ResultMain == "Video Library")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(VideoLibrary));
                        }
                    }
                    else if (selected.ResultMain == "Entire Music Library")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(EntireMusicLibrary));
                        }
                    }
                    else if (selected.ResultMain == "Entire Video Library")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(MusicLibrary));
                        }
                    }
                    else if (selected.ResultMain == "Queue")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(QueuePage));
                        }
                    }
                    else if (selected.ResultMain == "Log")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(LoggerPage));
                        }
                    }
                    else if (selected.ResultMain == "Settings")
                    {
                        if (App.NavigationFrame != null)
                        {
                            App.NavigationFrame.Navigate(typeof(SettingsPage));
                        }
                    }
                }
                else if(selected.SubInformation == "Setting")
                {
                    if (App.NavigationFrame != null)
                    {
                        if(SearchResultsPageState.alreadyNavigatedtoSettingsPage == true)
                        {
                            PENDING
                        }
                        else
                        {
                            SearchResultsPageState.alreadyNavigatedtoSettingsPage = true;
                            App.NavigationFrame.Navigate(typeof(SettingsPage), selected.ResultMain);
                        }
                    }
                }
            }
        }

        private void asbSearchOptions_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                // Do NOT set e.Handled = true; 
                // Just return so default textbox input handling processes the space character
                return;
            }
           
          

            if (e.Key == Windows.System.VirtualKey.Down)
            {
                e.Handled = true; // Prevent cursor from moving in TextBox

                popupSearch.IsOpen = true;
                lstViewSearchOptions.Focus(FocusState.Programmatic);

                // Only force index 0 if nothing is selected yet
                if (lstViewSearchOptions.SelectedIndex == -1)
                {
                    lstViewSearchOptions.SelectedIndex = 0;
                }
            }
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                if (App.NavigationFrame != null)
                {
                    if (SearchResultsPageState.alreadyNavigatedtoSearchResultsPage == false)
                    {
                        App.NavigationFrame.Navigate(typeof(SearchResults), asbSearchOptions.Text);
                        SearchResultsPageState.alreadyNavigatedtoSearchResultsPage = true;
                    }
                    else
                    {
                        if (App.NavigationFrame.Content is SearchResults searchresult)
                        {
                            searchresult.ModifySearchQuery(asbSearchOptions.Text);
                        }
                    }
                }

            }
        }
        private DependencyObject? FindChildElementByName(DependencyObject parent, string name)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element && element.Name == name)
                {
                    return child;
                }

                var result = FindChildElementByName(child, name);

                if (result != null) return result;
            }
            return null;
        }
        private void asbSearchOptions_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Find the DeleteButton by its template name
                var deleteButton = FindChildElementByName(textBox, "DeleteButton");

                if (deleteButton is Button button)
                {
                    // Collapse its visibility so it never takes up space or appears
                    button.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void asbSearchOptions_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(asbSearchOptions.Text))
            {
                btnSearchQuery.Margin = new Thickness(0, 0, 30, 0);
            }
        }

        private void asbSearchOptions_LostFocus(object sender, RoutedEventArgs e)
        {
            btnSearchQuery.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
