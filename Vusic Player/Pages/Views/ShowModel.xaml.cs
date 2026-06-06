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
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
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
                    txtSeasonCount.Text = $"• {seasons.Count} {(seasons.Count == 1 ? "season" : "seasons")}";
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var shows = currentSettings.Shows;
                    var exist = shows.FirstOrDefault(p => p.Name == show.Name);
                    if(exist != null)
                    {
                        exist.SeasonCount = seasons.Count;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    if (seasons.Count != 0)
                    {
                        var seasonsRearranged = seasons.OrderBy(p => p.SeasonNumber).ToList();
                        grdViewSeasons.ItemsSource = null;
                        grdViewSeasons.ItemsSource = seasonsRearranged;
                        grdViewSeasons.Visibility = Visibility.Visible;
                    }
                }
            }
            base.OnNavigatedTo(e);
        }

        bool isEditMode = false;
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (isEditMode)
            {
                txtDescription.Text = txtEditableDescription.Text;

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

        private void btnAddSeason_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ppArtist_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

        }

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
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(VideoPlayer), EpisodesList[0]);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is EpisodeModel episode)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(VideoPlayer), episode);
                }
            }
        }
    }

}
