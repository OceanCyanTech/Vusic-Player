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
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class OnlineLyrics : UserControl
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        private readonly HttpClient _httpClient;

        public OnlineLyrics()
        {
            InitializeComponent();
            lstViewQueryResults.ItemsSource = lyricTracksQueryResults;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://lrclib.net/")
            };
            // LRCLIB requests a descriptive User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VusicPlayer/1.1.0.0 (https://github.com/OceanCyanTech/Vusic-Player)");
        }
        ObservableCollection<LrcTrack> lyricTracksQueryResults = new();
        public async Task<List<LrcTrack>> SearchLyricsAsync(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<LrcTrack>>(
                    $"/api/search?q={Uri.EscapeDataString(query)}",
                    cancellationToken
                );
                return response ?? new List<LrcTrack>();
            }
            catch (OperationCanceledException)
            {
                return new List<LrcTrack>();
            }
            catch (Exception)
            {
                return new List<LrcTrack>();
            }
        }
        private CancellationTokenSource? _loadingCts;
        private DispatcherTimer? cooldownTimerFind;

        private async Task AnimateStatusAsync(string baseText)
        {
            _loadingCts = new CancellationTokenSource();
            var token = _loadingCts.Token;

            int dots = 0;

            while (!token.IsCancellationRequested)
            {
                dots = (dots % 3) + 1;
                txtLoading.Text = baseText + new string('.', dots);

                await Task.Delay(400);
            }
        }
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:network",
                UseShellExecute = true
            });
        }


        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearchResHeader.Visibility = Visibility.Collapsed;
            txtLyricHeader.Visibility = Visibility.Collapsed;
            if (txtQuery.Text == "") return;
            if (CheckInternet.IsInternetAvailable())
            {
                lyricTracksQueryResults.Clear();
                txtLoading.Visibility = Visibility.Visible;
                _ = AnimateStatusAsync("Loading");
                var result = await SearchLyricsAsync(txtQuery.Text);
                if (result != null)
                {
                    int remaining = 10;
                    btnSearch.IsEnabled = false;

                    cooldownTimerFind = new DispatcherTimer();
                    cooldownTimerFind.Interval = TimeSpan.FromSeconds(1);
                    cooldownTimerFind.Tick += (s, e) =>
                    {
                        remaining--;
                        txtBtnFind.Text = $"Find (Wait {remaining}s...)";

                        if (remaining <= 0)
                        {
                            cooldownTimerFind.Stop();
                            btnSearch.IsEnabled = true;
                            txtBtnFind.Text = "Find";
                        }
                    };
                    cooldownTimerFind.Start();
                    txtSearchResHeader.Visibility = Visibility.Visible;

                    foreach (var item in result)
                    {
                        double secondsFromApi = item.Duration;
                        TimeSpan time = TimeSpan.FromSeconds(secondsFromApi);

                        // Format it as "MM:SS" (e.g., "03:35")
                        string displayDuration = time.ToString(@"mm\:ss");
                        lyricTracksQueryResults.Add(new LrcTrack { TrackName = item.TrackName, AlbumName = item.AlbumName, ArtistName = item.ArtistName, StringDuration = displayDuration, PlainLyrics = item.PlainLyrics, SyncedLyrics = item.SyncedLyrics, Id= item.Id });
                    }
                    txtLoading.Visibility = Visibility.Collapsed;

                }
            }
            else
            {
                ifbError.Title = "You're not connected to the internet";
                ifbError.Severity = InfoBarSeverity.Error;
                ifbError.ActionButton.Visibility = Visibility.Visible;
                ifbError.IsOpen = true;
                ifbError.ActionButton.Click -= ActionButton_Click;
                ifbError.ActionButton.Click += ActionButton_Click;

            }
        }

        private void lstViewQueryResults_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("OR SOMEONE OUT THERE");
            if (e.ClickedItem is LrcTrack lrc)
            {
                btnCopyLyrics.Visibility = Visibility.Visible;
                btnSaveSelectedLyricsToFile.Visibility = Visibility.Visible;
                txtLyricHeader.Visibility = Visibility.Visible;
                Debug.WriteLine("HMMS");
                txtLyricsFull.Blocks.Clear();

                if (string.IsNullOrWhiteSpace(lrc.PlainLyrics))
                {
                    Debug.WriteLine("AS A ATIME ");
                    // Handle empty lyrics scenario
                    var emptyParagraph = new Paragraph();
                    emptyParagraph.Inlines.Add(new Run { Text = "No plain lyrics available.", FontStyle = Windows.UI.Text.FontStyle.Italic });
                    txtLyricsFull.Blocks.Add(emptyParagraph);
                    return;
                }

                // 2. Split the single string into an array of lines
                string[] lines = lrc.PlainLyrics.Split('\n');
                if (string.IsNullOrWhiteSpace(lrc.SyncedLyrics))
                {
                    txtLyricHeader.Text = "Lyrics Preview";
                }
                else
                {
                    txtLyricHeader.Text = "Lyrics Preview (synced with time)";
                }
                // 3. Loop through each line and add it to the UI
                foreach (string line in lines)
                {
                    var paragraph = new Paragraph();
                    var run = new Run { Text = line };

                    paragraph.Inlines.Add(run);

                    // Add the paragraph block to your RichTextBlock
                    txtLyricsFull.Blocks.Add(paragraph);
                }
            }
        }

        private void lstViewQueryResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private async void btnSaveSelectedLyricsToFile_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            if (lstViewQueryResults.SelectedItem is LrcTrack lyric)
            {
                string artist = lyric.ArtistName;
                string trackName = lyric.TrackName;
                string plainLyricsText = lyric.PlainLyrics;
                string syncedLyricsText = lyric.SyncedLyrics;

                // 1. Initialize the FileSavePicker
                FileSavePicker savePicker = new FileSavePicker();

                // CRITICAL WINUI 3 STEP: Retrieve the window handle (HWND) of the current Window 
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

                // 2. Configure Picker Properties
                savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                savePicker.SuggestedFileName = $"{artist} - {trackName}";

                // 3. Add Extension Choices to the Dropdown
                // The key is the display text, the value is a list of matching extensions
                savePicker.FileTypeChoices.Add("Synced Lyrics File (lrc file)", new List<string>() { ".lrc" });
                savePicker.FileTypeChoices.Add("Plain Text File (txt file)", new List<string>() { ".txt" });

                // 4. Open the Dialog Box
                var file = await savePicker.PickSaveFileAsync();

                // 5. If the user didn't hit 'Cancel', write the contents based on the choice
                if (file != null)
                {
                    try
                    {
                        // Determine which content format to write based on what extension the user selected
                        string contentToSave = file.FileType.Equals(".lrc", StringComparison.OrdinalIgnoreCase)
                            ? syncedLyricsText
                            : plainLyricsText;

                        // Write the text to the target file path safely using asynchronous IO
                        await File.WriteAllTextAsync(file.Path, contentToSave);
                        hypPathSaved.Content = file.Path;
                        ttJustSaved.IsOpen = true;
                        ttJustSaved.Title = "Saved to file successfully";
                        await Task.Delay(2000);
                        ttJustSaved.IsOpen = false;
                    }
                    catch (Exception ex)
                    {
                        // Handle file access or disk permission issues gracefully
                        System.Diagnostics.Debug.WriteLine($"Failed to save file: {ex.Message}");
                    }
                }
            }
        }

        private async void btnCopyLyrics_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var block in txtLyricsFull.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    foreach (var inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            sb.Append(run.Text);
                        }
                    }
                    // Add a newline after every paragraph to maintain lyric structure
                    sb.AppendLine();
                }
            }

            string fullText = sb.ToString().TrimEnd();

            CopyToClipboard.CopyStringToClipboard(fullText);
            btnCopyLyrics.Content = "Copied ✔️";
            await Task.Delay(2000);
            btnCopyLyrics.Content = "Copy Selected Lyrics";
        }

        private void hypPathSaved_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(hypPathSaved.Content.ToString()))
            {
                Process.Start("explorer.exe", $"/select,\"{hypPathSaved.Content.ToString()}\"");
            }
        }
    }
}
