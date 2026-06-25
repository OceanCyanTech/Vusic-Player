using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Paragraph = Microsoft.UI.Xaml.Documents.Paragraph;
using Run = Microsoft.UI.Xaml.Documents.Run;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{

    public sealed partial class MusicPlayerFull : Page
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public MusicPlayerFull()
        {
            InitializeComponent();
            PlayerService.PlayCalled -= PlayerService_PlayCalled;
            PlayerService.PlayCalled += PlayerService_PlayCalled;
        }

        private void PlayerService_PlayCalled()
        {
            if(PlayerService.Masterplayer != null )
            {
                if (PlayerService.Masterplayer.IsPlaying)
                {
                    lyrictimer.Start();
                }
                else
                {
                    lyrictimer.Stop();
                }
            }
        }

        public event EventHandler<Type>? NavigationRequested;
        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }

        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtArtist_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame == null) return;
            App.NavigationFrame.Navigate(typeof(ArtistView), mediacontroller.ArtistDisplayName);
        }
        DispatcherTimer lyrictimer = new();
        ObservableCollection<LyricLineModel> LyricsList = new ObservableCollection<LyricLineModel>();
        private void txtAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame == null) return;
            App.NavigationFrame.Navigate(typeof(AlbumView), mediacontroller.AlbumDisplayName);

        }

        public async Task LoadLyricsFromFileAsync(string filePath)
        {
            try
            {

                if (File.Exists(filePath))
                {
                    // Read all text asynchronously
                    string[] lines = await File.ReadAllLinesAsync(filePath);
                    var lrcRegex = new Regex(@"^\[(?<min>\d{2}):(?<sec>\d{2})\.(?<ms>\d{2})\](?<text>.*)$");
                    foreach (var line in lines)
                    {
                        var match = lrcRegex.Match(line.Trim());

                        if (match.Success)
                        {
                            // 1. Extract the time components from the regex groups
                            int minutes = int.Parse(match.Groups["min"].Value);
                            int seconds = int.Parse(match.Groups["sec"].Value);
                            // LRC uses centiseconds (hundredths of a second), so multiply by 10 for milliseconds
                            int milliseconds = int.Parse(match.Groups["ms"].Value) * 10;

                            // 2. Create a TimeSpan object
                            TimeSpan timestamp = new TimeSpan(0, 0, minutes, seconds, milliseconds);

                            // 3. Extract the text
                            string lyricText = match.Groups["text"].Value;
                            Debug.WriteLine(lyricText);
                            Debug.WriteLine(timestamp.ToString(@"hh\:mm\:ss\.ff"));

                            LyricsList.Add(new LyricLineModel { Line = lyricText, Timestamp = timestamp });
                        }
                        
                    }
                    btnFullLyrics.Visibility = Visibility.Visible;
                }
                else
                {
                    // Handle file not found (e.g., clear UI or show an error)
                    txtLyricRealTime.Text = "LRC file not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle potential I/O exceptions
                System.Diagnostics.Debug.WriteLine($"Error loading LRC file: {ex.Message}");
            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            txtLyricRealTime.Text = "";
            Debug.WriteLine("REQUESTED TICKING");

            LyricsList.Clear();
            lyrictimer = new DispatcherTimer();
            // Tick frequently enough for smooth sub-second line changes (e.g., every 100ms)
            lyrictimer.Interval = TimeSpan.FromMilliseconds(100);
            await LoadLyricsFromFileAsync(@"C:\Users\bnara\OneDrive\Documents\housethatalwaysrainslyrics.lrc");
            lyrictimer.Tick += Lyrictimer_Tick;
            lyrictimer.Start();
            //     StartPlayback();
        }

        private void Lyrictimer_Tick(object? sender, object e)
        {
            if (PlayerService.Masterplayer == null) return;

            var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);

            // Find the current lyric line that matches the playback window
            var currentLyric = LyricsList
                .Where(p => p.Timestamp <= curTime)
                .LastOrDefault();

            if (currentLyric != null && txtLyricRealTime.Text != currentLyric.Line)
            {
                // Only update the UI text block if the lyric actually changed
                txtLyricRealTime.Text = currentLyric.Line;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txtLyricHeader.Text = "Full lyrics";
            txtLyricsFull.Blocks.Clear();
            foreach(var line in LyricsList)
            {
                Run textRun = new Run { Text = line.Line };
                Paragraph paragraph = new Paragraph();
                textRun.FontSize = 24;
                // Add space below this specific line so lyrics don't look crammed
                paragraph.Margin = new Thickness(0, 0, 0, 14);
                paragraph.Inlines.Add(textRun);

                // 4. Add the paragraph block straight to your RichTextBlock
                txtLyricsFull.Blocks.Add(paragraph);
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Find Lyrics Online", "Load Lyrics", "", "Cancel", OceanDialogWindow.ContentType.LyricSearchOnline, OceanContentDialogDefault.Primary, XamlRoot, 900, 960, OceanContentDialogType.Elevated, App.MainWindowInstance, "appicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested; ;
        }

        private void OceanContentDialog_PrimaryRequested()
        {
    
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CopyToClipboard.CopyStringToClipboard(txtLyricRealTime.Text);
        }
    }
}
