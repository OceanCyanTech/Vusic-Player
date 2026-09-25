using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
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
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
                tempobservable.Add(new SongModel { Title = Path.GetFileNameWithoutExtension(item.OutputPath), FilePath = item.OutputPath });
            }
            QueueService.PlayMedia(tempobservable, false, false);
        }
        private async void btnAddtoQueueAllOutput_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ProcessedFiles)
            {
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

        private async void btnApplyReverb_Click(object sender, RoutedEventArgs e)
        {
            if (AddedFiles.Count == 0) return;
            if (numPitchValue.Text == "")
            {
                numPitchValue.Value = 1;
            }
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

            if (txtOutputDirectory.Text == "")
            {
                baseDirOutput = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            string baseDir = AppContext.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");
            // Capture the UI thread dispatcher before switching to background tasks
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);
            Debug.WriteLine(ffmpegPath);
            if (!File.Exists(ffmpegPath))
            {
                Debug.WriteLine($"Executable missing: FFmpeg={File.Exists(ffmpegPath)}");
                return;
            }

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
                            item.ImageState = "ms-appx:///Assets/error.png";
                            item.ErrorToolTip = "Input file does not exist on disk.";
                        });
                        return;
                    }

                    string sanitizedTitle = string.Join("_", item.Title.Split(Path.GetInvalidFileNameChars()));
                    string outputMp3 = Path.Combine(baseDirOutput, $"{sanitizedTitle}_{Guid.NewGuid()}.mp3");

                    if (txtOutputDirectory.Text != "Custom Directories" && !Directory.Exists(baseDirOutput))
                    {
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            item.ImageState = "ms-appx:///Assets/error.png";
                            item.ErrorToolTip = "Output directory does not exist.";
                        });
                        return;
                    }

                    if (item.IndividualDirectorySelBool)
                    {
                        outputMp3 = Path.Combine(item.DirectoryPath, $"{sanitizedTitle}_{Guid.NewGuid()}.mp3");
                        if (!Directory.Exists(item.DirectoryPath))
                        {
                            dispatcherQueue.TryEnqueue(() =>
                            {
                                item.ImageState = "ms-appx:///Assets/error.png";
                                item.DirectorySelectionEnabled = true;
                                item.ErrorToolTip = "Output directory not selected or does not exist.";
                            });
                            return;
                        }
                    }

                    // -vn prevents corrupt embedded album art from crashing conversion
                    string commandPipeline = $"\"{ffmpegPath}\" -i \"{item.FilePath}\" -vn -af \"rubberband=pitch={numPitchValue.Value}\" \"{outputMp3}\"";

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
                                await Task.Delay(50);
                            }
                            catch
                            {
                                break;
                            }

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
                        process.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                lock (errorBuilder)
                                {
                                    errorBuilder.AppendLine(e.Data);
                                }
                                Debug.WriteLine($"[{item.Title}] {e.Data}");
                            }
                        };

                        process.Start();
                        process.BeginErrorReadLine();

                        await process.WaitForExitAsync();

                        // Crucial: Stop async error reading before the using block calls process.Dispose()
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
                            if (exitCode == 0)
                            {
                                item.Progress = 100;
                                item.ImageState = "ms-appx:///Assets/success.png";
                                item.OutputPath = outputMp3;
                                item.ErrorToolTip = "Completed successfully";
                                Debug.WriteLine(outputMp3 + "  Post Processing");

                                ProcessedFiles.Add(item);
                                btnAddAudioFiles.IsEnabled = true;
                                mnftAddFiles.IsEnabled = true;
                                btnApplyReverb.IsEnabled = true;
                                btnExportDirectory.IsEnabled = !chkCustomDirectories.IsChecked ?? false;
                                item.DirectorySelectionEnabled = true;
                                stkOutputActions.Visibility = Visibility.Visible;
                                item.VisibilityOfCompletedFileLocation = Visibility.Visible;
                            }
                            else
                            {
                                item.Progress = 0;
                                item.ImageState = "ms-appx:///Assets/error.png";
                                item.DirectorySelectionEnabled = true;

                                // Extract the relevant error line(s) for the tooltip
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
                                    // Fallback to exit code and the last stderr line
                                    string lastLine = fullErrorOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "Unknown error";
                                    item.ErrorToolTip = $"Exit Code {exitCode}: {lastLine}";
                                }

                                Debug.WriteLine($"Failed with code {exitCode}: {fullErrorOutput}");
                            }
                        });
                    }
                }
                finally
                {
                    throttler.Release();
                }
            }).ToList();
            await Task.WhenAll(tasks);
        }
        bool isValueChanging = false;

        private void numPitchValue_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (isValueChanging) return;
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
