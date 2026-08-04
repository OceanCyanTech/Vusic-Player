using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using FileSavePicker = Windows.Storage.Pickers.FileSavePicker;
using Paragraph = Microsoft.UI.Xaml.Documents.Paragraph;
using PickerLocationId = Windows.Storage.Pickers.PickerLocationId;
using Run = Microsoft.UI.Xaml.Documents.Run;


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
            LyricsList.CollectionChanged -= LyricsList_CollectionChanged;
            LyricsList.CollectionChanged += LyricsList_CollectionChanged;
        }

        private void LyricsList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("CHECH");
            if (LyricsList.Count != 0)
            {
                Debug.WriteLine("CHECH1");

                btnCopyLyric.Visibility = Visibility.Visible;
                btnFullLyrics.IsEnabled = true;
            }
            else
            {
                Debug.WriteLine("CHEC2");
                CloseLyrics();
                btnCopyLyric.Visibility = Visibility.Collapsed;
                btnFullLyrics.IsEnabled = false;
            }
        }

        private async void btnOpenMedia_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var media = await MediaPicker.PickSingle(App.MainWindowInstance, "Open Media");
            if (media != null)
            {
                bool isAudio = AudioExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                bool isVideo = VideoExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                if (isAudio)
                {

                    if (File.Exists(media.Path))
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(media.Path);

                        single.Add(new SongModel { FilePath = media.Path, Title = Title, AlbumName = AudioMetadata.Album(media.Path), Artist = AudioMetadata.Artist(media.Path), SongDuration = await AudioMetadata.GetTimeSpanDuration(media.Path) });
                        QueueService.PlayMedia(single, false, false);
                    }
                }
                else if (isVideo)
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
                else
                {
                    //Handle other cases
                }
            }
        }


        private void PlayerService_PlayCalled()
        {
            if (PlayerService.Masterplayer != null)
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
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("Navigated to Music Player Full");
            if (PlayerService.CurrentPlayingPath != "")
            {
                btnCustomizeLyricText.Visibility = Visibility.Visible;
                stkLyricFunctions.Visibility = Visibility.Visible;
            }
            if (e.Parameter is string information)
            {
                LyricContainer.Visibility = Visibility.Collapsed;
                musicPlayerMaster.Visibility = Visibility.Collapsed;
                colLyricContainer.Width = GridLength.Auto;
                fulllyrics.Margin = new Thickness(20, 0, 0, 0);
                await LoadLyricsFromFileAsync(information);
                ViewFullLyrics();
                txtLyricHeader.Text = "Lyrics Opened - " + Path.GetFileNameWithoutExtension(information);
                txtLyricHeader.IsTextSelectionEnabled = true;
                txtLyricHeader.HorizontalAlignment = HorizontalAlignment.Center;
                stkLyricOptions.HorizontalAlignment = HorizontalAlignment.Center;

                ToolTipService.SetToolTip(txtLyricHeader, information);
                btnOpenMedia.Visibility = Visibility.Visible;
            }
            base.OnNavigatedTo(e);
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
        private async void btnSaveSelectedLyricsToFile_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            if (mediacontroller.LyricModel is LrcTrack lyric)
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
        private void hypPathSaved_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(hypPathSaved.Content.ToString()))
            {
                Process.Start("explorer.exe", $"/select,\"{hypPathSaved.Content.ToString()}\"");
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

        public async Task LoadLyricsFromFileAsync(string filePath)
        {
            try
            {

                if (File.Exists(filePath))
                {
                    // Read all text asynchronously
                    string[] lines = await File.ReadAllLinesAsync(filePath);
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var recentMusics = currentSettings.RecentMusic;
                    var exist = recentMusics.FirstOrDefault(p => p.SongPath == PlayerService.CurrentPlayingPath);
                    if (exist != null)
                    {
                        exist.LastLyricPath = filePath;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
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


            //     StartPlayback();
        }
        private async void PlaybackStart(string filePath)
        {
            LyricsList.Clear();
            lyrictimer = new DispatcherTimer();
            // Tick frequently enough for smooth sub-second line changes (e.g., every 100ms)
            lyrictimer.Interval = TimeSpan.FromMilliseconds(100);
            await LoadLyricsFromFileAsync(filePath);
            lyrictimer.Tick += Lyrictimer_Tick;
            lyrictimer.Start();
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
        private void ViewFullLyrics()
        {

            txtLyricHeader.Text = "Full Lyrics";
            btnCloseFullLyrics.Visibility = Visibility.Visible;
            stkLyricOptions.Visibility = Visibility.Visible;
            txtLyricsFull.Blocks.Clear();

            foreach (var line in LyricsList)
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
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ViewFullLyrics();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Find Lyrics Online", "Load Lyrics", "", "Cancel", OceanDialogWindow.ContentType.LyricSearchOnline, OceanContentDialogDefault.Primary, XamlRoot, 900, 960, OceanContentDialogType.Elevated, App.MainWindowInstance, "appicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }
        public async Task<string> WriteToTemporaryFileAsync(string textContent, string trackname)
        {
            try
            {
                // 1. Get the app's temporary folder

                // 2. Create a temporary file with a unique name
                // Generate a random filename or use a specific prefix
                string fileName = $"temp_lyric_{trackname}_{Guid.NewGuid()}.txt";
                string output = Path.Combine(Path.GetTempPath(), fileName);

                //         var storagefile = await StorageFile.GetFileFromPathAsync(output);
                // 3. Write the text content to the file
                await File.WriteAllTextAsync(output, textContent);

                // 4. Return the full path of the created file
                return output;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., I/O errors)
                System.Diagnostics.Debug.WriteLine($"Failed to write temp file: {ex.Message}");
                return "";
            }
        }
        private async void OceanContentDialog_PrimaryRequested()
        {
            var lyrictrack = mediacontroller.LyricModel;
            Debug.WriteLine(lyrictrack.SyncedLyrics);
            var filepath = await WriteToTemporaryFileAsync(lyrictrack.SyncedLyrics, lyrictrack.TrackName);
            if (File.Exists(filepath))
            {
                Process.Start("explorer.exe", $"/select,\"{filepath}\"");
            }
            PlaybackStart(filepath);
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CopyToClipboard.CopyStringToClipboard(txtLyricRealTime.Text);
        }
        private void CloseLyrics()
        {
            txtLyricHeader.Text = "";
            btnCloseFullLyrics.Visibility = Visibility.Collapsed;
            stkLyricOptions.Visibility = Visibility.Collapsed;
            txtLyricsFull.Blocks.Clear();
        }
        private void btnCloseFullLyrics_Click(object sender, RoutedEventArgs e)
        {
            CloseLyrics();
        }

        private async void mnftOpenPrevLyrics_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.CurrentPlayingPath == "") return;
            else
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var recentMusics = currentSettings.RecentMusic;
                var exist = recentMusics.FirstOrDefault(p => p.SongPath == PlayerService.CurrentPlayingPath);
                if (exist != null)
                {
                    if (File.Exists(exist.LastLyricPath))
                    {
                        PlaybackStart(exist.LastLyricPath);
                    }
                }
            }
        }

        private async void mnftOpenFromPC_Click(object sender, RoutedEventArgs e)
        {
            txtLyricRealTime.Text = "";

            if (App.MainWindowInstance == null) return;
            var lyricfile = await LyricPicker.PickSingle(App.MainWindowInstance, "Open Lyric File");
            if (lyricfile != null)
            {
                PlaybackStart(lyricfile.Path);
                if (stkLyricOptions.Visibility == Visibility.Visible)
                {
                    ViewFullLyrics();
                }
            }
        }
        private bool _isSyncingSizes = false;
        private void FontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncingSizes || FontSizeCombo.SelectedItem == null) return;

            _isSyncingSizes = true;
            FontSizeNumber.Value = (double)FontSizeCombo.SelectedItem;
            _isSyncingSizes = false;
        }
        private void UpdateLyricStyle()
        {
            // Safety check: Ensure the lyric text control exists before trying to modify it
            if (txtLyricRealTime == null) return;

            // 1. Update Font Family
            if (FontFamilyCombo?.SelectedItem is string selectedFont)
            {
                txtLyricRealTime.FontFamily = new FontFamily(selectedFont);
            }

            // 2. Update Font Size (Pulling directly from the central NumberBox value)
            if (FontSizeNumber != null && !double.IsNaN(FontSizeNumber.Value))
            {
                txtLyricRealTime.FontSize = FontSizeNumber.Value;
            }

            // 3. Update Font Color
            if (FontColorPicker != null)
            {
                txtLyricRealTime.Foreground = new SolidColorBrush(FontColorPicker.Color);
            }
        }
        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLyricStyle();
        }
        private void FontSizeNumber_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (_isSyncingSizes || double.IsNaN(sender.Value)) return;

            _isSyncingSizes = true;
            // Select the item if it matches an existing entry in the ComboBox list, otherwise clear selection
            FontSizeCombo.SelectedItem = FontSizeCombo.Items.Cast<double>().Cast<double?>().FirstOrDefault(i => i == sender.Value);
            _isSyncingSizes = false;
            UpdateLyricStyle();
        }
        private void btnCustomizeLyricText_Click(object sender, RoutedEventArgs e)
        {
            if (FontFamilyCombo.ItemsSource == null)
            {
                // Using Win2D to extract system font names
                var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies()
                                    .OrderBy(f => f)
                                    .ToList();

                FontFamilyCombo.ItemsSource = systemFonts;

                // Fallback selection to Segoe UI if available
                var defaultFont = systemFonts.FirstOrDefault(f => f.Equals("Segoe UI Variable Text", StringComparison.OrdinalIgnoreCase))
                                  ?? systemFonts.FirstOrDefault(f => f.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
                                  ?? systemFonts.FirstOrDefault();

                FontFamilyCombo.SelectedItem = defaultFont;
            }

            // 2. Open the Teaching Tip
            FontSettingsTip.IsOpen = true;
        }



        private void FontColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            UpdateLyricStyle();
        }

        private void btnResetToDefault_Click(object sender, RoutedEventArgs e)
        {
            FontSizeNumber.Value = 36;
            var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies()
                    .OrderBy(f => f)
                    .ToList();

            var defaultFont = systemFonts.FirstOrDefault(f => f.Equals("Segoe UI Variable Text", StringComparison.OrdinalIgnoreCase))
                              ?? systemFonts.FirstOrDefault(f => f.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
                              ?? systemFonts.FirstOrDefault();

            FontFamilyCombo.SelectedItem = defaultFont;
            // Programmatically set the ColorPicker to White
            FontColorPicker.Color = Microsoft.UI.Colors.White;
        }
        public bool IsFileNotEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Exists && fileInfo.Length > 0;
        }
        private async void MenuFlyout_Opened(object sender, object e)
        {
            if (PlayerService.CurrentPlayingPath == "") return;
            else
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var recentMusics = currentSettings.RecentMusic;
                var exist = recentMusics.FirstOrDefault(p => p.SongPath == PlayerService.CurrentPlayingPath);
                if (exist != null)
                {
                    if (File.Exists(exist.LastLyricPath))
                    {
                        if (IsFileNotEmpty(exist.LastLyricPath))
                        {
                            mnftOpenPrevLyrics.IsEnabled = true;
                        }
                        else
                        {
                            mnftOpenPrevLyrics.IsEnabled = false;
                        }
                    }
                    else
                    {
                        mnftOpenPrevLyrics.IsEnabled = false;
                    }

                }
                else
                {
                    mnftOpenPrevLyrics.IsEnabled = false;
                }
            }
        }
    }
}
