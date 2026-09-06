using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

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
                    var seasoncountstring = $"{show.SeasonCount} {(show.SeasonCount == 1 ? "season" : "seasons")}";
                    ShowsList.Add(new Show { Poster = show.Poster ?? "ms-appx:///Assets/appicon.png", ShowID = show.ShowID, SeasonCountString = seasoncountstring, Name = show.Name, Description = show.Description, Crew = show.Crew, Creators = show.Creators, Tags = show.Tags, Directory = show.Directory });
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
            grdLoading.Visibility = Visibility.Collapsed;
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
                    var exist = ShowsList.FirstOrDefault(p => p.ShowID == PlaylistCreation.showitem.ShowID);
                    if (exist == null)
                    {
                        ShowsList.Add(PlaylistCreation.showitem);

                        currentSettings.Shows.Add(PlaylistCreation.showitem);
                        MasterSearchIndex.ShowsMaster.Add(PlaylistCreation.showitem);
                    }
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
            bool isChecked = chkSelect.IsChecked ?? false;

            grdViewShows.SelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
            ttEditShow.IsOpen = false;

        }

        private void chckSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAll.IsChecked == true)
                grdViewShows.SelectAll();
            else
                grdViewShows.DeselectAll();
        }

        private void btnRemoveShows_Click(object sender, RoutedEventArgs e)
        {
            var selected = grdViewShows.SelectedItems.Cast<Show>().ToList();
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "deleteicon", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want to delete the selected shows? This cannot be undone.", "warning");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += (() =>
            {
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();
                foreach (var item in selected)
                {
                    ShowsList.Remove(item);
                }
                ttEditShow.IsOpen = false;
            });
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



        private void MenuFlyout_Opened(object sender, object e)
        {

        }

        private void mnftOpenShow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is Show show)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(ShowModel), show);
                }
            }
        }

        private async void mnftPlayShow_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuFlyoutItem mnft && mnft.DataContext is Show show)) return;

            string rootPath = show.Directory;
            if (!Directory.Exists(rootPath)) return;

            // Capture UI text before entering the background thread
            string showNameText = txtShowName.Text;
            ttLoadingShow.IsOpen = true;

            try
            {
                // 1. Move ALL disk I/O, Regex, and metadata extraction to a background thread
                var data = await Task.Run(() =>
                {
                    var localSeasons = new List<PlaylistItem>();
                    var primaryFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();
                    primaryFolders.Insert(0, rootPath);

                    string pattern = @"\b(season\s*|s)(\d+)\b";
                    var videoExtensions = Extensions.VideoExtensions.List.Select(ext => ext.ToLower()).ToHashSet();

                    // --- SCAN DIRECTORIES ---
                    foreach (string path in primaryFolders)
                    {
                        string folderName = Path.GetFileName(path);
                        Match match = Regex.Match(path == rootPath ? new DirectoryInfo(rootPath).Name : folderName, pattern, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                            string actualContentPath = path;

                            // Query directory ONCE for all files, then filter in memory (Much faster I/O)
                            var foundFiles = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                                                      .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                                                      .ToList();

                            if (foundFiles.Any())
                            {
                                int episodeCount = foundFiles.Count;
                                actualContentPath = Path.GetDirectoryName(foundFiles.First())!;

                                var existingSeason = localSeasons.FirstOrDefault(p => p.PlaylistName == $"Season {seasonNum}");
                                if (existingSeason == null)
                                {
                                    localSeasons.Add(new PlaylistItem
                                    {
                                        PlaylistName = $"Season {seasonNum}",
                                        PlaylistCount = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}",
                                        PlaylistId = actualContentPath,
                                        SeasonNumber = seasonNum
                                    });
                                }
                                else
                                {
                                    existingSeason.PlaylistCount = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}";
                                    existingSeason.PlaylistId = actualContentPath;
                                }
                            }
                        }
                    }

                    if (localSeasons.Count == 0) return (Seasons: localSeasons, Episodes: new List<EpisodeModel>());

                    var seasonsRearranged = localSeasons.OrderBy(p => p.SeasonNumber).ToList();
                    for (int i = 0; i < seasonsRearranged.Count; i++) seasonsRearranged[i].SeasonIndex = i;

                    var firstseason = seasonsRearranged[0];
                    if (!(firstseason.PlaylistId is string folderpath) || !Directory.Exists(folderpath))
                        return (Seasons: seasonsRearranged, Episodes: new List<EpisodeModel>());

                    // --- PARSE EPISODES ---
                    var episodePatterns = new List<string> {
                @"(?i)(?:s\d+)?e(\d+)\b", @"(?i)e(\d+)(?:[-_]?e?(\d+))?\b",
                @"(?i)\b(?:ep|episode)(?:\s*|\s*\.\s*)(\d+)\b", @"(?i)\b\d+x(\d+)\b",
                @"\[(\d+)\]", @"\((\d+)\)", @"(?<=\s+|-|_|#)(\d+)(?=\.\w+$|\s+|-|_)"
            };

                    var videoFiles = Directory.EnumerateFiles(folderpath)
                                              .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                                              .OrderBy(file => file)
                                              .ToList();

                    var episodes = new List<EpisodeModel>();
                
                    for (int i = 0; i < videoFiles.Count; i++)
                    {
                        string filePath = videoFiles[i];
                        string episodeNumber = "Unknown";

                        foreach (var pat in episodePatterns)
                        {
                            Match match = Regex.Match(Path.GetFileName(filePath), pat, RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                var validGroup = match.Groups.Cast<Group>().Skip(1).FirstOrDefault(g => g.Success && !string.IsNullOrEmpty(g.Value));
                                if (validGroup != null)
                                {
                                    episodeNumber = validGroup.Value;
                                    break;
                                }
                            }
                        }

                        episodes.Add(new EpisodeModel
                        {
                            EpisodeName = $"Episode {episodeNumber}",
                        
                            FilePath = filePath,
                            CurrentShowDirectory = Path.GetDirectoryName(filePath)
                        });
                    }

                    
                    return (Seasons: seasonsRearranged, Episodes: episodes);
                });

                if (data.Episodes == null || data.Episodes.Count == 0) return;

           
                // 3. Batch queue additions to avoid redundant O(N) loops
                var songModels = data.Episodes.Select(item => new SongModel
                {
                    Title = Path.GetFileName(item.FilePath),
                    VisibilityofVideoInfo = Visibility.Visible,
                    VisibilityofAudioMeta = Visibility.Collapsed,
                    Glyph = "\uE8B2",
                    IsAudioItem = false,
                    FilePath = item.FilePath
                }).ToList();

                foreach (var song in songModels)
                {
                    QueueService.VusicQueue.Add(song);
                    QueueService.VusicQueueNext.Add(song);
                }

                // 4. Navigate
                if (data.Seasons.Count > 0 && App.NavigationFrame != null)
                {
                    QueueService.VusicQueueNext.RemoveAt(0);
                    App.NavigationFrame.Navigate(typeof(VideoPlayer), new ShowData
                    {
                        ShowName = showNameText,
                        episodes = data.Episodes,
                        ShowID = show.ShowID,
                        seasons = data.Seasons,
                        CurrentSeasonNumber = data.Seasons[0].SeasonNumber,
                        CurrentSeasonDirectory = data.Seasons[0].PlaylistId
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing show: {ex.Message}");
            }
            finally
            {
                ttLoadingShow.IsOpen = false;
            }
        }
        private async void mnftEditShow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is Show show)
            {
                ttEditShow.IsOpen = true;
                imgShowPoster.Source = new BitmapImage(new Uri(show.Poster));
                txtShowName.Text = show.Name;
                txtDescription.Text = show.Description;
                txtTags.Text = show.Tags;
                dtRelease.Date = show.ReleaseDate;
                txtFolderPath.Text = show.Directory;
                txtGenre.Text = show.Genre;
                txtCreators.Text = show.Creators;
                txtCast.Text = show.Crew;

                btnSaveShowEdited.Click += (async (object sender, RoutedEventArgs e) =>
                {
                    if(txtShowName.Text == "")
                    {
                        txtShowName.Text = show.Name;
                    }
                    show.Name = txtShowName.Text;
                    show.Description = txtDescription.Text;
                    show.Tags = txtTags.Text;
                    show.ReleaseDate = dtRelease.Date;
                    show.Crew = txtCast.Text;
                    show.Creators = txtCreators.Text;
                    show.Genre = txtGenre.Text;
                    show.Directory = txtFolderPath.Text;
                    show.Poster = posterpath;
                    
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var existingshow = currentSettings.Shows.FirstOrDefault(p => p.ShowID == show.ShowID);
                    if(existingshow != null)
                    {
                        existingshow = show;
                        await SettingsLoader.SaveSettingsAsync(currentSettings);                    
                    }
                });
            }
        }

        private void mnftDeleteShow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is Show show)
            {
                if (App.MainWindowInstance == null) return;
                OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "deleteicon", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want to delete the show '{show.Name}'? This cannot be undone.", "warning");
                OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
                OceanContentDialog.PrimaryRequested += (() =>
                {
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                    ShowsList.Remove(show);
                    ttEditShow.IsOpen = false;
                });
            }
        }

        private void btnNewShow_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Show Model", "Create", "", "Cancel", OceanDialogWindow.ContentType.ShowModel, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", new PlaylistItem(), false, false);

            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;
        }

        private void OceanContentDialog_PrimaryRequested1()
        {
            Debug.WriteLine("Yes create");
            PlaylistCreation.CallShowCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }
        string posterpath = "";
        private async void btnUploadShowPoster_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose poster");
            if (image != null)
            {
                imgShowPoster.Source = new BitmapImage(new Uri(image.Path));
                posterpath = image.Path;
            }
         
        }

        private async void btnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.OceanDialogInstance, "Choose location", Windows.Storage.Pickers.PickerLocationId.VideosLibrary);
            if (folder != null)
            {
                txtFolderPath.Text = folder.Path;
                ToolTipService.SetToolTip(txtFolderPath, folder.Name);
            }
        }

        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ttEditShow.IsOpen = false;
        }
    }
}
