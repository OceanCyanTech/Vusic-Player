using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
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
    public class CountToAudioFilesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} audio file added" : $"• {count} audio files added";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class CountToAudioFilesProcessedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} audio file processed" : $"• {count} audio files processed";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ReverbExpanded : Page
    {
        ObservableCollection<FilesToModifyAdded> AddedFiles = new ObservableCollection<FilesToModifyAdded>();
        public ReverbExpanded()
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

        private void cmbReverb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbReverb == null || numReverberance == null) return;

            switch (cmbReverb.SelectedIndex)
            {
                case 0: UpdateReverbFields(30, 10, 20, 100, 5, -3); break; // Small Room
                case 1: UpdateReverbFields(60, 25, 70, 100, 15, 0); break; // Large Hall
                case 2: UpdateReverbFields(80, 40, 100, 100, 20, 3); break; // Cathedral
                case 3: UpdateReverbFields(50, 30, 50, 100, 10, 0); break; // Normal
                case 4: UpdateReverbFields(95, 60, 100, 100, 35, 4); break; // Cave
                case 5: UpdateReverbFields(70, 0, 15, 50, 10, 2); break; // Bathroom
                case 6: UpdateReverbFields(90, 50, 100, 100, 25, 3); break; // Ethereal
            }
        }
        private void UpdateReverbFields(double rev, double damp, double scale, double depth, double delay, double gain)
        {
            numReverberance.Value = rev;
            numHfDamping.Value = damp;
            numRoomScale.Value = scale;
            numStereoDepth.Value = depth;
            numPreDelay.Value = delay;
            numWetGain.Value = gain;
        }
        double GetValidValue(double val, double defaultValue)
        {
            return (double.IsNaN(val) || double.IsInfinity(val)) ? defaultValue : val;
        }
        ObservableCollection<FilesToModifyAdded> ProcessedFiles = new ObservableCollection<FilesToModifyAdded>();
        private async void btnApplyReverb_Click(object sender, RoutedEventArgs e)
        {
            if (AddedFiles.Count == 0) return;

            btnAddAudioFiles.IsEnabled = false;
            mnftAddFiles.IsEnabled = false;
            btnApplyReverb.IsEnabled = false;
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
            string soxExecutablePath = Path.Combine(baseDir, "soxReverb", "sox.exe");
            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");
            string soxWorkingDir = Path.GetDirectoryName(soxExecutablePath) ?? baseDir;

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);

            try
            {
                if (!File.Exists(ffmpegPath) || !File.Exists(soxExecutablePath))
                {
                    Debug.WriteLine($"Executable missing: FFmpeg={File.Exists(ffmpegPath)}, SoX={File.Exists(soxExecutablePath)}");
                    foreach (var item in AddedFiles)
                    {
                        item.ImageState = IconHolder.Instance.Error;
                        item.ErrorToolTip = "Audio processing engine missing (FFmpeg/SoX).";
                    }
                    return;
                }

                // Capture UI slider values safely on the UI thread before spinning up parallel tasks
                string reverberance = GetValidValue(numReverberance.Value, 50).ToString(CultureInfo.InvariantCulture);
                string hfDamping = GetValidValue(numHfDamping.Value, 50).ToString(CultureInfo.InvariantCulture);
                string roomScale = GetValidValue(numRoomScale.Value, 100).ToString(CultureInfo.InvariantCulture);
                string stereoDepth = GetValidValue(numStereoDepth.Value, 100).ToString(CultureInfo.InvariantCulture);
                string preDelay = GetValidValue(numPreDelay.Value, 0).ToString(CultureInfo.InvariantCulture);
                string wetGain = GetValidValue(numWetGain.Value, 0).ToString(CultureInfo.InvariantCulture);

                var tasks = AddedFiles.Select(async item =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        if (!File.Exists(item.FilePath))
                        {
                            dispatcherQueue.TryEnqueue(() =>
                            {
                                item.ImageState = IconHolder.Instance.Error;
                                item.DirectorySelectionEnabled = true;
                                item.ErrorToolTip = "Input file not found.";
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
                                    item.ErrorToolTip = "Output Directory not selected or does not exist.";
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
                                item.ErrorToolTip = "Output folder does not exist.";
                            });
                            return;
                        }

                        // -y forces overwrite if needed; -vn strips embedded cover art so it never stalls the SoX pipe
                        string commandPipeline = $"\"{ffmpegPath}\" -y -i \"{item.FilePath}\" -vn -f sox - | \"{soxExecutablePath}\" -p -p reverb {reverberance} {hfDamping} {roomScale} {stereoDepth} {preDelay} {wetGain} | \"{ffmpegPath}\" -y -i - -b:a 192k -f mp3 \"{outputMp3}\"";

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            // /s preserves outer quotes properly when paths contain spaces (e.g., WindowsApps)
                            Arguments = $"/s /c \"{commandPipeline}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = false,
                            RedirectStandardError = true,
                            WorkingDirectory = soxWorkingDir // Crucial: lets sox.exe find its bundled DLLs on clean PCs
                        };

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
                                    errorBuilder.AppendLine(args.Data);
                                    Debug.WriteLine($"[{item.Title}] {args.Data}");
                                }
                            };

                            process.Start();
                            process.BeginErrorReadLine();

                            await process.WaitForExitAsync();

                            int exitCode = process.ExitCode;
                            cts.Cancel();

                            dispatcherQueue.TryEnqueue(() =>
                            {
                                item.DirectorySelectionEnabled = true;

                                if (exitCode == 0 && File.Exists(outputMp3))
                                {
                                    item.Progress = 100;
                                    item.ImageState = IconHolder.Instance.Success;
                                    item.OutputPath = outputMp3;
                                    item.VisibilityOfCompletedFileLocation = Visibility.Visible;
                                    ProcessedFiles.Add(item);
                                    stkOutputActions.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    item.Progress = 0;
                                    item.ImageState = IconHolder.Instance.Error;
                                    string errSummary = errorBuilder.ToString().Trim();
                                    item.ErrorToolTip = string.IsNullOrEmpty(errSummary)
                                        ? $"Processing failed (Exit Code {exitCode})."
                                        : errSummary;
                                    Logger.Log($"Batch Reverb failed for {item.Title} (Code {exitCode}): {errSummary}", "BatchReverb", Logger.LogLevelType.Error);
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
                        Logger.Log(ex.Message, "BatchReverb", Logger.LogLevelType.Error);
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
                // Always restore UI controls even if every track failed or an early return occurred
                btnAddAudioFiles.IsEnabled = true;
                mnftAddFiles.IsEnabled = true;
                btnApplyReverb.IsEnabled = true;
                btnExportDirectory.IsEnabled = !(chkCustomDirectories.IsChecked ?? false);
                throttler.Dispose();
            }
        }
        string OutputDirectoryCommon = "";
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

        private void mnftCopyFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
            {
                CopyToClipboard.CopyStringToClipboard(file.FilePath);
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
                if (File.Exists(file.OutputPath))
                {
                    var stfile = await StorageFile.GetFileFromPathAsync(file.OutputPath);
                    var musicprops = await stfile.Properties.GetMusicPropertiesAsync();
                    QueueService.VusicQueue.Add(new SongModel { FilePath = file.OutputPath, Title = Path.GetFileNameWithoutExtension(file.OutputPath), SongDuration = musicprops.Duration });
                    QueueService.VusicQueueNext.Add(new SongModel { FilePath = file.OutputPath, SongDuration = musicprops.Duration, Title = Path.GetFileNameWithoutExtension(file.OutputPath) });
                }
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
                if (File.Exists(item.OutputPath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(item.OutputPath);
                    var musicprops = await file.Properties.GetMusicPropertiesAsync();
                    QueueService.VusicQueue.Add(new SongModel { Title = Path.GetFileNameWithoutExtension(item.OutputPath), FilePath = item.OutputPath, SongDuration = musicprops.Duration });
                    QueueService.VusicQueueNext.Add(new SongModel { Title = Path.GetFileNameWithoutExtension(item.OutputPath), FilePath = item.OutputPath, SongDuration = musicprops.Duration });
                }
            }
        }
    }
}
