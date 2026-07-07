using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Page = Microsoft.UI.Xaml.Controls.Page;


namespace Vusic_Player.Pages
{
   
    public sealed partial class MainPage : Page
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        private DispatcherTimer? _placeholderTimer;
        private List<string>? _suggestions;
        private int _currentIndex = 0;
        private object? _originalHeader;
        public MainPage()
        {
            InitializeComponent();
            App.NavigationFrame = frmMain;
            App.VideoPlayerFrame = frmVid;
            App.MasterFrame = frmRoot;
            frmMain.Navigate(typeof(HomeView));
            PlayerService.mainXamlRoot = XamlRoot;

            _originalHeader = nvgMain.Header;
        }
        private void UpdatePlaceholderText()
        {
            _suggestions = new List<string>
        {
            "Search for files across your library...",
            "Try 'Recent Music'...",
            "Find 'User Profile'...",
            "Looking for 'Advanced Tools'?"
        };

            // 2. Initialize the Timer
            _placeholderTimer = new DispatcherTimer();
            _placeholderTimer.Interval = TimeSpan.FromSeconds(3);
            _placeholderTimer.Tick += _placeholderTimer_Tick; ;

            // 3. Start the timer
            _placeholderTimer.Start();
        }

        private void _placeholderTimer_Tick(object? sender, object e)
        {
            if (_suggestions == null) return;
            _currentIndex = (_currentIndex + 1) % _suggestions.Count;

            // Update the UI
            asbMaster.PlaceholderText = _suggestions[_currentIndex];
        }


        private void nvgMain_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (frmMain.CanGoBack)
            {
                frmMain.GoBack();
            }
        }
        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }
        private void SetGridBackground()
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/oceanglow.png"));

            // 2. Create an ImageBrush and set its ImageSource
            ImageBrush imageBrush = new ImageBrush
            {
                ImageSource = bitmapImage,
                Stretch = Stretch.UniformToFill // Controls how the image scales to fit the grid
            };

            // 3. Assign the brush to the Grid's Background
            grdRoot.Background = imageBrush;
        }
        private async void FrmMain_Navigated(object sender, NavigationEventArgs e)
        {
            nvgMain.AlwaysShowHeader = true;
            nvgMain.Header = _originalHeader;
            nvgMain.IsPaneVisible = true;
            grdsplittr.Visibility = Visibility.Visible;
            blackBackground.Visibility = Visibility.Collapsed;
            ColumnMaster.MinWidth = 280;
            ColumnMaster.Width = new GridLength(300);
            MusicPlayerMaster.Visibility = Visibility.Visible;
        //    SetGridBackground();
            if (e.SourcePageType == typeof(HomeView))
                nvgMain.Header = "Home";

            else if (e.SourcePageType == typeof(MusicLibrary))
                nvgMain.Header = "Music Library";

            else if (e.SourcePageType == typeof(VideoLibrary))
                nvgMain.Header = "Video Library";
            else if (e.SourcePageType == typeof(EntireMusicLibrary))
            {
                nvgMain.Header = "Music Library";
                //if (LibraryStore.IsMusicLibrary)
                //{
                //    nvgMain.Header = "Music Library";
                //}
                //else
                //{
                //    nvgMain.Header = "Video Library";
                //}
            }
            else if(e.SourcePageType == typeof(FolderView))
            {
                nvgMain.Header = "";
                nvgMain.SelectedItem = null;
            }
            else if (e.SourcePageType == typeof(QueuePage))
            {
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
                ColumnMaster.MinWidth = 0;
                ColumnMaster.Width = new GridLength(0);
                grdsplittr.Visibility = Visibility.Collapsed;
                nvgMain.Header = "";

            }
            else if (e.SourcePageType == typeof(MusicPlayerFull))
            {
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
                ColumnMaster.MinWidth = 0;
                ColumnMaster.Width = new GridLength(0);
                grdsplittr.Visibility = Visibility.Collapsed;
                nvgMain.Header = "";

            }
            else if(e.SourcePageType == typeof(VideoPlayer))
            {
                nvgMain.AlwaysShowHeader = false;
                nvgMain.Header = null;
                blackBackground.Visibility = Visibility.Visible;
                nvgMain.IsPaneVisible = false;
                grdsplittr.Visibility = Visibility.Collapsed;
                ColumnMaster.MinWidth = 0;
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
            //    grdRoot.Background = null;
                ColumnMaster.Width = GridLength.Auto;
            }
            else if (e.SourcePageType == typeof(SettingsPage))
                nvgMain.Header = "App Settings";
            else if (e.SourcePageType == typeof(LoggerPage))
                nvgMain.Header = "App Logs";

            else if (e.SourcePageType == typeof(SearchResults))
                nvgMain.Header = "Search Results";


            else
                nvgMain.Header = "";
        }

        private void nvgMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MusicPlayerMaster.Visibility = Visibility.Visible;
            if (args.IsSettingsSelected)
            {
                frmMain.Navigate(typeof(SettingsPage));
                return;
            }

            if (args.SelectedItemContainer == null)
                return;

            Type? pageType = null;

            if (args.SelectedItemContainer == nvgitHome)
                pageType = typeof(HomeView);

            else if (args.SelectedItemContainer == nvgitMusic)
                pageType = typeof(MusicLibrary);

            else if (args.SelectedItemContainer == nvgitVideo)
                pageType = typeof(VideoLibrary);

            else if (args.SelectedItemContainer == nvgitQueue)
            {
                frmMain.Navigate(typeof(QueuePage), null, new DrillInNavigationTransitionInfo());
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
            }

            if (pageType != null && frmMain.CurrentSourcePageType != pageType)
            {
                frmMain.Navigate(pageType, null, new DrillInNavigationTransitionInfo());
            }
        }

        private void asbMaster_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void asbMaster_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void asbMaster_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void sldVolume_ValueChanged(double obj)
        {

        }

        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediacontroller.ArtistDisplayName == "Unknown Artist") return;
            if(App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(ArtistView), mediacontroller.ArtistDisplayName);
            }
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (mediacontroller.ArtistDisplayName == "Unknown Album") return;

            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(AlbumView), mediacontroller.AlbumDisplayName);
            }
        }
    }
}
