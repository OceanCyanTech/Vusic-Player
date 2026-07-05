using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using static Vusic_Player.UI.UserViews.Controls.ListViewMedia;


namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class GroupedCollectionView : UserControl
    {
        public GroupedCollectionView()
        {
            InitializeComponent();
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
            }
        }
        private void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource is IEnumerable<SongModel> list)
            {
                GenerateTimeline(list);
            }
        }
        private void btnGlyph_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnFavourite_Click(object sender, RoutedEventArgs e)
        {

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

        //private void LoadDummyData()
        //{
        //    // A - Astronomy
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Astronomy", Artist = "Conan Gray", Album = "Superache", Letter = "A", IsGroupStart = true });

        //    // B - Best Friend, Bourgeoisieses
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Best Friend", Artist = "Conan Gray", Album = "Superache", Letter = "B", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Bourgeoisieses", Artist = "Conan Gray", Album = "Found Heaven", Letter = "B", IsGroupStart = false });

        //    // C - Checkmate, Comfort Crowd, Crush Culture
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Checkmate", Artist = "Conan Gray", Album = "Kid Krow", Letter = "C", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Comfort Crowd", Artist = "Conan Gray", Album = "Kid Krow", Letter = "C", IsGroupStart = false });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Crush Culture", Artist = "Conan Gray", Album = "Sunset Season", Letter = "C", IsGroupStart = false });

        //    // F - Family Line, Footnote, Found Heaven
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Family Line", Artist = "Conan Gray", Album = "Superache", Letter = "F", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Footnote", Artist = "Conan Gray", Album = "Superache", Letter = "F", IsGroupStart = false });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Found Heaven", Artist = "Conan Gray", Album = "Found Heaven", Letter = "F", IsGroupStart = false });

        //    // H - Heather, Hollywood
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Heather", Artist = "Conan Gray", Album = "Kid Krow", Letter = "H", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Hollywood", Artist = "Conan Gray", Album = "Found Heaven", Letter = "H", IsGroupStart = false });

        //    // J - Jigsaw
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Jigsaw", Artist = "Conan Gray", Album = "Superache", Letter = "J", IsGroupStart = true });

        //    // L - Little League, Lookalike
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Little League", Artist = "Conan Gray", Album = "Kid Krow", Letter = "L", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Lookalike", Artist = "Conan Gray", Album = "Sunset Season", Letter = "L", IsGroupStart = false });

        //    // M - Memories, Maniac
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Memories", Artist = "Conan Gray", Album = "Superache", Letter = "M", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Maniac", Artist = "Conan Gray", Album = "Kid Krow", Letter = "M", IsGroupStart = false });

        //    // N - Never Ending Song
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Never Ending Song", Artist = "Conan Gray", Album = "Found Heaven", Letter = "N", IsGroupStart = true });

        //    // O - Online Love
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Online Love", Artist = "Conan Gray", Album = "Kid Krow", Letter = "O", IsGroupStart = true });

        //    // P - People Watching
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "People Watching", Artist = "Conan Gray", Album = "Superache", Letter = "P", IsGroupStart = true });

        //    // T - The Exit, Telepath, The King
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "The Exit", Artist = "Conan Gray", Album = "Superache", Letter = "T", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Telepath", Artist = "Conan Gray", Album = "Single", Letter = "T", IsGroupStart = false });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "The King", Artist = "Conan Gray", Album = "Single", Letter = "T", IsGroupStart = false });

        //    // V - Vengeance
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Vengeance", Artist = "Conan Gray", Album = "Found Heaven", Letter = "V", IsGroupStart = true });

        //    // W - Winner, Wish You Were Sober
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Winner", Artist = "Conan Gray", Album = "Found Heaven", Letter = "W", IsGroupStart = true });
        //    TimelineCollection.Add(new SemanticZooMitemtest { Title = "Wish You Were Sober", Artist = "Conan Gray", Album = "Kid Krow", Letter = "W", IsGroupStart = false });
        //    MapGridView.ItemsSource = TimelineCollection
        //            .Where(x => x.IsGroupStart)
        //            .Select(x => x.Letter)
        //            .ToList();        // If you don't have INotifyPropertyChanged, just re-bind the GridView ItemsSource
        //}
        public ObservableCollection<GroupedCollectionModel> TimelineCollection { get; set; }
                = new ObservableCollection<GroupedCollectionModel>();
        //public void SetDummyDataSource()
        //{
        //    // 1. Create a raw list of mock songs
        //    var rawSongs = new List<SongModel>
        //    { 
        //   // A Group
        //    new SongModel { Title = "Astronomy", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "Affluenza", Artist = "Conan Gray", AlbumName = "Kid Krow" },
        //    new SongModel { Title = "Alley Rose", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // B Group
        //    new SongModel { Title = "Best Friend", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "Bourgeoisieses", Artist = "Conan Gray", AlbumName = "Found Heaven" },
        //    new SongModel { Title = "Bubblegum", Artist = "Conan Gray", AlbumName = "Sunset Season" },

        //    // C Group
        //    new SongModel { Title = "Comfort Crowd", Artist = "Conan Gray", AlbumName = "Kid Krow" },
        //    new SongModel { Title = "Checkmate", Artist = "Conan Gray", AlbumName = "Kid Krow" },
        //    new SongModel { Title = "Crush Culture", Artist = "Conan Gray", AlbumName = "Sunset Season" },
        //    new SongModel { Title = "Cleanup", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // F Group
        //    new SongModel { Title = "Family Line", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "Fake", Artist = "Lauv & Conan Gray", AlbumName = "Single" },
        //    new SongModel { Title = "Footnote", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "Found Heaven", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // H Group
        //    new SongModel { Title = "Heather", Artist = "Conan Gray", AlbumName = "Kid Krow" },
        //    new SongModel { Title = "Holidays", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // J Group
        //    new SongModel { Title = "Jigsaw", Artist = "Conan Gray", AlbumName = "Superache" },

        //    // L Group
        //    new SongModel { Title = "Lookalike", Artist = "Conan Gray", AlbumName = "Sunset Season" },
        //    new SongModel { Title = "Little League", Artist = "Conan Gray", AlbumName = "Kid Krow" },
        //    new SongModel { Title = "Lonely Dancers", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // M Group
        //    new SongModel { Title = "Maniac", Artist = "Conan Gray", AlbumName = "Kid Krow" },
        //    new SongModel { Title = "Memories", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "Movies", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "Miss You", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // N Group
        //    new SongModel { Title = "Never Ending Song", Artist = "Conan Gray", AlbumName = "Found Heaven" },
        //    new SongModel { Title = "Night Drive", Artist = "Conan Gray", AlbumName = "Single" },

        //    // P Group
        //    new SongModel { Title = "People Watching", Artist = "Conan Gray", AlbumName = "Superache" },

        //    // T Group
        //    new SongModel { Title = "The Exit", Artist = "Conan Gray", AlbumName = "Superache" },
        //    new SongModel { Title = "The King", Artist = "Conan Gray", AlbumName = "Single" },
        //    new SongModel { Title = "Telepath", Artist = "Conan Gray", AlbumName = "Single" },

        //    // V Group
        //    new SongModel { Title = "Vengeance", Artist = "Conan Gray", AlbumName = "Found Heaven" },

        //    // W Group
        //    new SongModel { Title = "Winner", Artist = "Conan Gray", AlbumName = "Found Heaven" },
        //    new SongModel { Title = "Wish You Were Sober", Artist = "Conan Gray", AlbumName = "Kid Krow" },

        //    // # Group (Numbers/Symbols)
        //    new SongModel { Title = "80s Stars", Artist = "Conan Gray", AlbumName = "Single" }
        //    };
        //    // 2. Group them by the first letter
        //    SetDataSource(rawSongs, s => char.IsDigit(s.Title![0]) ? "#" : s.Title[0].ToString().ToUpper());

        //}
        //public void SetDataSource<T>(IEnumerable<T> items, Func<T, string> keySelector)
        //{
        ////    var groups = items
        ////       .GroupBy(keySelector)
        ////       .Select(g => new GenericGroup
        ////       {
        ////           Header = g.Key.ToString().ToUpper(),
        ////           Items = g.ToList()
        ////       })
        ////       .OrderBy(g => g.Header)
        ////       .ToList(); // Materialize the list first

        ////    // Update the backing collection
        ////    GroupedCollection.Clear();
        ////    foreach (var g in groups) GroupedCollection.Add(g);

        ////    // Re-assigning ensures the Repeater resets its scroll position and internal cache
        ////    MainRepeater.ItemsSource = null;
        ////    MainRepeater.ItemsSource = GroupedCollection;
        ////}
        ////private void Button_Click(object sender, RoutedEventArgs e)
        ////{
        ////    var clickedButton = sender as Button;

        ////    // Set the target to the specific button that was just clicked
        ////    ttJumpMap.Target = clickedButton;

        ////    // Open the tip
        ////    ttJumpMap.IsOpen = true;
        ////    grdViewMap.ItemsSource = AllHeaders;
        //}

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
                grdNoSearchResults.Visibility = Visibility.Collapsed;

                MainTimelineList.ItemsSource = TimelineCollection;
                MainTimelineList.Visibility = Visibility.Visible;

                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResults(sender.Text);

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };

                MainTimelineList.ItemsSource = searchresults;
            }
        }



        private void asbSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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
    }
}
