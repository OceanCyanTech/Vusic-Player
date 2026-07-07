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
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class OnlineArtistPicture : UserControl
    {
        public OnlineArtistPicture()
        {
            InitializeComponent();
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.Timeout = TimeSpan.FromSeconds(30);
            ImageGrid.ItemsSource = OnlineImages;
        }
        public ObservableCollection<string> OnlineImages { get; set; } = new ObservableCollection<string>();
        public void UpdateArtistName(string artist)
        {
            txtArtistToFind.Text = artist;
        }
        private int currentImageOffset = 10;
        private async void btnShowMoreResults_Click(object sender, RoutedEventArgs e)
        {
            btnShowMoreResults.IsEnabled = false;


            // 1. Fetch more images (starting from where we left off)
            currentImageOffset += 10; // DDG usually serves 30-100 per page
            if (currentImageOffset == 30)
            {
                btnShowMoreResults.IsEnabled = false;
                return;
            }
            FindOnline(currentImageOffset);
            StartCooldown();
        }
        public string GetSelectedImage()
        {
            if (ImageGrid.SelectedItem is string imageUrl)
            {
                return imageUrl;
            }

            // The compiler needs this "Plan B"
            return string.Empty;
        }
        string searchedresult = "";
        private DispatcherTimer? cooldownTimer;
        private DispatcherTimer? cooldownTimerFind;
        private void StartCooldown()
        {
            int remaining = 10;
            btnShowMoreResults.IsEnabled = false;

            cooldownTimer = new DispatcherTimer();
            cooldownTimer.Interval = TimeSpan.FromSeconds(1);
            cooldownTimer.Tick += (s, e) =>
            {
                remaining--;
                btnShowMoreResults.Content = $"Show more results (Wait {remaining}s...)";

                if (remaining <= 0)
                {
                    cooldownTimer.Stop();
                    btnShowMoreResults.IsEnabled = true;
                    btnShowMoreResults.Content = "Show more results";
                }
            };
            cooldownTimer.Start();
        }
        HttpClient client;
        public async Task<string?> DownloadImageAsync(string imageUrl)
        {
            try
            {

                txtWaitOnline.Visibility = Visibility.Visible;
                txtWaitOnline.Text = "Downloading image...";
                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

                // 📁 Save location (inside app folder)
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "VusicImages");

                Directory.CreateDirectory(folder);
                string extension = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
                if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                // 🖼 Unique filename
                string safeName = txtArtistToFind.Text.Replace(" ", "_");
                string fileName = $"{safeName}_{DateTime.Now.ToString("dd-MM-yy")}{extension}";
                string filePath = Path.Combine(folder, fileName);
                Debug.WriteLine(filePath);
                await File.WriteAllBytesAsync(filePath, imageBytes);
                Filepath = filePath;
                CheckInternet.UrlToDownload = Filepath;
                txtWaitOnline.Text = "Image saved successfully!";
                return filePath;
            }
            catch (Exception ex)
            {
                ifbNoInternet.Title = "Error";
                ifbMessage.Text = "An unexpected error occured. Check for log details in Log Page.";
                ifbActionButton.Content = "See Log";
                ifbNoInternet.IsOpen = true;
                ifbNoInternet.Severity = InfoBarSeverity.Error;
                Logger.Log(ex.Message, "ArtistFindImageOnline", Logger.LogLevelType.Error);
                ifbActionButton.Click += ActionButton_Click1;
                ifbActionButton.Visibility = Visibility.Visible;
                return null;
            }
            finally
            {
                CheckInternet.UrlToDownload = Filepath;
                CheckInternet.CallImageSet();
                txtWaitOnline.Visibility = Visibility.Collapsed;
                OceanContentDialog.HideDlg();
            }
        }
        string Filepath = "";
        public async Task<ObservableCollection<string>> FindImagesOfArtist(string query, int countbreak)
        {
            string encodedQuery = Uri.EscapeDataString(query);
            var results = OnlineImages;

            try
            {
                // Step 1: Get the VQD token from the main search page
                // We must mimic a real browser with a User-Agent
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                // This one is often the "magic" fix for DDG
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                var mainPageResponse = await client.GetStringAsync($"https://duckduckgo.com/?q={encodedQuery}");
                Debug.WriteLine("mainres");
                // Use Regex to grab the vqd token
                // Use this updated Regex pattern
                var vqdMatch = Regex.Match(mainPageResponse, @"vqd=['""]([^' ""]+)['""]|vqd=([^&'""]+)"); if (!vqdMatch.Success) return results;

                string vqd = vqdMatch.Groups[1].Value;
                if (string.IsNullOrEmpty(vqd)) vqd = vqdMatch.Groups[2].Value; // Try the second group

                Debug.WriteLine($"VQD Token Found: {vqd}");
                Debug.WriteLine("mainres2");

                if (string.IsNullOrEmpty(vqd))
                {
                    Debug.WriteLine("FAILED: Could not find VQD token in HTML.");
                    return results;
                }
                // Step 2: Call the internal JSON API
                // o=json is required, and we pass the vqd token
                client.DefaultRequestHeaders.Referrer = new Uri($"https://duckduckgo.com/?q={encodedQuery}");

                // The 'f' parameter defines filters; ',,,' is the default (all sizes/types/colors)
                // Adding &kp=1 ensures explicit content is blocked
                string apiUrl = $"https://duckduckgo.com/i.js?l=us-en&o=json&q={encodedQuery}&vqd={vqd}&f=,,,&kp=1";
                Debug.WriteLine($"FETCHING URL: {apiUrl}");
                Debug.WriteLine("Requesting Image JSON...");
                var jsonResponse = await client.GetStringAsync(apiUrl);
                Debug.WriteLine("JSON Received!");

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);

                if (doc.RootElement.TryGetProperty("results", out JsonElement images))
                {
                    foreach (var item in images.EnumerateArray())
                    {
                        string finalUrl = "";

                        // DDG uses 'image' for the original high-res link
                        if (item.TryGetProperty("image", out JsonElement imgElement))
                        {
                            string originalUrl = imgElement.GetString()!;

                            if (!string.IsNullOrEmpty(originalUrl) &&
                                !originalUrl.Contains("lookaside") &&
                                !originalUrl.Contains("tiktok.com"))
                            {
                                finalUrl = originalUrl;
                            }
                        }

                        // Fallback to 'thumbnail' if original is filtered or missing
                        if (string.IsNullOrEmpty(finalUrl) && item.TryGetProperty("thumbnail", out JsonElement thumbElement))
                        {
                            finalUrl = thumbElement.GetString()!;
                        }

                        if (!string.IsNullOrEmpty(finalUrl))
                        {
                            if (finalUrl.StartsWith("http://"))
                                finalUrl = finalUrl.Replace("http://", "https://");
                            results.Add(finalUrl);
                        }

                        if (results.Count >= countbreak) break; // Upped to 10 since DDG is free!
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "ArtistPage.FindImageOnline_DDG", Logger.LogLevelType.Error);
            }

            return results;
        }
        private CancellationTokenSource? _loadingCts;
        private async void FindOnline(int countbreak)
        {
            Debug.WriteLine("Find onlnie initiated");
            ifbNoInternet.IsOpen = false;
            txtWaitOnline.Visibility = Visibility.Visible;
            string query = txtArtistToFind.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                txtWaitOnline.Text = "The search box is empty.";
                return;
            }

            if (!CheckInternet.IsInternetAvailable())
            {
                ifbNoInternet.Title = "You're not connected to the internet";
                ifbNoInternet.Severity = InfoBarSeverity.Error;
                ifbActionButton.Visibility = Visibility.Visible;
                ifbNoInternet.IsOpen = true;
                ifbMessage.Text = "Connect to the internet to search online";
                ifbActionButton.Content = "Connect to internet in Settings";
                ifbActionButton.Click -= ActionButton_Click;
                ifbActionButton.Click += ActionButton_Click;
                return;
            }
            try
            {

                prgWaitOnline.Visibility = Visibility.Visible;

                _ = AnimateStatusAsync("Finding images");
                OnlineImages.Clear();
                var urls = await FindImagesOfArtist(query, countbreak);

                // 2. Update the UI (Resumes on UI thread automatically)
                if (urls != null && urls.Count > 0)
                {
                    // Setting the ItemsSource directly is fine, 
                    // but clearing and re-assigning ensures the UI refreshes properly
                    ImageGrid.ItemsSource = null;
                    ImageGrid.ItemsSource = OnlineImages;

                    txtWaitOnline.Visibility = Visibility.Collapsed;
                    prgWaitOnline.Visibility = Visibility.Collapsed;
                    int remaining = 10;
                    btnFindOnline.IsEnabled = false;

                    cooldownTimerFind = new DispatcherTimer();
                    cooldownTimerFind.Interval = TimeSpan.FromSeconds(1);
                    cooldownTimerFind.Tick += (s, e) =>
                    {
                        remaining--;
                        txtFindOnlineButton.Text = $"Find (Wait {remaining}s...)";

                        if (remaining <= 0)
                        {
                            cooldownTimerFind.Stop();
                            btnFindOnline.IsEnabled = true;
                            txtFindOnlineButton.Text = "Find";
                        }
                    };
                    cooldownTimerFind.Start();
                }
                else
                {
                    txtWaitOnline.Text = "No images found.";
                    prgWaitOnline.Visibility = Visibility.Collapsed;
                }

                _loadingCts?.Cancel();
            }

            catch (Exception ex)
            {
                _loadingCts?.Cancel();

                ifbNoInternet.Title = "Error";
                ifbActionButton.Visibility = Visibility.Visible;
                ifbMessage.Text = "An unexpected error occured. Check for log details in Log Page.";
                ifbActionButton.Content = "See Log";
                ifbNoInternet.IsOpen = true;
                ifbNoInternet.Severity = InfoBarSeverity.Error;
                ifbActionButton.Click += ActionButton_Click1;
                Debug.WriteLine(ex.Message);
                Logger.Log(ex.Message, "ArtistPage.FindImageOnline", Logger.LogLevelType.Error);
            }
            finally
            {
                //  prgWaitOnline.Visibility = Visibility.Collapsed;
                // txtWaitOnline.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnFindOnline_Click(object sender, RoutedEventArgs e)
        {
            if (searchedresult == txtArtistToFind.Text.ToLower())
            {
                return;
            }
            else
            {
                searchedresult = txtArtistToFind.Text.ToLower();
                btnShowMoreResults.IsEnabled = true;
            }
            FindOnline(10);
        }
        private async Task AnimateStatusAsync(string baseText)
        {
            _loadingCts = new CancellationTokenSource();
            var token = _loadingCts.Token;

            int dots = 0;

            while (!token.IsCancellationRequested)
            {
                dots = (dots % 3) + 1;
                txtWaitOnline.Text = baseText + new string('.', dots);

                await Task.Delay(400);
            }
        }

        private void ActionButton_Click1(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
                App.NavigationFrame.Navigate(typeof(LoggerPage));
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:network",
                UseShellExecute = true
            });
        }


        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindOnline_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image img)
            {
                // Log the error to your logger
                Debug.WriteLine($"Image load failed: {e.ErrorMessage}");

                // Option: Set a local fallback image so it's not just a gray box
                img.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png"));
            }
        }
    }

}
