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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Windows.Devices.Power;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Vusic_Player.Pages.Views
{

    public sealed partial class ShowModel : Page
    {
        Show? currentshow;
        ObservableCollection<ArtistShow> creators = new();
        ObservableCollection<ArtistShow> crewlist = new();
        ObservableCollection<EpisodeModel> EpisodesList = new();
        ObservableCollection<PlaylistItem> seasons = new();
        public ShowModel()
        {
            InitializeComponent();
            //     EpisodesList.CollectionChanged += EpisodesList_CollectionChanged;
        }

        private void EpisodesList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            txtEpisodeCount.Text = $"{EpisodesList.Count} {(EpisodesList.Count == 1 ? "episode" : "episodes")}";
        }


        public bool HasSubfolders(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return false;

            // EnumerateDirectories avoids loading all folder names into memory at once
            return Directory.EnumerateDirectories(folderPath).Any();
        }
        private void LoadCreators(Show show)
        {
            if (show.Creators != null)
            {
                creators.Clear();
                var splitcreators = show.Creators.Split(",");
                foreach (var creator in splitcreators)
                {
                    creators.Add(new ArtistShow { ArtistName = creator });
                }
                if (creators.Count != 0)
                {
                    grdCreators.Visibility = Visibility.Visible;

                    grdViewCreators.ItemsSource = creators;
                }
                else
                {
                    grdCreators.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void LoadCrew(Show show)
        {
            if (show.Crew != null)
            {
                crewlist.Clear();
                var splitcrew = show.Crew.Split(",");
                foreach (var crew in splitcrew)
                {
                    crewlist.Add(new ArtistShow { ArtistName = crew });
                }
                if (crewlist.Count != 0)
                {
                    grdCrew.Visibility = Visibility.Visible;

                    grdViewCast.ItemsSource = crewlist;
                }
                else
                {
                    grdCrew.Visibility = Visibility.Collapsed;
                }
            }

        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Show show)
            {
                currentshow = show;
                if (show.Poster != null)
                {
                    imgPoster.Source = new BitmapImage(new Uri(show.Poster));
                }
                txtShowName.Text = show.Name ?? "Show";
                if (show.isSeasonPage)
                {
                    ShowMainPanel.Visibility = Visibility.Collapsed;
                    SeasonPanel.Visibility = Visibility.Visible;

                    Debug.WriteLine("Yes Season Page");
                    if (show.Season is PlaylistItem pl && pl.PlaylistId is string folderpath)
                    {
                        Debug.WriteLine("Yes DD");
                        txtSeasonHeader.Text = pl.PlaylistName;
                        if (Directory.Exists(folderpath))
                        {
                            var videoExtensions = Extensions.VideoExtensions.List
            .Select(ext => ext.ToLower())
           .ToHashSet();
                            var episodePatterns = new List<string>
{
    // 1. Standard SxxExx or just Exx (Looks for 'E' or 'EP' optionally preceded by 'Sxx')
    @"(?i)(?:s\d+)?e(\d+)\b",

    // 2. Multi-episode format: E02-E03, E02E03, e02_03
    @"(?i)e(\d+)(?:[-_]?e?(\d+))?\b",

    // 3. Standard text 'episode' or 'ep' followed by numbers (e.g., Ep.01, Episode 1)
    @"(?i)\b(?:ep|episode)(?:\s*|\s*\.\s*)(\d+)\b",

    // 4. X / Cross format: S01x02, 1x02, 1x2
    @"(?i)\b\d+x(\d+)\b",

    // 5. Bracketed numbers (Anime style): [02], (02)
    @"\[(\d+)\]",
    @"\((\d+)\)",

    // 6. Absolute / Standalone numbers: "Show - 02.mp4" 
    @"(?<=\s+|-|_|#)(\d+)(?=\.\w+$|\s+|-|_)"
};
                            //                            // 2. Get only the files that match your video extensions
                            //                            // 1. Grab files and immediately turn it into a list to prevent multiple enumerations

                            //                            var videoFiles = Directory.EnumerateFiles(folderpath)

                            //                            .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))

                            //                            .OrderBy(file => file) // <-- Handles the sorting perfectly right here

                            //                            .ToList();



                            //                            EpisodesList.Clear();



                            //                            if (lstViewEpisodes.ItemsSource == null)

                            //                            {

                            //                                lstViewEpisodes.ItemsSource = EpisodesList;

                            //                            }



                            //                            // 2. Offload the entire loop processing to a background thread so the UI never drops frames

                            //                            await Task.Run(async () =>

                            //                            {

                            //                                // Limit concurrency to 3 tasks at a time to prevent FFmpeg from crashing/overloading

                            //                                using var semaphore = new SemaphoreSlim(3);

                            //                                var processingTasks = new List<Task>();



                            //                                foreach (var filePath in videoFiles)

                            //                                {

                            //                                    await semaphore.WaitAsync();



                            //                                    var task = Task.Run(async () =>

                            //                                    {

                            //                                        try

                            //                                        {

                            //                                            string fileName = Path.GetFileName(filePath);

                            //                                            string episodeNumber = "Unknown";



                            //                                            foreach (var pattern in episodePatterns)

                            //                                            {

                            //                                                Match match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);

                            //                                                if (match.Success)

                            //                                                {

                            //                                                    episodeNumber = match.Groups[match.Groups.Count - 1].Value;

                            //                                                    break;

                            //                                                }

                            //                                            }



                            //                                            string description = "No description available";

                            //                                            using (var tagfile = TagLib.File.Create(filePath))

                            //                                            {

                            //                                                if (!string.IsNullOrEmpty(tagfile.Tag.Comment))

                            //                                                {

                            //                                                    description = tagfile.Tag.Comment;

                            //                                                }

                            //                                            }



                            //                                            var file = await StorageFile.GetFileFromPathAsync(filePath);

                            //                                            var videoproperties = await file.Properties.GetVideoPropertiesAsync();

                            //                                            string durationString = videoproperties.Duration.ToString(@"hh\:mm\:ss");



                            //                                            // Create the model container (Thumbnail is initially null)

                            //                                            var newEpisode = new EpisodeModel

                            //                                            {

                            //                                                EpisodeName = $"Episode {episodeNumber}",

                            //                                                Description = description,

                            //                                                Duration = durationString,

                            //                                                FilePath = filePath

                            //                                            };



                            //                                            // Push to the UI thread to add the item and fetch its thumbnail safely

                            //                                            DispatcherQueue.TryEnqueue(async () =>

                            //                                            {

                            //                                                // Items are added in sorted order because videoFiles was pre-sorted

                            //                                                EpisodesList.Add(newEpisode);



                            //                                                // Fetch the thumbnail. Because of INotifyPropertyChanged,

                            //                                                // the thumbnail will pop into view the moment this finishes.

                            //                                                var thumbnail = await FileThumbnailObtain.GetVideoFrameAsync(filePath);

                            //                                                newEpisode.Thumbnail = thumbnail;

                            //                                            });

                            //                                        }

                            //                                        catch (Exception ex)

                            //                                        {

                            //                                            System.Diagnostics.Debug.WriteLine($"Error processing {filePath}: {ex.Message}");

                            //                                        }

                            //                                        finally

                            //                                        {

                            //                                            semaphore.Release();

                            //                                        }

                            //                                    });



                            //                                    processingTasks.Add(task);

                            //                                }



                            //                                // Await all tasks to guarantee absolutely nothing gets dropped or forgotten

                            //                                await Task.WhenAll(processingTasks);
                            //var sorted = EpisodesList.OrderBy(p => p.EpisodeName).ToList();
                            //for (int i = 0; i < sorted.Count; i++)
                            //{
                            //    var oldIndex = EpisodesList.IndexOf(sorted[i]);
                            //    var newIndex = i;

                            //    if (oldIndex != newIndex)
                            //    {
                            //        EpisodesList.Move(oldIndex, newIndex);
                            //    }
                            //}

                            //});
                            var videoFiles = Directory.EnumerateFiles(folderpath)
            .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
            .OrderBy(file => file)
            .ToList();

                            EpisodesList.Clear();

                            if (lstViewEpisodes.ItemsSource == null)
                            {
                                lstViewEpisodes.ItemsSource = EpisodesList;
                            }
                            ShowManager.currentseason = pl.SeasonIndex;
                            currentseasonindex = pl.SeasonIndex;
                            Debug.WriteLine(ShowManager.currentseason + " is the current season index");
                            // 1. Pre-populate the list on the UI thread so items stay perfectly sorted
                            var episodePlaceholders = new List<EpisodeModel>();

                            foreach (var filePath in videoFiles)
                            {
                                string fileName = Path.GetFileName(filePath);
                                string episodeNumber = "Unknown";
                                Debug.WriteLine($"Processing File: {fileName}");

                                // 3. Evaluate each regex pattern
                                foreach (var pattern in episodePatterns)
                                {
                                    Match match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
                                    if (match.Success)
                                    {
                                        var validGroups = match.Groups.Cast<Group>()
                                                                      .Skip(1)
                                                                      .Where(g => g.Success && !string.IsNullOrEmpty(g.Value))
                                                                      .ToList();

                                        Debug.WriteLine($"  [Check] Pattern '{pattern}' matched! Found {validGroups.Count} valid capture groups.");

                                        if (validGroups.Any())
                                        {
                                            episodeNumber = validGroups.First().Value;
                                            Debug.WriteLine($"  -> Match Found! Episode: {episodeNumber}");
                                            break;
                                        }
                                    }
                                }
                                var newEpisode = new EpisodeModel
                                {
                                    EpisodeName = $"Episode {episodeNumber}",
                                    Description = "Loading...",
                                    Duration = "--:--:--",
                                    FilePath = filePath,

                                    CurrentShowDirectory = Path.GetDirectoryName(filePath)
                                };

                                EpisodesList.Add(newEpisode);
                                episodePlaceholders.Add(newEpisode);
                            }

                            // 2. Offload processing to a background thread
                            await Task.Run(async () =>
                            {
                                using var semaphore = new SemaphoreSlim(3);
                                var processingTasks = new List<Task>();

                                for (int i = 0; i < videoFiles.Count; i++)
                                {
                                    var filePath = videoFiles[i];
                                    var targetEpisodeModel = episodePlaceholders[i];

                                    await semaphore.WaitAsync();
                                    string description = "No description available";
                                    using (var tagfile = TagLib.File.Create(filePath))
                                    {
                                        if (!string.IsNullOrEmpty(tagfile.Tag.Comment))
                                        {
                                            description = tagfile.Tag.Comment;
                                        }
                                    }


                                    var task = Task.Run(async () =>
                                    {
                                        try
                                        {
                                            // A. HEAVY IO: Read TagLib (Safe for background thread)


                                            // B. WINRT CALL: Get Video Duration
                                            // Since StorageFile demands an STA thread, we hop back to the UI thread briefly
                                            string durationString = "--:--:--";
                                            var tcsDuration = new TaskCompletionSource<string>();
                                            var file = await StorageFile.GetFileFromPathAsync(filePath);
                                            var videoproperties = await file.Properties.GetVideoPropertiesAsync();
                                            tcsDuration.SetResult(videoproperties.Duration.ToString(@"hh\:mm\:ss"));

                                            try { durationString = await tcsDuration.Task; } catch { /* Fallback to default */ }
                                            // C. HEAVY IO: Run FFmpeg to extract the image (Safe for background thread)
                                            string tempFile = await FileThumbnailObtain.ExtractVideoFrameToFileAsync(filePath);

                                            // D. WINRT CALL: Convert the temp image file into a BitmapImage
                                            // BitmapImage MUST be created and assigned on the UI thread
                                            DispatcherQueue.TryEnqueue(async () =>
                                            {


                                                if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                                                {
                                                    try
                                                    {
                                                        targetEpisodeModel.Description = description;
                                                        targetEpisodeModel.Duration = durationString;
                                                        var bitmap = new BitmapImage();
                                                        using (var stream = File.OpenRead(tempFile))
                                                        {
                                                            await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                                                        }
                                                        targetEpisodeModel.Thumbnail = bitmap;

                                                        // Clean up the temp file after loading it into memory
                                                        File.Delete(tempFile);
                                                    }
                                                    catch (Exception bitmapEx)
                                                    {
                                                        Debug.WriteLine($"Bitmap Load Error: {bitmapEx.Message}");
                                                        targetEpisodeModel.Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
                                                    }
                                                }
                                                else
                                                {
                                                    targetEpisodeModel.Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
                                                }
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Error processing {filePath}: {ex.Message}");
                                        }
                                        finally
                                        {
                                            semaphore.Release();
                                        }
                                    });

                                    processingTasks.Add(task);
                                }

                                await Task.WhenAll(processingTasks);
                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    txtEpisodeCount.Text = $"{videoFiles.Count} {(videoFiles.Count == 1 ? "episode" : "episodes")}";

                                });
                            });
                        }
                    }
                    return;

                }
                ShowMainPanel.Visibility = Visibility.Visible;
                SeasonPanel.Visibility = Visibility.Collapsed;

                txtDescription.Text = show.Description ?? "";
                txtShowName.Width = imgPoster.Width;
                txtReleaseDate.Text = show.ReleaseDate.ToString("dd MMMM yyyy");
                txtGenre.Text = show.Genre;
                LoadCreators(show);
                LoadCrew(show);
                if (show.Directory == null) return;
                Debug.WriteLine("Negative 2 check");
                string rootPath = show.Directory;

                if (Directory.Exists(rootPath))
                {
                    // 1. Only get the top-level folders (e.g., "Season 1", "Season 2", "Season 3")
                    var primaryFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();
                    primaryFolders.Insert(0, rootPath);

                    string pattern = @"\b(season\s*|s)(\d+)\b";

                    foreach (string path in primaryFolders)
                    {
                        string folderName = Path.GetFileName(path);
                        Match match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);

                        if (path == rootPath) match = Regex.Match(new DirectoryInfo(rootPath).Name, pattern, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                            string seasonName = $"Season {seasonNum}";

                            int episodeCount = 0;

                            // This variable will track the actual deep folder where files are found!
                            string actualContentPath = path;

                            foreach (var ext in Extensions.VideoExtensions.List)
                            {
                                string searchPattern = $"*{ext.ToLower()}";

                                // Get the full path details of any matching video files inside
                                var foundFiles = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).ToList();

                                if (foundFiles.Any())
                                {
                                    episodeCount += foundFiles.Count;

                                    // Grab the directory name of the first video file found. 
                                    // This is guaranteed to be the real folder containing the episodes!
                                    actualContentPath = Path.GetDirectoryName(foundFiles.First())!;
                                }
                            }

                            string episodeCountString = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}";

                            var existingSeason = seasons.FirstOrDefault(p => p.PlaylistName == seasonName);
                            if (existingSeason == null)
                            {
                                seasons.Add(new PlaylistItem
                                {
                                    PlaylistName = seasonName,
                                    PlaylistCount = episodeCountString,

                                    // SAVE THIS: Points exactly to "Season 3\Extra Subfolder" if files are deep
                                    PlaylistId = actualContentPath,

                                    SeasonNumber = seasonNum
                                });
                            }
                            else
                            {
                                existingSeason.PlaylistCount = episodeCountString;
                                existingSeason.PlaylistId = actualContentPath; // Update path if found
                            }
                        }
                    }
                    // Update WinUI 3 UI Elements
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var shows = currentSettings.Shows;
                    var exist = shows.FirstOrDefault(p => p.Name == show.Name);
                    if (exist == null) return;
                    var Paths = exist.AddedSeasons;
                    foreach (var path in Paths)
                    {
                        var existingseason = seasons.FirstOrDefault(p => p.PlaylistId == path);
                        if (existingseason == null)
                        {
                            AddSeasonToList(path);
                        }
                    }
                    CheckForUnlinkedSeasons();
                    txtSeasonCount.Text = $"• {seasons.Count} {(seasons.Count == 1 ? "season" : "seasons")}";

                    if (exist != null)
                    {
                        exist.SeasonCount = seasons.Count;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    if (seasons.Count != 0)
                    {
                        var seasonsRearranged = seasons.OrderBy(p => p.SeasonNumber).ToList();
                        for (int i = 0; i < seasonsRearranged.Count; i++)
                        {
                            if (seasonsRearranged[i] != null)
                            {
                                seasonsRearranged[i].SeasonIndex = i;
                            }
                        }
                        grdViewSeasons.ItemsSource = null;
                        grdViewSeasons.ItemsSource = seasonsRearranged;
                        grdViewSeasons.Visibility = Visibility.Visible;
                    }
                }
            }
            base.OnNavigatedTo(e);
        }
        private async void CheckForUnlinkedSeasons()
        {
            if (currentshow == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            var current = shows.FirstOrDefault(p => p.Name == currentshow.Name);
            if (current != null)
            {
                foreach (var path in current.UnlinkedSeasons)
                {
                    var existingseason = seasons.FirstOrDefault(k => k.PlaylistId == path);
                    if (existingseason != null)
                    {
                        seasons.Remove(existingseason);
                    }
                }
            }
            txtSeasonCount.Text = $"• {seasons.Count} {(seasons.Count == 1 ? "season" : "seasons")}";

        }
        bool isEditMode = false;
        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            if (isEditMode)
            {

                var currentsettings = await SettingsLoader.LoadSettingsAsync();
                var shows = currentsettings.Shows;
                var exist = shows.FirstOrDefault(p => p.Name == currentshow.Name);
                if (exist != null)
                {
                    exist.Description = txtDescription.Text = txtEditableDescription.Text;
                }
                await SettingsLoader.SaveSettingsAsync(currentsettings);
                txtEditableDescription.Visibility = Visibility.Collapsed;
                txtDescription.Visibility = Visibility.Visible;
                isEditMode = false;
            }
            else
            {
                txtEditableDescription.Text = txtDescription.Text;


                isEditMode = true;
                txtEditableDescription.Visibility = Visibility.Visible;
                txtDescription.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnAddSeason_Click(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose folder to add as season", Windows.Storage.Pickers.PickerLocationId.VideosLibrary);
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            var currentShow = shows.FirstOrDefault(p => p.Name == currentshow.Name);
            if (currentShow == null) return;
            if (folder != null)
            {
                Debug.WriteLine("1TEST");
                var existingfolder = currentShow.AddedSeasons.FirstOrDefault(p => p == folder.Path);
                var existingSeason = seasons.FirstOrDefault(k => k.PlaylistId == folder.Path);
                if (existingSeason != null) return;
                Debug.WriteLine("2TEST");

                if (existingfolder != null) return;
                Debug.WriteLine("3TEST");

                currentShow.AddedSeasons.Add(folder.Path);
                AddSeasonToList(folder.Path);
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        private void AddSeasonToList(string path)
        {
            if (Directory.Exists(path))
            {
                Debug.WriteLine("3TEST");

                if (currentshow == null) return;
                Debug.WriteLine("4TEST");

                if (currentshow.Directory == null) return;
                Debug.WriteLine("5TEST");

                string pattern = @"\b(season\s*|s)(\d+)\b";

                string rootPath = currentshow.Directory;
                string folderName = Path.GetFileName(path);
                Match match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);

                if (path == rootPath) match = Regex.Match(new DirectoryInfo(rootPath).Name, pattern, RegexOptions.IgnoreCase);
                Debug.WriteLine("6TEST");

                if (match.Success)
                {
                    Debug.WriteLine("7TEST");

                    int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                    string seasonName = $"Season {seasonNum}";

                    int episodeCount = 0;

                    // This variable will track the actual deep folder where files are found!
                    string actualContentPath = path;

                    foreach (var ext in Extensions.VideoExtensions.List)
                    {
                        string searchPattern = $"*{ext.ToLower()}";
                        Debug.WriteLine("8TEST");

                        // Get the full path details of any matching video files inside
                        var foundFiles = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).ToList();

                        if (foundFiles.Any())
                        {
                            Debug.WriteLine("9TEST");

                            episodeCount += foundFiles.Count;

                            // Grab the directory name of the first video file found. 
                            // This is guaranteed to be the real folder containing the episodes!
                            actualContentPath = Path.GetDirectoryName(foundFiles.First())!;
                        }
                    }

                    string episodeCountString = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}";

                    var existingSeason = seasons.FirstOrDefault(p => p.PlaylistName == seasonName);
                    if (existingSeason == null)
                    {
                        Debug.WriteLine("10TEST");
                        if (Directory.Exists(actualContentPath))
                        {
                            seasons.Add(new PlaylistItem
                            {
                                PlaylistName = seasonName,
                                PlaylistCount = episodeCountString,

                                // SAVE THIS: Points exactly to "Season 3\Extra Subfolder" if files are deep
                                PlaylistId = actualContentPath,

                                SeasonNumber = seasonNum
                            });
                        }
                    }
                    else
                    {
                        Debug.WriteLine("11TEST");

                        existingSeason.PlaylistCount = episodeCountString;
                        existingSeason.PlaylistId = actualContentPath; // Update path if found
                    }
                }
                var seasonsRearranged = seasons.OrderBy(p => p.SeasonNumber).ToList();
                for (int i = 0; i < seasonsRearranged.Count; i++)
                {
                    if (seasonsRearranged[i] != null)
                    {
                        seasonsRearranged[i].SeasonIndex = i;
                    }
                }
                grdViewSeasons.ItemsSource = null;
                grdViewSeasons.ItemsSource = seasonsRearranged;
                txtSeasonCount.Text = $"• {seasons.Count} {(seasons.Count == 1 ? "season" : "seasons")}";
                CheckForUnlinkedSeasons();
            }
        }
        private void ppArtist_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

        }
        int currentseasonindex = 0;
        private void grdViewSeasons_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PlaylistItem season)
            {
                if (currentshow == null) return;
                var showitemtotransfer = new Show { Name = currentshow.Name, Poster = currentshow.Poster, Season = season, isSeasonPage = true };
                this.Frame.Navigate(typeof(ShowModel), showitemtotransfer);
            }
        }

        private void grdViewSeasons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is EpisodeModel episode && episode.FilePath is string filepath)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    //          wind.ShowFileInfo(filepath);
                }
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is EpisodeModel episode && episode.FilePath is string filepath)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    //              wind.ShowFileInfo(filepath);
                }
            }
        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {

        }


        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {

            var observablesongcollection = new ObservableCollection<SongModel>();

            foreach (var item in EpisodesList)
            {
                observablesongcollection.Add(new SongModel { Title = Path.GetFileName(item.FilePath), VisibilityofVideoInfo = Visibility.Visible, VisibilityofAudioMeta = Visibility.Collapsed, Glyph = "\uE8B2", IsAudioItem = false, FilePath = item.FilePath });
            }
            foreach (var item in observablesongcollection)
            {
                QueueService.VusicQueue.Add(item);
            }
            foreach (var item in observablesongcollection)
            {
                QueueService.VusicQueueNext.Add(item);
            }
            QueueService.VusicQueueNext.RemoveAt(0);
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(VideoPlayer), EpisodesList[0]);
            }
            var first = EpisodesList[0];
            if (first.FilePath != null)
            {
                Debug.WriteLine("Yesa");
                Debug.WriteLine(first.FilePath);
                ShowManager.LoadAvailableShow(first.FilePath);
                ShowManager.totalepisodecount = EpisodesList.Count;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Play clicked on single item
            if (sender is Button btn && btn.DataContext is EpisodeModel episode)
            {
                if (App.NavigationFrame != null)
                {
                    if (episode.FilePath == null) return;
                    ShowManager.LoadAvailableShow(episode.FilePath);

                    QueueService.VusicQueue.Clear();
                    QueueService.VusicQueueNext.Clear();
                    var observablesongcollection = new ObservableCollection<SongModel>();

                    foreach (var item in EpisodesList)
                    {
                        observablesongcollection.Add(new SongModel { Title = Path.GetFileName(item.FilePath), VisibilityofVideoInfo = Visibility.Visible, VisibilityofAudioMeta = Visibility.Collapsed, Glyph = "\uE8B2", IsAudioItem = false, FilePath = item.FilePath });
                    }
                    foreach (var item in observablesongcollection)
                    {
                        QueueService.VusicQueue.Add(item);
                    }
                    foreach (var item in observablesongcollection)
                    {
                        QueueService.VusicQueueNext.Add(item);
                    }

                    var exist = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == episode.FilePath);
                    if (exist != null)
                    {
                        int indexbefore = QueueService.VusicQueueNext.IndexOf(exist);

                        // Ensure the item was actually found (-1 means not found)
                        if (indexbefore != -1)
                        {
                            // Loop indexbefore + 1 times to include the 'exist' item itself
                            int itemsToRemove = indexbefore + 1;

                            for (int i = 0; i < itemsToRemove; i++)
                            {
                                if (QueueService.VusicQueueNext.Count > 0)
                                {
                                    QueueService.VusicQueueNext.RemoveAt(0);
                                }
                            }
                        }
                        //   QueueService.VusicQueueNext.Remove(exist);
                    }

                    ShowManager.totalepisodecount = EpisodesList.Count;
                    //   ShowManager.currentseason = currentseasonindex;

                    App.NavigationFrame.Navigate(typeof(VideoPlayer), episode);
                }
            }
        }
        bool iseditabout = true;
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            if (sender is Button btn)
            {
                if (iseditabout)
                {
                    ToolTipService.SetToolTip(btn, "Save");
                    iseditabout = false;
                    txtGenre.Visibility = Visibility.Collapsed;
                    txtReleaseDate.Visibility = Visibility.Collapsed;
                    dtPickerReleaseDate.Visibility = Visibility.Visible;
                    txtGenreEdit.Visibility = Visibility.Visible;
                    dtPickerReleaseDate.Date = currentshow.ReleaseDate;
                    txtGenreEdit.Text = txtGenre.Text;
                }
                else
                {
                    ToolTipService.SetToolTip(btn, "Edit");






                    currentshow.Genre = txtGenreEdit.Text;
                    var currentsettings = await SettingsLoader.LoadSettingsAsync();
                    var shows = currentsettings.Shows;
                    var exist = shows.FirstOrDefault(p => p.Name == currentshow.Name);
                    if (exist != null)
                    {
                        exist.Genre = txtGenre.Text = txtGenreEdit.Text;
                        exist.ReleaseDate = currentshow.ReleaseDate = dtPickerReleaseDate.SelectedDate ?? DateTime.Now;
                        txtReleaseDate.Text = dtPickerReleaseDate.SelectedDate?.ToString("dd MMMM yyyy") ?? "07 October 2008";
                    }
                    await SettingsLoader.SaveSettingsAsync(currentsettings);
                    iseditabout = true;
                    txtGenre.Visibility = Visibility.Visible;
                    txtReleaseDate.Visibility = Visibility.Visible;
                    dtPickerReleaseDate.Visibility = Visibility.Collapsed;
                    txtGenreEdit.Visibility = Visibility.Collapsed;

                }
            }
        }

        private async void mnftEditPoster_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            if (currentshow == null) return;
            var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.MainWindowInstance, "Choose poster");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            var exist = shows.FirstOrDefault(p => p.Name == currentshow.Name);
            if (image != null)
            {
                imgPoster.Source = new BitmapImage(new Uri(image.Path));
                if (exist != null)
                {
                    exist.Poster = image.Path;
                }
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void mnftEditShowName_Click(object sender, RoutedEventArgs e)
        {
            ttRename.IsOpen = true;
            txtRenameShow.Text = txtShowName.Text;
        }

        private void btnEditCreators_Click(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            crewNames.Clear();
            ttEditCrew.Target = btnEditCreators;
            ttEditCrew.Title = "Edit Creators";
            ttEditCrew.IsOpen = true;
            var showcrew = currentshow.Creators.Split(",");
            foreach (var name in showcrew)
            {
                crewNames.Add(new CrewName { Name = name });
            }
            lstViewNamesCrew.ItemsSource = crewNames;
        }

        private void btnEditCrew_Click(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            crewNames.Clear();
            ttEditCrew.Target = btnEditCrew;
            ttEditCrew.Title = "Edit Crew";
            ttEditCrew.IsOpen = true;
            var showcrew = currentshow.Crew.Split(",");
            foreach (var name in showcrew)
            {
                crewNames.Add(new CrewName { Name = name });
            }
            lstViewNamesCrew.ItemsSource = crewNames;
        }

        private void btnAddToListName_Click(object sender, RoutedEventArgs e)
        {
            if (txtAddCrewMember.Text == "") return;
            var exist = crewNames.FirstOrDefault(p => p.Name.ToLower() == txtAddCrewMember.Text.ToLower());
            if (exist == null)
            {
                crewNames.Insert(0, new CrewName { Name = txtAddCrewMember.Text });
            }
        }

        private void btnRemoveName_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = lstViewNamesCrew.SelectedItems.Cast<CrewName>().ToList();
            foreach (var item in selectedItems)
            {
                crewNames.Remove(item);
            }
        }
        ObservableCollection<CrewName> crewNames = new();

        private void MenuFlyoutItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is CrewName crewName)
            {
                crewNames.Remove(crewName);
            }
        }

        private void lstViewNamesCrew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewNamesCrew.SelectedItems.Count == 0)
            {
                btnRemoveName.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnRemoveName.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ttEditCrew.IsOpen = false;
        }


        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            var exist = shows.FirstOrDefault(p => p.Name == currentshow.Name);
            if (exist == null) return;

            // Fix: Efficiently join names with commas, automatically avoiding a trailing comma
            string crew = string.Join(",", crewNames.Select(item => item.Name));

            if (ttEditCrew.Title == "Edit Crew")
            {
                currentshow.Crew = crew;

                exist.Crew = crew;
                LoadCrew(new Show { Crew = crew });
            }
            else
            {
                currentshow.Creators = crew;

                exist.Creators = crew;
                LoadCreators(new Show { Creators = crew });
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        bool isAtoZ = true;
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (isAtoZ == true)
            {
                isAtoZ = false;
                // Sort A to Z
                var sorted = crewNames.OrderBy(p => p.Name).ToList();

                // Rebuild the ObservableCollection safely
                crewNames.Clear();
                foreach (var item in sorted)
                {
                    crewNames.Add(item);
                }
            }
            else
            {
                isAtoZ = true;
                // Sort Z to A
                var sorted = crewNames.OrderByDescending(p => p.Name).ToList();

                crewNames.Clear();
                foreach (var item in sorted)
                {
                    crewNames.Add(item);
                }
            }
        }

        private void mnftOpenSeason_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnOfficialRename_Click(object sender, RoutedEventArgs e)
        {
            if (currentshow == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            var exist = shows.FirstOrDefault(p => p.Name == currentshow.Name);
            if (exist != null)
            {
                string baseName = txtRenameShow.Text.Trim();

                if (string.IsNullOrEmpty(baseName)) baseName = "Show";

                string finalName = baseName;
                int counter = 1;
                while (currentSettings.Shows.Any(p =>
                    string.Equals(p.Name, finalName, StringComparison.OrdinalIgnoreCase)))
                {
                    finalName = $"{baseName} ({counter++})";
                }
                exist.Name = finalName;
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            txtShowName.Text = txtRenameShow.Text;
            ttRename.IsOpen = false;
        }

        private async void mnftUnlinkSeason_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem season)
            {
                if (currentshow == null) return;
                if (season.PlaylistId == null) return;
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var shows = currentSettings.Shows;
                var exist = shows.FirstOrDefault(p => p.Name == currentshow.Name);
                if (exist != null)
                {
                    var existunlink = exist.UnlinkedSeasons.FirstOrDefault(p => p == season.PlaylistId);
                    if(existunlink == null)
                    {
                        exist.UnlinkedSeasons.Add(season.PlaylistId);

                        seasons.Remove(season); 
                        var seasonsRearranged = seasons.OrderBy(p => p.SeasonNumber).ToList();
                        for (int i = 0; i < seasonsRearranged.Count; i++)
                        {
                            if (seasonsRearranged[i] != null)
                            {
                                seasonsRearranged[i].SeasonIndex = i;
                            }
                        }
                        grdViewSeasons.ItemsSource = null;
                        grdViewSeasons.ItemsSource = seasonsRearranged;
                        grdViewSeasons.Visibility = Visibility.Visible;
                        txtSeasonCount.Text = $"• {seasons.Count} {(seasons.Count == 1 ? "season" : "seasons")}";
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private void hypViewUnlinkedSeasons_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyout_Opened(object sender, object e)
        {

        }
    }

}
