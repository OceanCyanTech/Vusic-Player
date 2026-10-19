using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public class CountToMediaFilesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} media file added" : $"• {count} media files added";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class CountToMediaFilesProcessedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} media file processed" : $"• {count} media files processed";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class FormatConverter : Page
    {

        ObservableCollection<FileFormatConverterModel> AddedFiles = new ObservableCollection<FileFormatConverterModel>();
        ObservableCollection<FileFormatConverterModel> ProcessedFiles = new ObservableCollection<FileFormatConverterModel>();
        public FormatConverter()
        {
            InitializeComponent();
        }

        private async void btnAddAudioFiles_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var files = await FilePickers.MediaPicker.PickMultipleMediaFilesAsync(App.MainWindowInstance, "Add Audio Files");
            foreach (var file in files)
            {
                try
                {
                    if (Extensions.AudioExtensions.List.Contains(Path.GetExtension(file.Path)))
                    {
                        var newExtensions = new List<string>();
                        foreach (var ext in AudioExtensions.List)
                        {
                            if (ext != Path.GetExtension(file.Path))
                            {
                                newExtensions.Add(ext);
                            }
                        }

                        var audioprops = await file.Properties.GetMusicPropertiesAsync();
                        AddedFiles.Add(new FileFormatConverterModel { FilePath = file.Path, FromFormat = Path.GetExtension(file.Path).ToLower(), FileName = Path.GetFileNameWithoutExtension(file.Path), Duration = audioprops.Duration, AudioExtensionsList = new List<string>(newExtensions) });
                    }
                    else
                    {
                        var newExtensions = new List<string>();
                        foreach (var ext in AudioExtensions.List)
                        {
                            if (ext != Path.GetExtension(file.Path))
                            {
                                newExtensions.Add(ext);
                            }
                        }
                        foreach (var ext in VideoExtensions.List)
                        {
                            if (ext != Path.GetExtension(file.Path))
                            {
                                newExtensions.Add(ext);
                            }
                        }
                        var videoprops = await file.Properties.GetVideoPropertiesAsync();
                        AddedFiles.Add(new FileFormatConverterModel { Glyph = "\uE8B2", FromFormat = Path.GetExtension(file.Path).ToLower(), FilePath = file.Path, FileName = Path.GetFileNameWithoutExtension(file.Path), Duration = videoprops.Duration, AudioExtensionsList = new List<string>(newExtensions) });
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private async void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FileFormatConverterModel fileFormatConverterModel)
            {
                Interlocked.Increment(ref _activeConversionsCount);
                btnConvertAll.IsEnabled = false;
                fileFormatConverterModel.IsEnabled = false;

                try
                {
                    var baseDirOutput = fileFormatConverterModel.OutputPath;
                    if (string.IsNullOrWhiteSpace(baseDirOutput))
                    {
                        baseDirOutput = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    }

                    string destinationPath = Path.Combine(
                        baseDirOutput,
                        $"{fileFormatConverterModel.FileName}_{Guid.NewGuid()}{fileFormatConverterModel.ToFormat}"
                    );

                    var output = await ConvertSingleFile(fileFormatConverterModel.FilePath, fileFormatConverterModel.ToFormat, destinationPath);
                    fileFormatConverterModel.Info = output;
                }
                finally
                {
                    Interlocked.Decrement(ref _activeConversionsCount);
                    UpdateConvertAllButtonState();
                }
            }
        }

        private async void btnConvertAll_Click(object sender, RoutedEventArgs e)
        {
            var pendingFiles = AddedFiles.Where(f => f.Progress < 100).ToList();
            if (pendingFiles.Count == 0) return;

            btnConvertAll.IsEnabled = false;
            btnAddAudioFiles.IsEnabled = false;

            try
            {
                var tasks = pendingFiles.Select(async item =>
                {
                    Interlocked.Increment(ref _activeConversionsCount);
                    item.IsEnabled = false;

                    try
                    {
                        var baseDirOutput = item.OutputPath;
                        if (string.IsNullOrWhiteSpace(baseDirOutput))
                        {
                            baseDirOutput = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                        }

                        string destinationPath = Path.Combine(
                            baseDirOutput,
                            $"{item.FileName}_{Guid.NewGuid()}{item.ToFormat}"
                        );

                        string result = await ConvertSingleFile(item.FilePath, item.ToFormat, destinationPath);
                        item.Info = result;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeConversionsCount);
                    }
                });

                await Task.WhenAll(tasks);
            }
            finally
            {
                btnAddAudioFiles.IsEnabled = true;
                UpdateConvertAllButtonState();
            }
        }
        private readonly SemaphoreSlim _throttler = new SemaphoreSlim(Environment.ProcessorCount);
        private int _activeConversionsCount = 0;

        private void UpdateConvertAllButtonState()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                btnConvertAll.IsEnabled = _activeConversionsCount == 0 && AddedFiles.Count > 0;
            });
        }
        private void asbSearchFormats_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.DataContext is FileFormatConverterModel model)
                {
                    string query = sender.Text?.Trim() ?? "";

                    // Filter the extensions list associated with this item
                    var filtered = model.AudioExtensionsList.Where(ext =>
                        ext.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        ext.TrimStart('.').Contains(query, StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    sender.ItemsSource = filtered;
                }
            }
        }

        private void asbSearchFormats_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is string chosenFormat && sender.DataContext is FileFormatConverterModel model)
            {
                ApplySelectedFormat(sender, model, chosenFormat);
            }
        }

        private void asbSearchFormats_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.DataContext is FileFormatConverterModel model)
            {
                string chosenFormat = args.ChosenSuggestion as string;

                if (string.IsNullOrWhiteSpace(chosenFormat) && !string.IsNullOrWhiteSpace(args.QueryText))
                {
                    // Normalize entered text e.g. "mp3" -> ".mp3"
                    string query = args.QueryText.Trim();
                    if (!query.StartsWith(".")) query = "." + query;

                    // Match if it exists in the extension list
                    chosenFormat = model.AudioExtensionsList.FirstOrDefault(ext =>
                        string.Equals(ext, query, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(chosenFormat))
                {
                    ApplySelectedFormat(sender, model, chosenFormat);
                }
            }
        }

        private void ApplySelectedFormat(AutoSuggestBox sender, FileFormatConverterModel model, string format)
        {
            // 1. Update the bound model property (which updates the ComboBox via TwoWay binding)
            model.ToFormat = format;

            // 2. Clear search text for the next time it opens
            sender.Text = string.Empty;

            // 3. Automatically close the flyout
            var flyoutPresenter = VisualTreeHelper.GetParent(sender);
            while (flyoutPresenter != null && !(flyoutPresenter is FlyoutPresenter))
            {
                flyoutPresenter = VisualTreeHelper.GetParent(flyoutPresenter);
            }

            if (flyoutPresenter is FlyoutPresenter fp && fp.Parent is Popup popup)
            {
                popup.IsOpen = false;
            }
        }
        private async Task<string> ConvertSingleFile(string inputpath, string toformat, string outputpath)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            string baseDir = AppContext.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");

            var item = AddedFiles.FirstOrDefault(p => p.FilePath == inputpath);
            if (item == null) return "ERROR: Unable to find file in list";
            if (!File.Exists(inputpath)) return $"ERROR: Input file does not exist: {inputpath}";

            string targetExt = Path.GetExtension(outputpath).ToLowerInvariant();
            bool isAudioTarget = AudioExtensions.List.Any(ext =>
                string.Equals(ext.TrimStart('.'), targetExt.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
            bool isVideoTarget = VideoExtensions.List.Any(ext =>
                string.Equals(ext.TrimStart('.'), targetExt.TrimStart('.'), StringComparison.OrdinalIgnoreCase));

            await _throttler.WaitAsync();
            try
            {
                // 1. Audio destination or pure re-encode targets
                if (isAudioTarget)
                {
                    string audioFlags = "-vn";
                    if (targetExt == ".wav") audioFlags += " -rf64 auto";
                    else if (targetExt == ".mp3" || targetExt == ".m4a" || targetExt == ".aac") audioFlags += " -b:a 320k";
                    else if (targetExt == ".flac") audioFlags += " -compression_level 5";

                    string pipeline = $"\"{ffmpegPath}\" -y -i \"{inputpath}\" {audioFlags} \"{outputpath}\"";
                    return await ExecuteFFmpegProcessAsync(pipeline, baseDir, item, dispatcherQueue, outputpath);
                }

                // 2. Video destination: Attempt instant stream copy first
                if (isVideoTarget)
                {
                    // Try fast remux (-c copy). Include -movflags +faststart for MP4 web compatibility
                    string fastCopyArgs = targetExt == ".mp4" ? "-c copy -movflags +faststart" : "-c copy";
                    string fastPipeline = $"\"{ffmpegPath}\" -y -i \"{inputpath}\" {fastCopyArgs} \"{outputpath}\"";

                    string fastResult = await ExecuteFFmpegProcessAsync(fastPipeline, baseDir, item, dispatcherQueue, outputpath, isFallbackAttempt: false);

                    if (fastResult == "Success")
                    {
                        return "Success";
                    }

                    // Fast remux failed (codecs incompatible with target container) -> Fallback to re-encoding
                    System.Diagnostics.Debug.WriteLine($"[{item.FileName}] Direct copy failed. Falling back to libx264 re-encode.");

                    // Reset progress for second pass
                    dispatcherQueue.TryEnqueue(() => item.Progress = 0);

                    string reencodePipeline = $"\"{ffmpegPath}\" -y -i \"{inputpath}\" -c:v libx264 -crf 18 -preset fast -c:a aac -b:a 320k \"{outputpath}\"";
                    return await ExecuteFFmpegProcessAsync(reencodePipeline, baseDir, item, dispatcherQueue, outputpath, isFallbackAttempt: true);
                }

                // Default fallback for any other uncategorized format
                string genericPipeline = $"\"{ffmpegPath}\" -y -i \"{inputpath}\" \"{outputpath}\"";
                return await ExecuteFFmpegProcessAsync(genericPipeline, baseDir, item, dispatcherQueue, outputpath);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
            finally
            {
                _throttler.Release();
            }
        }

        private async Task<string> ExecuteFFmpegProcessAsync(
            string commandPipeline,
            string baseDir,
            FileFormatConverterModel item,
            Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue,
            string outputpath,
            bool isFallbackAttempt = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{commandPipeline}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                WorkingDirectory = baseDir
            };

            using var cts = new CancellationTokenSource();

            var progressTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(50, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    dispatcherQueue.TryEnqueue(() =>
                    {
                        if (item.Progress < 95)
                        {
                            item.Progress += 0.95;
                        }
                    });
                }
            });

            var errorBuilder = new System.Text.StringBuilder();

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                        System.Diagnostics.Debug.WriteLine($"[{item.FileName}] {e.Data}");
                    }
                };

                process.Start();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                int exitCode = process.ExitCode;
                cts.Cancel();

                // If fast copy failed and we still have a fallback attempt left, return error without updating UI to failed state
                if (exitCode != 0 && !isFallbackAttempt)
                {
                    return $"FAILED_ATTEMPT: Code {exitCode}";
                }

                dispatcherQueue.TryEnqueue(() =>
                {
                    if (exitCode == 0)
                    {
                        item.Progress = 100;
                        item.ImageState = "ms-appx:///Assets/success.png";
                        item.OutputPath = outputpath;
                        ProcessedFiles.Add(item);
                    }
                    else
                    {
                        item.ImageState = IconHolder.Instance.Error;
                        System.Diagnostics.Debug.WriteLine($"Failed with code {exitCode}: {errorBuilder}");
                    }

                    item.IsEnabled = true;
                });

                return exitCode == 0 ? "Success" : $"ERROR: Code {exitCode}";
            }
        }
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FileFormatConverterModel fileFormatConverterModel)
            {
                OpenFileLocation.PathSelect(fileFormatConverterModel.OutputPath);
            }
        }
        private void mnftOpenFileLocOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                OpenFileLocation.PathSelect(file.OutputPath);
            }
        }

        private void mnftOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                OpenFileLocation.PathSelect(file.FilePath);
            }
        }

        private void mnftCopyFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                CopyToClipboard.CopyStringToClipboard(file.FilePath);
            }
        }

        private void mnftFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(file.FilePath);
                }
            }
        }

        private void mnftCopyReverbOutputFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                CopyToClipboard.CopyStringToClipboard(file.OutputPath);
            }
        }

        private void mnftFileInfoReverbOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(file.OutputPath);
                }
            }
        }
        private async void btnCustomOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FileFormatConverterModel formatter)
            {
                if (App.MainWindowInstance == null) return;

                var folderpick = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose custom output location", Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary);

                if (folderpick != null)
                {
                    formatter.OutputPath = folderpick.Path;
                }
            }
        }
        private string BuildFFmpegArguments(string inputPath, string outputPath)
        {
            string targetExt = Path.GetExtension(outputPath);

            // Case-insensitive check against your existing format collections
            bool isAudioTarget = AudioExtensions.List.Any(ext =>
                string.Equals(ext.TrimStart('.'), targetExt.TrimStart('.'), StringComparison.OrdinalIgnoreCase));

            bool isVideoTarget = VideoExtensions.List.Any(ext =>
                string.Equals(ext.TrimStart('.'), targetExt.TrimStart('.'), StringComparison.OrdinalIgnoreCase));

            var flags = new List<string>
    {
        "-y",                   // Overwrite destination without blocking background threads
        $"-i \"{inputPath}\""   // Input file path
    };

            if (isAudioTarget)
            {
                // 1. Strip video stream completely to avoid muxing failures into audio containers
                flags.Add("-vn");

                // 2. Specific container guards
                string normalizedExt = targetExt.ToLowerInvariant();
                switch (normalizedExt)
                {
                    case ".wav":
                        // Switches to RF64 64-bit headers if file exceeds 4GB, preventing 1.5h cutoff
                        flags.Add("-rf64 auto");
                        break;

                    case ".mp3":
                    case ".m4a":
                    case ".aac":
                        // Max bitrate to preserve transparency
                        flags.Add("-b:a 320k");
                        break;

                    case ".flac":
                        flags.Add("-compression_level 5");
                        break;
                }
            }
            else if (isVideoTarget)
            {
                // Visually lossless conversion with universal x264/aac encoding
                flags.Add("-c:v libx264 -crf 18 -preset fast -c:a aac -b:a 320k");
            }

            // Output target path
            flags.Add($"\"{outputPath}\"");

            return string.Join(" ", flags);
        }
        private void cmbToFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox btn && btn.DataContext is FileFormatConverterModel formatter)
            {
                Debug.WriteLine("TEST1");
                formatter.ToFormat = btn.SelectedItem.ToString() ?? ".wav";
            }
        }

        private async void mnftAddOutputToQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                if (File.Exists(file.OutputPath))
                {
                    var stfile = await StorageFile.GetFileFromPathAsync(file.OutputPath);
                    var musicprops = await stfile.Properties.GetMusicPropertiesAsync();
                    QueueService.VusicQueue.Add(new SongModel { FilePath = file.OutputPath, Title = Path.GetFileNameWithoutExtension(file.OutputPath), SongDuration = musicprops.Duration });
                    QueueService.VusicQueueNext.Add(new SongModel { FilePath = file.OutputPath, SongDuration = musicprops.Duration, Title = Path.GetFileNameWithoutExtension(file.OutputPath) });
                }
            }

        }

        private void mnftRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                AddedFiles.Remove(file);
                var exist = ProcessedFiles.FirstOrDefault(p => p.FilePath == file.FilePath);
                if (exist != null)
                {
                    ProcessedFiles.Remove(exist);
                }
            }

        }
        private async void PlaySelection(FileFormatConverterModel selectedSong)
        {
            if (selectedSong.OutputPath != null)
            {
                if (File.Exists(selectedSong.OutputPath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(selectedSong.OutputPath);
                    string fileExtension = file.FileType.ToLowerInvariant();
                    bool isVideo = false;
                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {
                        isVideo = true;
                    }
                    if (isVideo == false)
                    {
                        try
                        {
                            ObservableCollection<SongModel> single = new();
                            var storagefile = await StorageFile.GetFileFromPathAsync(selectedSong.OutputPath);
                            var musicprops = await storagefile.Properties.GetMusicPropertiesAsync();
                            string Title = Path.GetFileNameWithoutExtension(selectedSong.OutputPath);
                            single.Add(new SongModel { FilePath = selectedSong.OutputPath, Title = Title, SongDuration = musicprops.Duration, Artist = musicprops.Artist, AlbumName = musicprops.Album });
                            QueueService.PlayMedia(single, false, false);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Error Occured: " + ex.Message, "ListViewMedia.PlaySelection", Logger.LogLevelType.Error);
                        }
                    }
                    else
                    {
                        if (App.NavigationFrame == null) return;
                        if (PlayerService.InVideoPage == false)
                        {
                            App.NavigationFrame.Navigate(typeof(VideoPlayer), selectedSong.OutputPath);
                        }
                        else
                        {
                            PlayerService.OpenPath(selectedSong.OutputPath);
                        }
                    }
                }
            }
        }


        private void mnftPlayOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileFormatConverterModel file)
            {
                PlaySelection(file);
            }
        }


    }
}
