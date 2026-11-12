using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
using Vusic_Player.FilePickers;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PitchExport : Page
    {
        ObservableCollection<FilesToModifyAdded> AddedFiles = new ObservableCollection<FilesToModifyAdded>();
        ObservableCollection<FilesToModifyAdded> ProcessedFiles = new ObservableCollection<FilesToModifyAdded>();
        string OutputDirectoryCommon = "";

        public PitchExport()
        {
            InitializeComponent();
        }
        private async void btnAddAudioFiles_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Add Audio Files");
            foreach (var file in files)
            {
                AddedFiles.Add(new FilesToModifyAdded { FilePath = file.Path, Title = Path.GetFileNameWithoutExtension(file.Path) });
            }
        }
        private async void btnPlayAllOutput_Click(object sender, RoutedEventArgs e)
        {
            var tempobservable = new ObservableCollection<SongModel>();
            foreach (var item in ProcessedFiles)
            {
                if (File.Exists(item.OutputPath))
                {
                    try
                    {
                        var storagefile = await StorageFile.GetFileFromPathAsync(item.OutputPath);
                        var musicprops = await storagefile.Properties.GetMusicPropertiesAsync();
                        tempobservable.Add(new SongModel { Title = Path.GetFileNameWithoutExtension(item.OutputPath), FilePath = item.OutputPath, SongDuration = musicprops.Duration });
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            QueueService.PlayMedia(tempobservable, false, false);
        }
        private async void btnAddtoQueueAllOutput_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ProcessedFiles)
            {
                if (!File.Exists(item.OutputPath)) return;

                var file = await StorageFile.GetFileFromPathAsync(item.OutputPath);
                var musicprops = await file.Properties.GetMusicPropertiesAsync();
                QueueService.VusicQueue.Add(new SongModel { Title = Path.GetFileNameWithoutExtension(item.OutputPath), FilePath = item.OutputPath, SongDuration = musicprops.Duration });
                QueueService.VusicQueueNext.Add(new SongModel { Title = Path.GetFileNameWithoutExtension(item.OutputPath), FilePath = item.OutputPath, SongDuration = musicprops.Duration });
            }
        }
        private async void btnExportDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var folder = await FolderPickerFunct.PickFolder(App.MainWindowInstance, "Pick Export Directory", PickerLocationId.PicturesLibrary);
            if (folder != null)
            {
                string folderPath = folder.Path;
                OutputDirectoryCommon = folderPath;
                txtOutputDirectory.Text = folderPath;
                ToolTipService.SetToolTip(btnExportDirectory, folderPath);
            }
        }
        private void chkCustomDirectories_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in AddedFiles.ToList())
            {
                item.IndividualDirectorySel = chkCustomDirectories.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
                item.IndividualDirectorySelBool = chkCustomDirectories.IsChecked ?? false;

            }
            btnExportDirectory.IsEnabled = !chkCustomDirectories.IsChecked ?? false;
            txtOutputDirectory.Text = chkCustomDirectories.IsChecked ?? false ? "Custom Directories" : OutputDirectoryCommon;
        }
        private void btnOpenFileLocationProcessed_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                OpenFileLocation.PathSelect(file.OutputPath);
            }
        }
        private void mnftPlayOriginal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                PlayerService.OpenPath(file.FilePath);
            }
        }

        private void mnftPlayOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                PlayerService.OpenPath(file.OutputPath);
            }
        }

        private async void mnftAddToQueueOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                if (!File.Exists(file.OutputPath)) return;
                var stfile = await StorageFile.GetFileFromPathAsync(file.OutputPath);
                var musicprops = await stfile.Properties.GetMusicPropertiesAsync();
                QueueService.VusicQueue.Add(new SongModel { FilePath = file.OutputPath, Title = Path.GetFileNameWithoutExtension(file.OutputPath), SongDuration = musicprops.Duration });
                QueueService.VusicQueueNext.Add(new SongModel { FilePath = file.OutputPath, SongDuration = musicprops.Duration, Title = Path.GetFileNameWithoutExtension(file.OutputPath) });
            }
        }
        private async void mnftChangeExportDir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                file.VisibilityOfCompletedFileLocation = Visibility.Collapsed;
                if (App.MainWindowInstance == null) return;
                var folder = await FolderPickerFunct.PickFolder(App.MainWindowInstance, "Pick Export Directory", PickerLocationId.PicturesLibrary);
                if (folder != null)
                {
                    string folderPath = folder.Path;

                    ToolTipService.SetToolTip(btnExportDirectory, folderPath);
                    file.DirectoryPath = folderPath;
                }
                file.ImageState = "";
                var exist = ProcessedFiles.FirstOrDefault(p => p.FilePath == file.FilePath);
                if (exist != null)
                {
                    ProcessedFiles.Remove(exist);
                }
            }
        }

        private void mnftOpenFileLocOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                OpenFileLocation.PathSelect(file.OutputPath);
            }
        }

        private void mnftOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                OpenFileLocation.PathSelect(file.FilePath);
            }
        }
        private void mnftFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(file.FilePath);
                }
            }
        }

        private void mnftCopyReverbOutputFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                CopyToClipboard.CopyStringToClipboard(file.OutputPath);
            }
        }

        private void mnftFileInfoReverbOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(file.OutputPath);
                }
            }
        }

        private async void btnSelectIndividualDir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FilesToModifyAdded file)
            {
                if (App.MainWindowInstance == null) return;
                var folder = await FolderPickerFunct.PickFolder(App.MainWindowInstance, "Pick Export Directory", PickerLocationId.PicturesLibrary);
                if (folder != null)
                {
                    string folderPath = folder.Path;

                    ToolTipService.SetToolTip(btnExportDirectory, folderPath);
                    file.DirectoryPath = folderPath;
                    file.ImageState = "";
                    file.Progress = 0;
                    file.VisibilityOfCompletedFileLocation = Visibility.Collapsed;
                    ToolTipService.SetToolTip(btn, folderPath);
                }
            }
        }

        private void mnftCopyFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                CopyToClipboard.CopyStringToClipboard(file.FilePath);
            }
        }

        private void mnftRemoveAudioFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                AddedFiles.Remove(file);
                var exist = ProcessedFiles.FirstOrDefault(p => p.FilePath == file.FilePath);
                if (exist != null)
                {
                    ProcessedFiles.Remove(exist);
                }
            }
        }

        private async void btnApplyPitch_Click(object sender, RoutedEventArgs e)
        {
            if (AddedFiles.Count == 0) return;

            if (string.IsNullOrWhiteSpace(numPitchValue.Text) || double.IsNaN(numPitchValue.Value))
            {
                numPitchValue.Value = 1;
            }

            btnAddAudioFiles.IsEnabled = false;
            mnftAddFiles.IsEnabled = false;
            btnApplyPitch.IsEnabled = false;
            btnExportDirectory.IsEnabled = false;
            stkOutputActions.Visibility = Visibility.Collapsed;

            foreach (var item in AddedFiles.ToList())
            {
                item.DirectorySelectionEnabled = false;
                item.VisibilityOfCompletedFileLocation = Visibility.Collapsed;
                item.ErrorToolTip = "";
                item.ImageState = "";
                item.Progress = 0;
            }

            ProcessedFiles.Clear();

            string baseDirOutput = txtOutputDirectory.Text;
            if (string.IsNullOrWhiteSpace(baseDirOutput))
            {
                baseDirOutput = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            string baseDir = AppContext.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");
            string ffmpegWorkingDir = Path.GetDirectoryName(ffmpegPath) ?? baseDir;

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);

            try
            {
                if (!File.Exists(ffmpegPath))
                {
                    Debug.WriteLine($"Executable missing: FFmpeg={File.Exists(ffmpegPath)}");
                    foreach (var item in AddedFiles)
                    {
                        item.ImageState = IconHolder.Instance.Error;
                        item.DirectorySelectionEnabled = true;
                        item.ErrorToolTip = "FFmpeg executable is missing.";
                    }
                    return;
                }

                // Capture pitch value on the UI thread using InvariantCulture (prevents decimal comma issues on non-US PCs)
                string pitchValueStr = numPitchValue.Value.ToString(CultureInfo.InvariantCulture);

                var tasks = AddedFiles.Select(async item =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        if (!File.Exists(item.FilePath))
                        {
                            Debug.WriteLine($"Input file does not exist: {item.FilePath}");
                            dispatcherQueue.TryEnqueue(() =>
                            {
                                item.ImageState = IconHolder.Instance.Error;
                                item.DirectorySelectionEnabled = true;
                                item.ErrorToolTip = "Input file does not exist on disk.";
                            });
                            return;
                        }

                        string sanitizedTitle = string.Join("_", item.Title.Split(Path.GetInvalidFileNameChars())).TrimEnd('.');
                        string outputMp3 = Path.Combine(baseDirOutput, $"{sanitizedTitle}_{Guid.NewGuid():N}.mp3");

                        if (item.IndividualDirectorySelBool)
                        {
                            if (string.IsNullOrWhiteSpace(item.DirectoryPath) || !Directory.Exists(item.DirectoryPath))
                            {
                                dispatcherQueue.TryEnqueue(() =>
                                {
                                    item.ImageState = IconHolder.Instance.Error;
                                    item.DirectorySelectionEnabled = true;
                                    item.ErrorToolTip = "Output directory not selected or does not exist.";
                                });
                                return;
                            }
                            outputMp3 = Path.Combine(item.DirectoryPath, $"{sanitizedTitle}_{Guid.NewGuid():N}.mp3");
                        }
                        else if (txtOutputDirectory.Text != "Custom Directories" && !Directory.Exists(baseDirOutput))
                        {
                            dispatcherQueue.TryEnqueue(() =>
                            {
                                item.ImageState = IconHolder.Instance.Error;
                                item.DirectorySelectionEnabled = true;
                                item.ErrorToolTip = "Output directory does not exist.";
                            });
                            return;
                        }

                        // Launch ffmpeg.exe directly with ArgumentList (no cmd.exe quote-escaping issues)
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = false,
                            RedirectStandardError = true,
                            WorkingDirectory = ffmpegWorkingDir
                        };

                        startInfo.ArgumentList.Add("-y");
                        startInfo.ArgumentList.Add("-i");
                        startInfo.ArgumentList.Add(item.FilePath);
                        startInfo.ArgumentList.Add("-vn");
                        startInfo.ArgumentList.Add("-af");
                        startInfo.ArgumentList.Add($"rubberband=pitch={pitchValueStr}");
                        startInfo.ArgumentList.Add("-b:a");
                        startInfo.ArgumentList.Add("192k");
                        startInfo.ArgumentList.Add(outputMp3);

                        using var cts = new CancellationTokenSource();

                        var progressTask = Task.Run(async () =>
                        {
                            while (!cts.Token.IsCancellationRequested)
                            {
                                await Task.Delay(50);
                                if (cts.Token.IsCancellationRequested) break;

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
                            process.ErrorDataReceived += (s, args) =>
                            {
                                if (!string.IsNullOrEmpty(args.Data))
                                {
                                    lock (errorBuilder)
                                    {
                                        errorBuilder.AppendLine(args.Data);
                                    }
                                    Debug.WriteLine($"[{item.Title}] {args.Data}");
                                }
                            };

                            process.Start();
                            process.BeginErrorReadLine();

                            await process.WaitForExitAsync();
                            process.CancelErrorRead();

                            int exitCode = process.ExitCode;
                            cts.Cancel();

                            string fullErrorOutput;
                            lock (errorBuilder)
                            {
                                fullErrorOutput = errorBuilder.ToString();
                            }

                            dispatcherQueue.TryEnqueue(() =>
                            {
                                item.DirectorySelectionEnabled = true;

                                if (exitCode == 0 && File.Exists(outputMp3))
                                {
                                    item.Progress = 100;
                                    item.ImageState = IconHolder.Instance.Success;
                                    item.OutputPath = outputMp3;
                                    item.ErrorToolTip = "Completed successfully";
                                    Debug.WriteLine(outputMp3 + "  Post Processing");

                                    ProcessedFiles.Add(item);
                                    stkOutputActions.Visibility = Visibility.Visible;
                                    item.VisibilityOfCompletedFileLocation = Visibility.Visible;
                                }
                                else
                                {
                                    item.Progress = 0;
                                    item.ImageState = IconHolder.Instance.Error;

                                    var errorLines = fullErrorOutput
                                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Where(line => line.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                                                       line.Contains("Invalid", StringComparison.OrdinalIgnoreCase) ||
                                                       line.Contains("failed", StringComparison.OrdinalIgnoreCase))
                                        .TakeLast(3)
                                        .ToList();

                                    if (errorLines.Count > 0)
                                    {
                                        item.ErrorToolTip = string.Join(Environment.NewLine, errorLines);
                                    }
                                    else
                                    {
                                        string lastLine = fullErrorOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "Unknown error";
                                        item.ErrorToolTip = $"Exit Code {exitCode}: {lastLine}";
                                    }

                                    Logger.Log($"Batch Pitch failed for {item.Title} (Code {exitCode}): {fullErrorOutput}", "BatchPitch", Logger.LogLevelType.Error);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            item.Progress = 0;
                            item.ImageState = IconHolder.Instance.Error;
                            item.DirectorySelectionEnabled = true;
                            item.ErrorToolTip = ex.Message;
                        });
                        Logger.Log(ex.Message, "BatchPitch", Logger.LogLevelType.Error);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }).ToList();

                await Task.WhenAll(tasks);
            }
            finally
            {
                btnAddAudioFiles.IsEnabled = true;
                mnftAddFiles.IsEnabled = true;
                btnApplyPitch.IsEnabled = true;
                btnExportDirectory.IsEnabled = !(chkCustomDirectories.IsChecked ?? false);
                throttler.Dispose();
            }
        }
        bool isValueChanging = false;

        private void numPitchValue_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (isValueChanging) return;
            if (numPitchValue == null || numSemiTones == null || numOctaves == null) return;
            if (double.IsNaN(numPitchValue.Value) || numPitchValue.Value <= 0) return;

            try
            {
                isValueChanging = true;

                // P = 2^(s/12) => s = 12 * log2(P)
                double semitones = 12.0 * Math.Log2(numPitchValue.Value);
                double octaves = semitones / 12.0;

                numSemiTones.Value = Math.Round(semitones, 4);
                numOctaves.Value = Math.Round(octaves, 4);
            }
            finally
            {
                isValueChanging = false;
            }
        }
        private void numSemiTones_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (isValueChanging) return;
            if (numPitchValue == null || numSemiTones == null || numOctaves == null) return;

            if (double.IsNaN(numSemiTones.Value)) return;

            try
            {
                isValueChanging = true;

                double semitones = numSemiTones.Value;
                double octaves = semitones / 12.0;
                double pitch = Math.Pow(2.0, semitones / 12.0); // Base must be 2.0

                numPitchValue.Value = Math.Round(pitch, 4);
                numOctaves.Value = Math.Round(octaves, 4);
            }
            finally
            {
                isValueChanging = false;
            }
        }

        private void numOctaves_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (isValueChanging) return;
            if (numPitchValue == null || numSemiTones == null || numOctaves == null) return;

            if (double.IsNaN(numOctaves.Value)) return;

            try
            {
                isValueChanging = true;

                double octaves = numOctaves.Value;
                double semitones = octaves * 12.0;
                double pitch = Math.Pow(2.0, octaves); // 2^octaves

                numSemiTones.Value = Math.Round(semitones, 4);
                numPitchValue.Value = Math.Round(pitch, 4);
            }
            finally
            {
                isValueChanging = false;
            }
        }
    }
}
