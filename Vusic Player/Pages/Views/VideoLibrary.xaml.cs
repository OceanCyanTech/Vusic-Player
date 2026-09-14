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
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Vusic_Player.UI.Dialogs;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    public sealed partial class VideoLibrary : Page
    {
        public VideoLibrary()
        {
            InitializeComponent();
        }
        private async void btnOpenVideo_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var media = await MediaPicker.PickSingleVideo(App.MainWindowInstance, "Open Video");
            if (media != null)
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
        }
        PlaylistCreationValues PlaylistInstance => PlaylistCreationValues.Instance;
        bool iscreating = false;

        private void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "Playlist", "", "", "", "", new PlaylistItem(), false, false);
            Debug.WriteLine("BTNNEWPLAYLIST");
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                if (iscreating) return;
                try
                {
                    iscreating = true;
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    string baseName = PlaylistInstance.PlaylistName.Trim();

                    if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                    string finalName = baseName;
                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                    var newplaylist = new PlaylistItem { PlaylistName = finalName, PlaylistGenre = PlaylistInstance.Genre, plthumb = PlaylistInstance.PlCover, PlaylistId = PlaylistInstance.PlaylistID, DateCreation = PlaylistInstance.CreationDate, PlaylistCount = PlaylistInstance.PlaylistCount, SongsPaths = new HashSet<string>(PlaylistInstance.MediaPaths), Thumbnail = PlaylistInstance.Thumbnail };
                    Debug.WriteLine(newplaylist.PlaylistName);
                    PlaylistInstance.PlaylistsMaster.Add(newplaylist);

                    currentSettings.SavedPlaylists.Add(newplaylist);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                }
                catch (Exception ex)
                {
                    Logger.Log("An unexpected error occured: " + ex.Message, "PlaylistCreate.MusicLibPage", Logger.LogLevelType.Error);
                }
                finally
                {
                    iscreating = false;
                }
            });
        }
        private void OceanContentDialog_PrimaryRequested()
        {
         //   PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
    //        MainWindow.ShowWindow();

        }
        private void hypViewEntireLib_Click(object sender, RoutedEventArgs e)
        {
        //    LibraryStore.IsMusicLibrary = false;
            this.Frame.Navigate(typeof(EntireVideoLibrary), "Videos");
        }

        private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (!this.IsLoaded) return;

            if (sender != expRecents) expRecents.IsExpanded = false;

            if (sender != expRecommended) expRecommended.IsExpanded = false;

            if (sender != expVideoPlaylists) expVideoPlaylists.IsExpanded = false;

        }
        ShowCreationValues Instance => ShowCreationValues.Instance;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Show Model", "Create", "", "Cancel", OceanDialogWindow.ContentType.ShowModel, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", new PlaylistItem(), false, false);
            Debug.WriteLine("BTNNEWSHOW");
            OceanContentDialog.PrimaryRequested += (async() =>
            {
                var newshow = new Show { Name = Instance.ShowName, Description = Instance.Description, Genre = Instance.Genre, Creators = Instance.Creators, Crew = Instance.Cast, Directory = Instance.Directory, ShowID = Instance.ShowID, Tags = Instance.Tags, Poster = Instance.PosterPath };
                Debug.WriteLine(newshow.Name);
                Instance.ShowsMaster.Add(newshow);
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.Shows.Add(newshow);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();
            });
        }

        private void OceanContentDialog_PrimaryRequested1()
        {
            Debug.WriteLine("Yes create");
            PlaylistCreation.CallShowCreation();
            OceanContentDialog.HideDlg();
           MainWindow.ShowWindow();
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            expRecommended.IsExpanded = true;
        }

        private void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            expVideoPlaylists.IsExpanded = true;
        }

        private void TextBlock_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            expShows.IsExpanded = true;
        }
    }

}
