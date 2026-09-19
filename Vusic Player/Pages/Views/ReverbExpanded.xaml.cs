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
using System.Drawing.Text;
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
        ObservableCollection<FilesToModifyAdded> ProcessedFiles = new ObservableCollection<FilesToModifyAdded>();
        private async void btnApplyReverb_Click(object sender, RoutedEventArgs e)
        {
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
            string soxExecutablePath = Path.Combine(baseDir, "soxReverb", "sox.exe");
            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");
            // Capture the UI thread dispatcher before switching to background tasks
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);
            Debug.WriteLine(soxExecutablePath);
            Debug.WriteLine(ffmpegPath);
            if (!File.Exists(ffmpegPath) || !File.Exists(soxExecutablePath))
            {
                Debug.WriteLine($"Executable missing: FFmpeg={File.Exists(ffmpegPath)}, SoX={File.Exists(soxExecutablePath)}");
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
                        return;
                    }
                    // 1. Sanitize file name to avoid invalid Windows path characters
                    string sanitizedTitle = string.Join("_", item.Title.Split(Path.GetInvalidFileNameChars()));
                    string outputMp3 = Path.Combine(baseDirOutput, $"{sanitizedTitle}_{Guid.NewGuid()}.mp3");
                    if (txtOutputDirectory.Text != "Custom Directories")
                    {
                        if (!Directory.Exists(baseDirOutput))
                        {
                            Debug.WriteLine("Base Dir Output does not exist");
                            return;
                        }
                    }


                    if (item.IndividualDirectorySelBool)
                    {
                        outputMp3 = Path.Combine(item.DirectoryPath, $"{sanitizedTitle}_{Guid.NewGuid()}.mp3");
                        if (!Directory.Exists(item.DirectoryPath))
                        {
                            Debug.WriteLine("Item output doesnt exist " + item.OutputPath);
                            item.ImageState = "ms-appx:///Assets/error.png";
                            item.DirectorySelectionEnabled = true;
                            item.ErrorToolTip = "Output Directory not selected";
                            return;
                        }
                    }


                    Debug.WriteLine("Path to be processed: " + item.FilePath);
                    // Escape the pipeline properly for cmd.exe
                    string commandPipeline = $"\"{ffmpegPath}\" -i \"{item.FilePath}\" -f sox - | \"{soxExecutablePath}\" -p -p reverb {numReverberance.Value} {numHfDamping.Value} {numRoomScale.Value} {numStereoDepth.Value} {numPreDelay.Value} {numWetGain.Value} | \"{ffmpegPath}\" -i - -b:a 192k -f mp3 \"{outputMp3}\"";

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        // Use /c with outer quotes around the whole string. 
                        // cmd /c " "C:\path 1\ffmpeg.exe" ... | "C:\path 2\sox.exe" ... "
                        Arguments = $"/c \"{commandPipeline}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = false,
                        RedirectStandardError = true,
                        WorkingDirectory = baseDir // Crucial: sets context so relative path lookups do not fail
                    };

                    using var cts = new CancellationTokenSource();

                    var progressTask = Task.Run(async () =>
                    {
                        while (!cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                // Delay without passing the token so TaskCanceledException is never thrown
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
                        // Stream stderr lines in real-time to avoid pipe buffer deadlocks
                        process.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                errorBuilder.AppendLine(e.Data);

                                // This proves under the hood that ffmpeg/sox is actively writing output
                                System.Diagnostics.Debug.WriteLine($"[{item.Title}] {e.Data}");
                            }
                        };

                        process.Start();
                        process.BeginErrorReadLine(); // Starts reading stderr asynchronously

                        // Wait for the pipeline process to finish
                        await process.WaitForExitAsync();

                        int exitCode = process.ExitCode;
                        cts.Cancel(); // Stop the incremental progress loop

                        dispatcherQueue.TryEnqueue(() =>
                        {
                            if (exitCode == 0)
                            {
                                item.Progress = 100;
                                item.ImageState = "ms-appx:///Assets/success.png";
                                item.OutputPath = outputMp3;
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
                                item.ImageState = "ms-appx:///Assets/error.png";
                                System.Diagnostics.Debug.WriteLine($"Failed with code {exitCode}: {errorBuilder}");
                            }
                        });
                    }
                }
                finally
                {
                    throttler.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);            // Await all conversions in parallel
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
                if(exist != null)
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
            if(sender is MenuFlyoutItem mnft && mnft.DataContext is FilesToModifyAdded file)
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
                QueueService.VusicQueueNext.Add(new SongModel { FilePath = file.OutputPath, SongDuration = musicprops.Duration , Title = Path.GetFileNameWithoutExtension(file.OutputPath) });
            }
        }

        private async void btnPlayAllOutput_Click(object sender, RoutedEventArgs e)
        {
            var tempobservable = new ObservableCollection<SongModel>();
            foreach(var item in ProcessedFiles)
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
    }
}
