using Flyleaf.FFmpeg;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Vusic_Player.UI.UserViews.Controls.ListViewMedia;
using FileInfo = Vusic_Player.Configuration.Helper.FileInfo;


namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class GroupedCollectionView : UserControl
    {
        public enum TimelineTemplateMode
        {
            Media,
            Playlists,
            Artist,
            Album,
            Genre
        }
        public GroupedCollectionView()
        {
           
            InitializeComponent();
     //       FileInfo.RefreshValues += FileInfo_RefreshValues;
        }

        private void FileInfo_RefreshValues()
        {
            throw new NotImplementedException();
        }

        public static readonly DependencyProperty TemplateModeProperty =
               DependencyProperty.Register(
                   nameof(TemplateMode),
                   typeof(TimelineTemplateMode),
                   typeof(GroupedCollectionView),
                   new PropertyMetadata(TimelineTemplateMode.Media, OnTemplateModeChanged));

        public TimelineTemplateMode TemplateMode
        {
            get => (TimelineTemplateMode)GetValue(TemplateModeProperty);
            set => SetValue(TemplateModeProperty, value);
        }
        private static void OnTemplateModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("UPDATE3");

            if (d is GroupedCollectionView control)
            {
                Debug.WriteLine("UPDATE4");

                control.UpdateListViewTemplate();
            }
        }

        // Switch the template based on the enum selection
        private void UpdateListViewTemplate()
        {
            Debug.WriteLine("UPDATE1");
            string resourceKey = TemplateMode switch
            {
                TimelineTemplateMode.Media => "TimelineSongTemplate",
                TimelineTemplateMode.Album => "AlbumTimelineTemplate",
                TimelineTemplateMode.Artist => "ArtistTimelineTemplate",
                TimelineTemplateMode.Playlists => "PlaylistTimelineTemplate",
                TimelineTemplateMode.Genre => "GenreTimelineTemplate",
                _ => "TimelineSongTemplate" // Fallback default
            };

            if (this.Resources.TryGetValue(resourceKey, out object templateObj)
                && templateObj is DataTemplate template)
            {
                Debug.WriteLine("UPDATE2");

                MainTimelineList.ItemTemplate = template;
            }
        }

        // Ensure the default template loads correctly on startup
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateListViewTemplate();
        }
        public DataTemplate ItemContentTemplate
        {
            get { return (DataTemplate)GetValue(ItemContentTemplateProperty); }
            set { SetValue(ItemContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemContentTemplateProperty =
            DependencyProperty.Register("ItemContentTemplate", typeof(DataTemplate),
            typeof(GroupedCollectionView), new PropertyMetadata(null));

        public static readonly DependencyProperty SearchPlaceholder =
              DependencyProperty.Register("Value", typeof(string), typeof(SearchControl), new PropertyMetadata("Search for titles, artists, albums...."));

        public string SearchPlaceHolderText
        {
            get => (string)GetValue(SearchPlaceholder);
            set => SetValue(SearchPlaceholder, value);
        }
        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object),
            typeof(GroupedCollectionView), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GroupedCollectionView control)
            {
                // 1. Unsubscribe from the old collection to prevent memory leaks
                if (e.OldValue is System.Collections.Specialized.INotifyCollectionChanged oldList)
                {
                    oldList.CollectionChanged -= control.OnCollectionChanged;
                }

                if (e.NewValue is IEnumerable<SongModel> list)
                {
                    control.GenerateTimeline(list);

                    // 2. Subscribe to the new collection's changes
                    if (e.NewValue is System.Collections.Specialized.INotifyCollectionChanged newList)
                    {
                        newList.CollectionChanged += control.OnCollectionChanged;
                    }
                }
                else if (e.NewValue is IEnumerable<PlaylistItem> list2)
                {
                    Debug.WriteLine("Playlst4");

                    control.GenerateTimelinePlaylist(list2);
                    Debug.WriteLine("Playlst7");

                    // 2. Subscribe to the new collection's changes
                    if (e.NewValue is System.Collections.Specialized.INotifyCollectionChanged newList)
                    {
                        Debug.WriteLine("Playlst8");

                        newList.CollectionChanged += control.OnCollectionChanged;
                    }
                }
                else if (e.NewValue is IEnumerable<ArtistShow> list3)
                {
                    Debug.WriteLine("Playlst4");

                    control.GenerateTimelineArtist(list3);
                    Debug.WriteLine("Playlst7");

                    // 2. Subscribe to the new collection's changes
                    if (e.NewValue is System.Collections.Specialized.INotifyCollectionChanged newList)
                    {
                        Debug.WriteLine("Playlst8");

                        newList.CollectionChanged += control.OnCollectionChanged;
                    }
                }
                else if (e.NewValue is IEnumerable<ArtistDiscAlbumModel> list4)
                {
                    Debug.WriteLine("Playlst4");

                    control.GenerateTimelineAlbum(list4);
                    Debug.WriteLine("Playlst7");

                    // 2. Subscribe to the new collection's changes
                    if (e.NewValue is System.Collections.Specialized.INotifyCollectionChanged newList)
                    {
                        Debug.WriteLine("Playlst8");

                        newList.CollectionChanged += control.OnCollectionChanged;
                    }
                }

            }
        }
        private void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource is IEnumerable<SongModel> list)
            {
                MainTimelineList.ItemsSource = TimelineCollection;
                GenerateTimeline(list);
            }
            else if (ItemsSource is IEnumerable<PlaylistItem> list2)
            {
                MainTimelineList.ItemsSource = TimelineCollectionPlaylist;
                GenerateTimelinePlaylist(list2);
            }
            else if (ItemsSource is IEnumerable<ArtistShow> list3)
            {
                MainTimelineList.ItemsSource = TimelineCollectionArtist;
                GenerateTimelineArtist(list3);
            }
            else if (ItemsSource is IEnumerable<ArtistDiscAlbumModel> list4)
            {
                MainTimelineList.ItemsSource = TimelineCollectionAlbum;
                GenerateTimelineAlbum(list4);
            }
        }
        private void btnGlyph_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyp && hyp.DataContext is GroupedCollectionModel group && group.Data is SongModel song)
            {

                if (song.Glyph == "\uE768")
                {
                    PlayerService.Play();
                }
                else if (song.Glyph == "\uE769")
                {
                    PlayerService.Pause();
                }

            }
        }
        private async void btnFavourite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is Grid rootGrid && btn.DataContext is GroupedCollectionModel group && group.Data is SongModel song)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                var pathtocheck = song.FilePath;
                if (pathtocheck == null) return;
                var fillHeartIcon = rootGrid.FindName("HeartIcon") as FontIcon;
                if (fillHeartIcon == null) return;
                var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
                if (existing == null)
                {
                    fillHeartIcon.Glyph = "\uEB52";
                    song.IsFavourite = true;
                    AnimateHeartFull(fillHeartIcon, true);
                    Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    song.FavString = "Remove from Favourites";

                }
                else
                {
                    song.IsFavourite = false;
                    fillHeartIcon.Glyph = "\uEB51";
                    AnimateHeartFull(fillHeartIcon, false);
                    Favourites.Remove(existing);
                    song.FavString = "Add to Favourites";


                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);

            }

        }
        private void AnimateHeartFull(FontIcon targetIcon, bool isBecomingFavorite)
        {
            // 1. Get the Visual and Compositor
            var visual = ElementCompositionPreview.GetElementVisual(targetIcon);
            var compositor = visual.Compositor;

            // 2. Setup Scale Animation (The "Pop")
            var scaleAnim = compositor.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, 1.0f);
            scaleAnim.InsertKeyFrame(0.5f, 1.4f); // Pulse size
            scaleAnim.InsertKeyFrame(1.0f, 1.0f);
            scaleAnim.Duration = TimeSpan.FromMilliseconds(400);

            visual.CenterPoint = new Vector3((float)targetIcon.ActualWidth / 2, (float)targetIcon.ActualHeight / 2, 0);
            visual.StartAnimation("Scale.X", scaleAnim);
            visual.StartAnimation("Scale.Y", scaleAnim);

            // 3. Setup Color Animation (The "Fill")
            // Note: We animate the 'Brush.Color' of the visual
            var colorAnim = compositor.CreateColorKeyFrameAnimation();
            colorAnim.Duration = TimeSpan.FromMilliseconds(400);

            if (isBecomingFavorite)
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Gray);
                colorAnim.InsertKeyFrame(1.0f, Colors.Red);
            }
            else
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Red);
                colorAnim.InsertKeyFrame(1.0f, Colors.Gray);
            }

            // Create a Brush if one doesn't exist on the visual layer
            var brush = compositor.CreateColorBrush();
            visual.Properties.InsertColor("Color", isBecomingFavorite ? Colors.Red : Colors.Gray);

            // Start the color transition
            // Note: For FontIcon, it's often easier to just swap the Glyph 
            // and let the Composition color animation handle the tint.
            targetIcon.Foreground = new SolidColorBrush(isBecomingFavorite ? Colors.Red : Colors.Gray);
        }

        private async void PlaySelection(SongModel selectedSong)
        {
            if (selectedSong.FilePath != null)
            {
                if (File.Exists(selectedSong.FilePath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(selectedSong.FilePath);
                    string fileExtension = file.FileType.ToLowerInvariant();
                    bool isVideo = false;
                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {
                        isVideo = true;
                    }
                    if (isVideo == false)
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                        single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                        QueueService.PlayMedia(single, false, false);
                    }
                    else
                    {
                        //if (App.UltimateFrame != null)
                        //{
                        //    if (App.NavigationFrame == null) return;
                        //    NavigationManager.LastContentPageType = App.NavigationFrame.CurrentSourcePageType;
                        //    App.UltimateFrame.Navigate(typeof(VideoPlay), selectedSong.FilePath);
                        //}

                    }
                }
            }
        }

        private void hypTitle_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SONG1");
            if (sender is HyperlinkButton hyperlin)
            {
                Debug.WriteLine(hyperlin.DataContext.ToString());
            }
            if (sender is HyperlinkButton hyperlink && hyperlink.DataContext is GroupedCollectionModel song && song.Data is SongModel songmodel)
            {
                Debug.WriteLine("SONG");
                PlaySelection(songmodel);
            }
        }

        private void hypArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyperlink && hyperlink.DataContext is GroupedCollectionModel song && song.Data is SongModel songmodel)
            {

                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(ArtistView), songmodel.Artist);
            }

        }

        private void hypAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyperlink && hyperlink.DataContext is GroupedCollectionModel song && song.Data is SongModel songmodel && songmodel.AlbumName is string str)
            {
                if (App.NavigationFrame == null) return;
                var myApp = (App)Application.Current;
                if (myApp.SelectedAlbum == null)
                {
                    myApp.SelectedAlbum = new AlbumContext { Name = str };
                }
                App.NavigationFrame.Navigate(typeof(AlbumView), myApp.SelectedAlbum);
            }
        }
        private void GenerateTimelinePlaylist(IEnumerable<PlaylistItem> items)
        {
            Debug.WriteLine("Playlst5");


            TimelineCollectionPlaylist.Clear();

            // 1. Helper to get the string value dynamically
            Func<object, string> getStringValue = (obj) =>
            {
                if (string.IsNullOrEmpty(DisplayMemberPath)) return obj.ToString() ?? "";
                var prop = obj.GetType().GetProperty(DisplayMemberPath);
                return prop?.GetValue(obj)?.ToString() ?? obj.ToString() ?? "";
            };

            // 2. Sort by the dynamic value
            var sorted = items.OrderBy(x => getStringValue(x)).ToList();
            string lastLetter = "";

            foreach (var item in sorted)
            {
                string currentTitle = getStringValue(item);
                string firstLetter = string.IsNullOrEmpty(currentTitle) ? "#" :
                                     char.IsDigit(currentTitle[0]) ? "#" :
                                     currentTitle[0].ToString().ToUpper();

                bool isStart = firstLetter != lastLetter;
                if (isStart) lastLetter = firstLetter;
                Debug.WriteLine(item.PlaylistName);

                TimelineCollectionPlaylist.Add(new GroupedCollectionModelPlaylist
                {

                    Data = item,
                    Letter = firstLetter,
                    IsGroupStart = isStart
                });
            }

            MapGridView.ItemsSource = TimelineCollectionPlaylist
                .Where(x => x.IsGroupStart)
                .Select(x => x.Letter)
                .ToList();
            Debug.WriteLine("Playlst6");

        }
        private void GenerateTimelineArtist(IEnumerable<ArtistShow> items)
        {
            Debug.WriteLine("Playlst5");


            TimelineCollectionArtist.Clear();

            // 1. Helper to get the string value dynamically
            Func<object, string> getStringValue = (obj) =>
            {
                if (string.IsNullOrEmpty(DisplayMemberPath)) return obj.ToString() ?? "";
                var prop = obj.GetType().GetProperty(DisplayMemberPath);
                return prop?.GetValue(obj)?.ToString() ?? obj.ToString() ?? "";
            };

            // 2. Sort by the dynamic value
            var sorted = items.OrderBy(x => getStringValue(x)).ToList();
            string lastLetter = "";

            foreach (var item in sorted)
            {
                string currentTitle = getStringValue(item);
                string firstLetter = string.IsNullOrEmpty(currentTitle) ? "#" :
                                     char.IsDigit(currentTitle[0]) ? "#" :
                                     currentTitle[0].ToString().ToUpper();

                bool isStart = firstLetter != lastLetter;
                if (isStart) lastLetter = firstLetter;

                TimelineCollectionArtist.Add(new GroupedCollectionModelArtist
                {

                    Data = item,
                    Letter = firstLetter,
                    IsGroupStart = isStart
                });
            }

            MapGridView.ItemsSource = TimelineCollectionArtist
                .Where(x => x.IsGroupStart)
                .Select(x => x.Letter)
                .ToList();

        }
        private void GenerateTimelineAlbum(IEnumerable<ArtistDiscAlbumModel> items)
        {
            Debug.WriteLine("Playlst5");


            TimelineCollectionAlbum.Clear();

            // 1. Helper to get the string value dynamically
            Func<object, string> getStringValue = (obj) =>
            {
                if (string.IsNullOrEmpty(DisplayMemberPath)) return obj.ToString() ?? "";
                var prop = obj.GetType().GetProperty(DisplayMemberPath);
                return prop?.GetValue(obj)?.ToString() ?? obj.ToString() ?? "";
            };

            // 2. Sort by the dynamic value
            var sorted = items.OrderBy(x => getStringValue(x)).ToList();
            string lastLetter = "";

            foreach (var item in sorted)
            {
                string currentTitle = getStringValue(item);
                string firstLetter = string.IsNullOrEmpty(currentTitle) ? "#" :
                                     char.IsDigit(currentTitle[0]) ? "#" :
                                     currentTitle[0].ToString().ToUpper();

                bool isStart = firstLetter != lastLetter;
                if (isStart) lastLetter = firstLetter;

                TimelineCollectionAlbum.Add(new GroupedCollectionModelAlbum
                {

                    Data = item,
                    Letter = firstLetter,
                    IsGroupStart = isStart
                });
            }

            MapGridView.ItemsSource = TimelineCollectionAlbum
                .Where(x => x.IsGroupStart)
                .Select(x => x.Letter)
                .ToList();

        }


        private void GenerateTimeline(IEnumerable<SongModel> items)
        {
            TimelineCollection.Clear();

            // 1. Helper to get the string value dynamically
            Func<object, string> getStringValue = (obj) =>
            {
                if (string.IsNullOrEmpty(DisplayMemberPath)) return obj.ToString() ?? "";
                var prop = obj.GetType().GetProperty(DisplayMemberPath);
                return prop?.GetValue(obj)?.ToString() ?? obj.ToString() ?? "";
            };

            // 2. Sort by the dynamic value
            var sorted = items.OrderBy(x => getStringValue(x)).ToList();
            string lastLetter = "";

            foreach (var item in sorted)
            {
                string currentTitle = getStringValue(item);
                string firstLetter = string.IsNullOrEmpty(currentTitle) ? "#" :
                                     char.IsDigit(currentTitle[0]) ? "#" :
                                     currentTitle[0].ToString().ToUpper();

                bool isStart = firstLetter != lastLetter;
                if (isStart) lastLetter = firstLetter;

                TimelineCollection.Add(new GroupedCollectionModel
                {
                    Data = item,
                    Letter = firstLetter,
                    IsGroupStart = isStart
                });
            }

            MapGridView.ItemsSource = TimelineCollection
                .Where(x => x.IsGroupStart)
                .Select(x => x.Letter)
                .ToList();
        }
        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string),
            typeof(GroupedCollectionView), new PropertyMetadata(null));

        public ObservableCollection<GroupedCollectionModel> TimelineCollection { get; set; }
                = new ObservableCollection<GroupedCollectionModel>();
        public ObservableCollection<GroupedCollectionModelPlaylist> TimelineCollectionPlaylist { get; set; }
                = new ObservableCollection<GroupedCollectionModelPlaylist>();
        public ObservableCollection<GroupedCollectionModelArtist> TimelineCollectionArtist { get; set; }
        = new ObservableCollection<GroupedCollectionModelArtist>();
        public ObservableCollection<GroupedCollectionModelAlbum> TimelineCollectionAlbum { get; set; }
    = new ObservableCollection<GroupedCollectionModelAlbum>();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void grdViewMap_ItemClick(object sender, ItemClickEventArgs e)
        {


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MapTip.Target = sender as Button;
            MapTip.IsOpen = true;
        }

        private void MapGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var targetLetter = e.ClickedItem.ToString();
            // LINQ search: Find the first item where the letter matches AND it's a group start
            var target = TimelineCollection.FirstOrDefault(x => x.IsGroupStart && x.Letter == targetLetter);

            if (target != null)
            {
                MainTimelineList.ScrollIntoView(target, ScrollIntoViewAlignment.Leading);
                MapTip.IsOpen = false;
            }
        }

        private void ExternalContentPresenter_Loading(FrameworkElement sender, object args)
        {
            if (sender is ContentPresenter presenter)
            {
                // Set the template directly from the UserControl's property
                presenter.ContentTemplate = this.ItemContentTemplate;
            }
        }
        ObservableCollection<GroupedCollectionModel> searchresults = new();
        ObservableCollection<GroupedCollectionModelPlaylist> searchresultsplaylists = new();
        ObservableCollection<GroupedCollectionModelArtist> searchresultsartists = new();
        ObservableCollection<GroupedCollectionModelAlbum> searchresultsalbums = new();

        private IEnumerable<GroupedCollectionModel> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<GroupedCollectionModel>();

            var rawQuery = query.Trim();

            var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
            var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

            int searchSeconds = 0;
            if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
            if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);

            var textQuery = rawQuery;
            if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
            if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
            textQuery = textQuery.Trim();

            return TimelineCollection.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.Data.Title?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.Artist?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.AlbumName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.Year.ToString().Contains(textQuery))
                );

                bool durationMatch = (searchSeconds > 0 && s.Data.SongDuration.HasValue &&
                                     Math.Abs(s.Data.SongDuration.Value.TotalSeconds - searchSeconds) < 2);

                return textMatch || durationMatch;
            })
            .OrderByDescending(s => s.Data.Title?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Data.Title);
        }
        private IEnumerable<GroupedCollectionModelPlaylist> GetFilteredResultsPlaylist(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<GroupedCollectionModelPlaylist>();

            var rawQuery = query.Trim();

            var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
            var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

            int searchSeconds = 0;
            if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
            if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);

            var textQuery = rawQuery;
            if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
            if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
            textQuery = textQuery.Trim();

            return TimelineCollectionPlaylist.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.Data.PlaylistName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.PlaylistCount?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)

                );



                return textMatch;
            })
            .OrderByDescending(s => s.Data.PlaylistName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Data.PlaylistName);
        }
        private IEnumerable<GroupedCollectionModelArtist> GetFilteredResultsArtist(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<GroupedCollectionModelArtist>();

            var rawQuery = query.Trim();
            var textQuery = rawQuery;
            textQuery = textQuery.Trim();

            return TimelineCollectionArtist.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.Data.ArtistName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.ArtistSongCount?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)

                );
                return textMatch;
            })
            .OrderByDescending(s => s.Data.ArtistName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Data.ArtistName);
        }
        private IEnumerable<GroupedCollectionModelAlbum> GetFilteredResultsAlbum(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<GroupedCollectionModelAlbum>();

            var rawQuery = query.Trim();
            var textQuery = rawQuery;
            textQuery = textQuery.Trim();

            return TimelineCollectionAlbum.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.Data.AlbumName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.AlbumArtists?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.AlbumYear?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Data.AlbumCount?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)

                );
                return textMatch;
            })
            .OrderByDescending(s => s.Data.AlbumName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Data.AlbumName);
        }


        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbSearch.Text = "";
            MainTimelineList.Focus(FocusState.Programmatic);
            asbSearch.ItemsSource = null;
        }
        private void asbSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                searchresults.Clear();
                searchresultsplaylists.Clear();
                searchresultsartists.Clear();
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                if (TemplateMode == TimelineTemplateMode.Playlists)
                {
                    MainTimelineList.ItemsSource = TimelineCollectionPlaylist;
                }
                else if (TemplateMode == TimelineTemplateMode.Artist)
                {
                    MainTimelineList.ItemsSource = TimelineCollectionArtist;
                }
                else if (TemplateMode == TimelineTemplateMode.Album)
                {
                    MainTimelineList.ItemsSource = TimelineCollectionAlbum;
                }
                else
                {
                    MainTimelineList.ItemsSource = TimelineCollection;
                }
                MainTimelineList.Visibility = Visibility.Visible;

                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (TemplateMode == TimelineTemplateMode.Media)
                {
                    var results = GetFilteredResults(sender.Text);
                    searchresults.Clear();
                    foreach (var item in results) searchresults.Add(item);

                    sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };

                    MainTimelineList.ItemsSource = searchresults;
                }
                else if (TemplateMode == TimelineTemplateMode.Playlists)
                {
                    var results = GetFilteredResultsPlaylist(sender.Text);
                    searchresultsplaylists.Clear();
                    foreach (var item in results) searchresultsplaylists.Add(item);

                    sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };

                    MainTimelineList.ItemsSource = searchresultsplaylists;
                }
                else if (TemplateMode == TimelineTemplateMode.Artist)
                {
                    var results = GetFilteredResultsArtist(sender.Text);
                    searchresultsartists.Clear();
                    foreach (var item in results) searchresultsartists.Add(item);

                    sender.ItemsSource = results.Any() ? null : new List<string> { "No artists found!" };

                    MainTimelineList.ItemsSource = searchresultsartists;
                }
                else if (TemplateMode == TimelineTemplateMode.Album)
                {
                    var results = GetFilteredResultsAlbum(sender.Text);
                    searchresultsalbums.Clear();
                    foreach (var item in results) searchresultsalbums.Add(item);

                    sender.ItemsSource = results.Any() ? null : new List<string> { "No albums found!" };

                    MainTimelineList.ItemsSource = searchresultsalbums;
                }

            }
        }



        private void asbSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (TemplateMode == TimelineTemplateMode.Media)
            {
                var results = GetFilteredResults(sender.Text);

                if (results.Any())
                {
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    MainTimelineList.Visibility = Visibility.Visible;
                    //       Grid.SetRow(grdNoSearchResults, 2);

                    searchresults.Clear();
                    foreach (var item in results) searchresults.Add(item);
                }
                else if (TimelineCollection.Count > 0)
                {

                    MainTimelineList.Visibility = Visibility.Collapsed;

                    grdNoSearchResults.Visibility = Visibility.Visible;
                    frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                }
            }
            else if (TemplateMode == TimelineTemplateMode.Playlists)
            {
                var results = GetFilteredResultsPlaylist(sender.Text);

                if (results.Any())
                {
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    MainTimelineList.Visibility = Visibility.Visible;
                    //       Grid.SetRow(grdNoSearchResults, 2);

                    searchresultsplaylists.Clear();
                    foreach (var item in results) searchresultsplaylists.Add(item);
                }
                else if (TimelineCollectionPlaylist.Count > 0)
                {

                    MainTimelineList.Visibility = Visibility.Collapsed;

                    grdNoSearchResults.Visibility = Visibility.Visible;
                    frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                }
            }
            else if (TemplateMode == TimelineTemplateMode.Artist)
            {
                var results = GetFilteredResultsArtist(sender.Text);

                if (results.Any())
                {
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    MainTimelineList.Visibility = Visibility.Visible;
                    //       Grid.SetRow(grdNoSearchResults, 2);

                    searchresultsartists.Clear();
                    foreach (var item in results) searchresultsartists.Add(item);
                }
                else if (TimelineCollectionArtist.Count > 0)
                {

                    MainTimelineList.Visibility = Visibility.Collapsed;

                    grdNoSearchResults.Visibility = Visibility.Visible;
                    frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                }
            }
            else if (TemplateMode == TimelineTemplateMode.Album)
            {
                var results = GetFilteredResultsAlbum(sender.Text);

                if (results.Any())
                {
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    MainTimelineList.Visibility = Visibility.Visible;
                    //       Grid.SetRow(grdNoSearchResults, 2);

                    searchresultsalbums.Clear();
                    foreach (var item in results) searchresultsalbums.Add(item);
                }
                else if (TimelineCollectionAlbum.Count > 0)
                {

                    MainTimelineList.Visibility = Visibility.Collapsed;

                    grdNoSearchResults.Visibility = Visibility.Visible;
                    frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                }
            }

        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton mnft && mnft.DataContext is GroupedCollectionModelArtist arist)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(ArtistView), arist.Data.ArtistName);
            }
        }
        private async void PlaySelectionAlbum(IEnumerable<GroupedCollectionModelAlbum> albumsongs, bool loop = false, bool shuffle = false)
        {
            ObservableCollection<SongModel> tempTransfer = new();

            foreach (var item in albumsongs)
            {
                var songs = item.Data.Songs;
                foreach (var song in songs)
                {
                    var file = await StorageFile.GetFileFromPathAsync(song.FilePath);
                    var props = await file.Properties.GetMusicPropertiesAsync();
                    string Title = props.Title;
                    if (Title == "")
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Path);
                    }
                    string AlbumName = string.IsNullOrWhiteSpace(props.Album) ? "Unknown Album" : props.Album;
                    string Artist = string.IsNullOrWhiteSpace(props.Artist) ? "Unknown Artist" : props.Artist;

                    tempTransfer.Add(new SongModel
                    {
                        Title = Title,
                        AlbumName = AlbumName,
                        Artist = Artist,
                        SongDuration = props.Duration,
                        FilePath = file.Path
                    });
                }
            }
            QueueService.PlayMedia(tempTransfer, shuffle, loop);

        }

        private async void PlaySelectionArtist(IEnumerable<GroupedCollectionModelArtist> artistsongs, bool loop = false, bool shuffle = false)
        {
            ObservableCollection<SongModel> tempTransfer = new();

            foreach (var item in artistsongs)
            {
                var songs = item.Data.Songs;
                foreach (var song in songs)
                {
                    var file = await StorageFile.GetFileFromPathAsync(song.FilePath);
                    var props = await file.Properties.GetMusicPropertiesAsync();
                    string Title = props.Title;
                    if (Title == "")
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Path);
                    }
                    string AlbumName = string.IsNullOrWhiteSpace(props.Album) ? "Unknown Album" : props.Album;
                    string Artist = string.IsNullOrWhiteSpace(props.Artist) ? "Unknown Artist" : props.Artist;

                    tempTransfer.Add(new SongModel
                    {
                        Title = Title,
                        AlbumName = AlbumName,
                        Artist = Artist,
                        SongDuration = props.Duration,
                        FilePath = file.Path
                    });
                }
            }
            QueueService.PlayMedia(tempTransfer, shuffle, loop);

        }
        private async void btnPlayAllArtistSongs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GroupedCollectionModelArtist artist)
            {
                var artistsongs = TimelineCollectionArtist.Where(p => p.Data.ArtistName == artist.Data.ArtistName);
                PlaySelectionArtist(artistsongs);
            }
        }

        private void mnftViewArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModelArtist arist)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(ArtistView), arist.Data.ArtistName);
            }
        }

        private async void mnftPlayArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelArtist artist)
            {
                var artistsongs = TimelineCollectionArtist.Where(p => p.Data.ArtistName == artist.Data.ArtistName);
                PlaySelectionArtist(artistsongs);
            }

        }

        private async void mnftPlayArtistShuffled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelArtist artist)
            {
                var artistsongs = TimelineCollectionArtist.Where(p => p.Data.ArtistName == artist.Data.ArtistName);
                PlaySelectionArtist(artistsongs, false, true);
            }
        }

        private async void mnftPlayArtistLoop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelArtist artist)
            {
                var artistsongs = TimelineCollectionArtist.Where(p => p.Data.ArtistName == artist.Data.ArtistName);
                PlaySelectionArtist(artistsongs, true, false);
            }
        }

        private void mnftUnlinkArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelArtist artist)
            {
                Debug.WriteLine(artist.Data.ArtistName + " unlink");
                var artistsongs = TimelineCollectionArtist
                            .Where(p => p.Data.ArtistName == artist.Data.ArtistName)
                            .ToList();
                foreach (var artistname in artistsongs)
                {
                    TimelineCollectionArtist.Remove(artistname);

                    var songs = artistname.Data.Songs;
                    foreach (var song in songs)
                    {
                        song.Artist = "";
                        AudioMetadata.ChangeArtistName(song.FilePath, "");
                    }
                }
            }
        }

        private void mnftViewAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModelAlbum album)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(AlbumView), album.Data.AlbumName);
            }
        }

        private void mnftPlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelAlbum album)
            {
                var albumsongs = TimelineCollectionAlbum.Where(p => p.Data.AlbumName == album.Data.AlbumName);
                PlaySelectionAlbum(albumsongs);
            }
        }

        private void mnftPlayAlbumShuffled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelAlbum album)
            {
                var albumsongs = TimelineCollectionAlbum.Where(p => p.Data.AlbumName == album.Data.AlbumName);
                PlaySelectionAlbum(albumsongs, false, true);
            }
        }

        private void mnftPlayAlbumLoop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelAlbum album)
            {
                var albumsongs = TimelineCollectionAlbum.Where(p => p.Data.AlbumName == album.Data.AlbumName);
                PlaySelectionAlbum(albumsongs, true, false);
            }
        }

        private void mnftUnlinkAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelAlbum album)
            {
                Debug.WriteLine(album.Data.AlbumName + " unlink");
                var albumsongs = TimelineCollectionAlbum
                            .Where(p => p.Data.AlbumName == album.Data.AlbumName)
                            .ToList();
                foreach (var albumname in albumsongs)
                {
                    TimelineCollectionAlbum.Remove(albumname);

                    var songs = albumname.Data.Songs;
                    foreach (var song in songs)
                    {
                        song.AlbumName = "";
                        AudioMetadata.ChangeAlbumName(song.FilePath, "");
                    }
                }
            }

        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton mnft && mnft.DataContext is GroupedCollectionModelAlbum album)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(AlbumView), album.Data.AlbumName);
            }
        }

        private void mnftPlayAlbumLoopAndShuffled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelAlbum album)
            {
                var albumsongs = TimelineCollectionAlbum.Where(p => p.Data.AlbumName == album.Data.AlbumName);
                PlaySelectionAlbum(albumsongs, true, true);
            }
        }

        private void mnftPlayArtistLoopandShuffle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem btn && btn.DataContext is GroupedCollectionModelArtist artist)
            {
                var artistsongs = TimelineCollectionArtist.Where(p => p.Data.ArtistName == artist.Data.ArtistName);
                PlaySelectionArtist(artistsongs, true, true);
            }
        }

        private async void mnftContext_Opened(object sender, object e)
        {

            var flyout = sender as MenuFlyout;
            if (flyout == null) return;




            var addToPlaylist = flyout?.Items
        .OfType<MenuFlyoutSubItem>()
        .FirstOrDefault(x => x.Text == "Add to Playlist");

            if (addToPlaylist == null)
                return;

            addToPlaylist.Items.Clear();
            var selectedsong = addToPlaylist?.DataContext as GroupedCollectionModel;
            if (selectedsong == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var Playlists = currentSettings.SavedPlaylists;
            foreach (var playliitem in Playlists)
            {
                MenuFlyoutItem playlistitem = new MenuFlyoutItem();
                playlistitem.Text = playliitem.PlaylistName;
                addToPlaylist?.Items.Add(playlistitem);
                playlistitem.Click += async (sender, e) =>
                {
                    var path = selectedsong?.Data.FilePath;

                    if (path != null)
                    {
                        if (playliitem.SongsPaths.Contains(path))
                        {
                            ttAddedToPlaylist.Title = $"{Path.GetFileNameWithoutExtension(path)} already exists in {playliitem.PlaylistName}";
                        }
                        else
                        {
                            playliitem.SongsPaths.Add(path);
                            int count = playliitem.SongsPaths.Count;
                            playliitem.PlaylistCount = $"{count} {(count == 1 ? "item" : "items")}";
                            await SettingsLoader.SaveSettingsAsync(currentSettings);
                            ttAddedToPlaylist.Title = $"{Path.GetFileNameWithoutExtension(path)} has been added to {playliitem.PlaylistName}";

                        }
                        hypPlaylistAdded.Content = playliitem.PlaylistName;
                        hypPlaylistAdded.Tag = playliitem;
                        ttAddedToPlaylist.IsOpen = true;
                        await Task.Delay(3000);
                        ttAddedToPlaylist.IsOpen = false;
                    }
                };

            }

            //Favourites 
            var mnftAddtoFav = flyout?.Items
    .OfType<MenuFlyoutItem>()
    .FirstOrDefault(x => x.Name == "mnftAddToFavourites");

            if (mnftAddtoFav == null) return;
            if (selectedsong.Data.IsFavourite == true)
            {
                mnftAddtoFav.Text = "Remove from Favourites";
            }
            else
            {

                mnftAddtoFav.Text = "Add to Favourites";
            }
        }

        private async void mnftPlaySong_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group)
            {
                if (File.Exists(group.Data.FilePath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(group.Data.FilePath);
                    string fileExtension = file.FileType.ToLowerInvariant();
                    bool isVideo = false;
                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {
                        isVideo = true;
                    }
                    if (isVideo == false)
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(group.Data.FilePath);
                        single.Add(new SongModel { FilePath = group.Data.FilePath, Title = Title });
                        QueueService.PlayMedia(single, false, false);
                    }
                }
            }
        }

        private void mnftPlaySongNext_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group)
            {
                var item = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == group.Data.FilePath);
                if (item != null && item.FilePath != null)
                {
                    QueueService.VusicQueueNext.Remove(item);
                    QueueService.VusicQueueNext.Insert(0, item);
                }
                else if (item == null)
                {
                    QueueService.VusicQueueNext.Insert(0, group.Data);
                }
            }
        }

        private void mnftAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group && group.Data is SongModel song)
            {
                QueueService.VusicQueue.Add(song);
                QueueService.VusicQueueNext.Add(song);
            }
        }

        private void mnftGoToArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group && group.Data is SongModel song)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(ArtistView), song.Artist);
            }
        }

        private void mnftGoToAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group && group.Data is SongModel song)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(AlbumView), song.AlbumName);
            }
        }

        private async void mnftSongDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group && group.Data is SongModel song && song.FilePath is string filepath)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    FileInfo.RefreshValues += async () =>
                    {
                        var storagefile = await StorageFile.GetFileFromPathAsync(filepath);
                        var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                        string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(filepath) : musicproperties.Title;
                        string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                        string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;
                        group.Data.Artist = Artist;
                        group.Data.Title = title;
                        group.Data.AlbumName = AlbumName;
                    };
                    wind.ShowFileInfo(filepath);
                }
            }
        }

        private void lstViewPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewPlaylists.SelectedItems.Count == 0)
            {
                btnAddFinalPlaylists.IsEnabled = false;
            }
            else
            {
                btnAddFinalPlaylists.IsEnabled = true;
            }

        }

        private async void btnAddFinalPlaylists_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var SelectedItems = MainTimelineList.SelectedItems;
                var selected = SelectedItems.Cast<SongModel>().ToList();
                var selectedplaylists = lstViewPlaylists.SelectedItems.ToList();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playlists = currentSettings.SavedPlaylists;
                foreach (var item in selected)
                {
                    if (item != null && item.FilePath is string str)
                    {
                        if (File.Exists(str))
                        {
                            foreach (var playlist in selectedplaylists)
                            {
                                var exist = playlists.FirstOrDefault(p => p.PlaylistName == playlist.ToString());
                                if (exist != null)
                                {
                                    var songspaths = exist.SongsPaths;
                                    var defaultitem = songspaths.FirstOrDefault(k => k == item.FilePath);
                                    if (defaultitem == null)
                                    {
                                        exist.SongsPaths.Add(item.FilePath);
                                        exist.PlaylistCount = $"{exist.SongsPaths.Count} {(exist.SongsPaths.Count == 1 ? "item" : "items")}";
                                    }
                                }
                            }
                        }
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ex.Message, Logger.LogLevelType.Error);
                ttError.IsOpen = true;
                txtError.Text = "An unexpected error occured. Please check log page for details";
            }
            finally
            {
                ttAddToPlaylist.IsOpen = false;
                ttAddedToPlaylist.IsOpen = true;
                await Task.Delay(2000);
                ttAddedToPlaylist.IsOpen = false;
            }

        }

        private void hypPlaylistAdded_Click(object sender, RoutedEventArgs e)
        {
            if (hypPlaylistAdded.Tag is PlaylistItem playlistItem)
            {
                if (App.NavigationFrame != null)
                    App.NavigationFrame.Navigate(typeof(PlaylistView), playlistItem);
            }
        }

        private async void mnftAddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel group && group.Data is SongModel song)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                var pathtocheck = song.FilePath;
                if (pathtocheck == null) return;
                var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
                if (existing != null)
                {
                    song.IsFavourite = false;
                    Favourites.Remove(existing);
                    song.FavString = "Add to Favourites";

                }
                else
                {
                    Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    song.IsFavourite = true;
                    song.FavString = "Remove from Favourites";

                }


                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }

        }
        SongModel currentcontextfileitem = new();
        private void btnOpenRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "shell:RecycleBinFolder",
                UseShellExecute = true
            });

        }

        private void ttInaccessibleFiles_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (PathsIncomplete.Count > 0)
            {
                if (chckSkipForAllFiles.IsChecked == false)
                {
                    args.Cancel = true;
                    Debug.WriteLine("yess");
                    string removedFile = PathsIncomplete[0];
                    currentFileInactive = removedFile;
                    Debug.WriteLine(removedFile + " is removed from queue");
                    PathsIncomplete.Remove(removedFile);
                    ifbFileInUse.Message = $"The selected file '{removedFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                }
            }
        }

        private async void ttInaccessibleFiles_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            args.Cancel = true;
            var item = currentFileInactive;
            var lockingprocesses = GetLockingProcess.GetLockingProcesses(item);
            if (lockingprocesses.Count == 0)
            {
                var exist2 = PathsIncomplete.FirstOrDefault(p => p == currentFileInactive);
                if (exist2 != null)
                {
                    PathsIncomplete.Remove(exist2);
                }
                Debug.WriteLine("ZERO PROCESS: " + item);

                await DeleteSingleFileAsync(item);

                var exist = TimelineCollection.FirstOrDefault(p => p.Data.FilePath == item);
                if (exist != null)
                {
                    TimelineCollection.Remove(exist);
                    List<SongModel> inenumerable = new List<SongModel>();
                    foreach (var timelineitem in TimelineCollection)
                    {
                        inenumerable.Add(timelineitem.Data);
                    }
                    GenerateTimeline(inenumerable);
                }
                if (PathsIncomplete.Count != 0)
                {
                    var currentFile = PathsIncomplete[0];
                    ifbFileInUse.IsOpen = true;
                    ifbFileInUse.Severity = InfoBarSeverity.Error;
                    ifbFileInUse.Message = $"The selected file '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                }
                else
                {
                    ttInaccessibleFiles.Hide();
                }
            }
            else
            {
                if (App.MainWindowInstance == null) return;
                string currentFile = item;

                ifbFileInUse.IsOpen = true;
                ifbFileInUse.Severity = InfoBarSeverity.Error;
                ifbFileInUse.Message = $"The selected file '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                //                ttInaccessibleFiles.IsOpen = true;



            }

        }


        private void mnftDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModel file && file.Data is SongModel song)
            {
                if (App.MainWindowInstance == null) return;
                currentcontextfileitem = song;
                OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{song.Title}' will be sent to the Recycle Bin. You can restore it from the Recycle Bin if needed.");

                OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested; ;
                OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested; ;
            }
        }
        ObservableCollection<string> PathsIncomplete = new();
        private string currentFileInactive = "";

        private async Task DeleteSingleFileAsync(string path)
        {
            try
            {
                var storagefile = await StorageFile.GetFileFromPathAsync(path);
                await storagefile.DeleteAsync(StorageDeleteOption.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An unexpected error occured. Check log page for more details");
                Logger.Log(ex.Message, "DeleteFileFolderPage", Logger.LogLevelType.Error);
            }
        }

        private async void OceanContentDialog_PrimaryRequested()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
            PathsIncomplete.Clear();
            var item = currentcontextfileitem;
            Debug.WriteLine("DELETE FILE: " + item.FilePath);

            var lockingprocesses = GetLockingProcess.GetLockingProcesses(item.FilePath);
            if (lockingprocesses.Count == 0)
            {
                Debug.WriteLine("ZERO PROCESS: " + item.FilePath);


                await DeleteSingleFileAsync(item.FilePath);

                var exist = TimelineCollection.FirstOrDefault(p => p.Data.FilePath == item.FilePath);
                if (exist != null)
                {
                    TimelineCollection.Remove(exist);
                    List<SongModel> inenumerable = new List<SongModel>();
                    foreach (var timelineitem in TimelineCollection)
                    {
                        inenumerable.Add(timelineitem.Data);
                    }
                    GenerateTimeline(inenumerable);
                }
            }
            else
            {
                Debug.WriteLine("MULTI PROCESS: " + item.FilePath);
                //bool onlyVusicPlayer = lockingprocesses.All(p => p.ProcessName == "Vusic Player");
                //if (onlyVusicPlayer)
                //{
                //    if (PlayerService.Masterplayer != null)
                //    {
                //        PlayerService.filestreamcurrent?.Dispose();
                //        var filelocked2 = GetLockingProcess.GetLockingProcesses(item.Path);
                //        if (filelocked2.Count == 0)
                //        {


                //        }
                //    }

                Debug.WriteLine("ADDING: " + item.FilePath);
                PathsIncomplete.Add(item.FilePath);


            }
            if (PathsIncomplete.Count == 0)
            {
                ttDeletedFiles.IsOpen = true;
                ifbDeleteFiles.IsOpen = true;
                ifbDeleteFiles.Title = "Completed";
                ifbDeleteFiles.Severity = InfoBarSeverity.Success;
                ifbDeleteFiles.Message = "Successfully deleted! You can restore it from the Recycle Bin if needed.";
            }
            else
            {
                Debug.WriteLine("DDHJLFHJOOFH");
                if (App.MainWindowInstance == null) return;
                string currentFile = PathsIncomplete[0];
                PathsIncomplete.Remove(currentFile);
                currentFileInactive = currentFile;
                ifbFileInUse.IsOpen = true;
                ifbFileInUse.Severity = InfoBarSeverity.Error;
                ifbFileInUse.Message = $"The selected item '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                chckSkipForAllFiles.IsChecked = false;
                if (App.MainWindowInstance.Content != null)
                {
                    ttInaccessibleFiles.XamlRoot = App.MainWindowInstance.Content.XamlRoot;
                }

                // FIX 2: Prevent "ContentDialog is already open" or visual tree tracking errors
                try
                {
                    await ttInaccessibleFiles.ShowAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Dialog failed to show: {ex.Message}");
                }
                //                ttInaccessibleFiles.IsOpen = true;



            }
        }
        private bool _isopening = false;

        private void hypPlaylistClick_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyp && hyp.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem playlist)
            {
                if (_isopening) return;
                try
                {

                    _isopening = true;
                    if (App.NavigationFrame == null) return;
                    App.NavigationFrame.Navigate(typeof(PlaylistView), playlist);

                }
                finally
                {
                    _isopening = false;
                }
            }
        }

        private async void btnPlayAllPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button hyp && hyp.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, false);
            }

        }

        private async void mnftPlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, false);
            }
        }

        private void mnftEditPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem currentPlaylist)
            {
                if (App.MainWindowInstance == null) return;
                PlaylistCreation.playlistItem = currentPlaylist;
                OceanContentDialog.Show("Edit Playlist", "Save", "", "Cancel", OceanDialogWindow.ContentType.PlaylistEdit, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "saveicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", currentPlaylist, true);


                OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
                OceanContentDialog.PrimaryRequested += () =>
                {
                    Debug.WriteLine(PlaylistCreation.playlistItem.PlaylistName + " shshd");
                    currentPlaylist = PlaylistCreation.playlistItem;
                    foreach(var song in PlaylistCreation.playlistItem.SongsPaths)
                    {
                        Debug.WriteLine("DODNG " + song);
                    }
                    group.Data.PlaylistName = PlaylistCreation.playlistItem.PlaylistName;
                    group.Data.PlaylistCount = PlaylistCreation.playlistItem.PlaylistCount;
                    group.Data.PlaylistGenre = PlaylistCreation.playlistItem.PlaylistGenre;
                    group.Data.SongsPaths = PlaylistCreation.playlistItem.SongsPaths;
                    currentPlaylist.PlaylistName = PlaylistCreation.playlistItem.PlaylistName;
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                    List<PlaylistItem> inenumerable = new List<PlaylistItem>();
                    foreach (var timelineitem in TimelineCollectionPlaylist)
                    {
                        inenumerable.Add(timelineitem.Data);
                    }
                    GenerateTimelinePlaylist(inenumerable);
                };
            }
        }

        private void mnftOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem playlist)
            {
                if (_isopening) return;
                try
                {

                    _isopening = true;
                    if (App.NavigationFrame == null) return;
                    App.NavigationFrame.Navigate(typeof(PlaylistView), playlist);

                }
                finally
                {
                    _isopening = false;
                }
            }

        }

        private async void mnftShuffleAndPlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, false);
            }
        }

        private async void mnftDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuFlyoutItem mnft && mnft.DataContext is GroupedCollectionModelPlaylist group && group.Data is PlaylistItem playlist)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playlists = currentSettings.SavedPlaylists;
                var exist = playlists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId);
                if(exist != null)
                {
                    playlists.Remove(exist);
                }
                TimelineCollectionPlaylist.Remove(group);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }
    }
}

