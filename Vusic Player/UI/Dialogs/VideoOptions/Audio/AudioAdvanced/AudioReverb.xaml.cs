using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Pages.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioAdvanced
{
    public sealed partial class AudioReverb : UserControl
    {
        public AudioReverb()
        {
            InitializeComponent();
        }
        private void ProgressTimer(bool isStop)
        {

            prgApplyingReverb.Visibility = Visibility.Visible;
            txtApplyReverbProgress.Visibility = Visibility.Visible;
            prgApplyingReverb.ShowError = false;
            prgApplyingReverb.Value = 0;


            progressTimer.Interval = TimeSpan.FromMilliseconds(50);

            progressTimer.Tick += (s, args) =>
            {
                if (prgApplyingReverb.Value < 95)
                {
                    prgApplyingReverb.Value += 0.95;
                    txtApplyReverbProgress.Text = $"Applying reverb: {prgApplyingReverb.Value:F0}%";
                }
                else
                {
                    txtApplyReverbProgress.Text = "Almost there.... 96%";
                }
            };

            progressTimer.Start();
        }
        DispatcherTimer progressTimer = new DispatcherTimer();
        MemoryStream? msstream;
        private async void ProcessReverb(string dynamicCommand, bool isMemory, string outputMP3 = "")
        {
            Debug.WriteLine(dynamicCommand);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {dynamicCommand}",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (Process process = new Process { StartInfo = startInfo })
            {
                Debug.WriteLine("Started reverb0");
                process.Start();

                //      ProcessErrorLog = await process.StandardError.ReadToEndAsync();

                Debug.WriteLine("Started reverb");
                if (isMemory)
                {
                    msstream = PlayerService.msReverb;
                    if (msstream == null)
                    {
                        msstream = new MemoryStream();
                        PlayerService.msReverb = msstream;
                    }
                    else
                    {
                        msstream.SetLength(0);
                    }
                    Task copyTask = process.StandardOutput.BaseStream.CopyToAsync(msstream);
                    await Task.WhenAll(copyTask, process.WaitForExitAsync());
                    progressTimer.Stop();
                    if (process.ExitCode == 0)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer!.CurTime);
                        curtime = curTime;
                        curtimetemp = PlayerService.Masterplayer.CurTime;
                        PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
                        PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
                        PlayerService.Masterplayer?.Open(msstream);

                        prgApplyingReverb.Value = 100;
                        txtApplyReverbProgress.Text = "Applied Reverb successfully!";

                    }
                    else
                    {
                        ShowError(ProcessErrorLog);
                    }
                }
                else
                {
                    if (process.ExitCode == 0)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer!.CurTime);
                        curtime = curTime;
                        curtimetemp = PlayerService.Masterplayer.CurTime;
                        PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
                        PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
                        PlayerService.OpenPath(outputMP3);

                        prgApplyingReverb.Value = 100;
                        txtApplyReverbProgress.Text = "Applied Reverb successfully!";

                    }
                    else
                    {
                        ShowError(ProcessErrorLog);
                    }
                }

            }

        }
        private void ShowError(string errorLog)
        {
            Debug.WriteLine($"Pipeline failed: {errorLog}");
            prgApplyingReverb.ShowError = true;
            txtApplyReverbProgress.Text = "An unexpected error occured. Check log page for details.";
            Logger.Log(errorLog, "AudioReverb", Logger.LogLevelType.Error);
        }
        string ProcessErrorLog = "";
        private async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.CurrentPlayingPath == null || PlayerService.Masterplayer == null) return;

            string? tempFileToClean = null;

            try
            {
                string baseDir = AppContext.BaseDirectory;
                string soxExecutablePath = Path.Combine(baseDir, "soxReverb", "sox.exe");
                string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");

                if (!File.Exists(soxExecutablePath) || !File.Exists(ffmpegPath))
                {
                    txtApplyReverbProgress.Text = "Required audio components are missing.";
                    prgApplyingReverb.Visibility = Visibility.Visible;
                    return;
                }

                string inputMp3 = PlayerService.CurrentPlayingPath ?? "";
                string outputMp3 = "";
                bool isMemoryStream = false;

                prgApplyingReverb.Visibility = Visibility.Visible;
                txtApplyReverbProgress.Visibility = Visibility.Visible;
                prgApplyingReverb.ShowError = false;
                prgApplyingReverb.Value = 0;

                if (chckExporttoFile.IsChecked == false)
                {
                    if (cmbOutput.SelectedIndex == 0)
                    {
                        isMemoryStream = true;
                    }
                    else
                    {
                        // Write temp files to writable TempPath, NEVER to baseDir
                        outputMp3 = Path.Combine(Path.GetTempPath(), $"vusic_temp_{Guid.NewGuid():N}.mp3");
                        tempFileToClean = outputMp3;
                        if (File.Exists(outputMp3))
                        {
                            File.Delete(outputMp3);
                        }
                    }
                }
                else
                {
                    if (App.MainWindowInstance == null) return;

                    var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Export", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
                    if (folder == null) return;

                    if (string.IsNullOrEmpty(txtFileName.Text))
                    {
                        txtEnterFileNameWarning.Visibility = Visibility.Visible;
                        return;
                    }

                    string userFileName = txtFileName.Text.Trim();
                    char[] invalidChars = Path.GetInvalidFileNameChars();
                    foreach (char c in invalidChars)
                    {
                        userFileName = userFileName.Replace(c, '_');
                    }
                    userFileName = userFileName.TrimEnd('.');

                    outputMp3 = Path.Combine(folder.Path, userFileName + ".mp3");
                    int counter = 1;
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(outputMp3);
                    string extension = Path.GetExtension(outputMp3);

                    while (File.Exists(outputMp3))
                    {
                        outputMp3 = Path.Combine(folder.Path, $"{nameWithoutExt} ({counter}){extension}");
                        counter++;
                    }

                    txtFileName.Text = Path.GetFileName(outputMp3);
                }

                string outTarget = isMemoryStream ? "-" : $"\"{outputMp3}\"";

                // Double quote wrapping to ensure cmd.exe handles spaces in Program Files/WindowsApps correctly
                string dynamicCommand = $"/s /c \"\"{ffmpegPath}\" -y -i \"{inputMp3}\" -f sox - | \"{soxExecutablePath}\" -p -p reverb {numReverberance.Value} {numHfDamping.Value} {numRoomScale.Value} {numStereoDepth.Value} {numPreDelay.Value} {numWetGain.Value} | \"{ffmpegPath}\" -y -i - -b:a 192k -f mp3 {outTarget}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = dynamicCommand,
                    WorkingDirectory = Path.GetDirectoryName(soxExecutablePath), // Crucial for SoX to locate dependent DLLs
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = isMemoryStream, // Only redirect output if we are reading it into memory
                    RedirectStandardError = true
                };

                var progressTimer = new Microsoft.UI.Xaml.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                progressTimer.Tick += (s, args) =>
                {
                    if (prgApplyingReverb.Value < 95)
                    {
                        prgApplyingReverb.Value += 0.95;
                        txtApplyReverbProgress.Text = $"Applying reverb: {prgApplyingReverb.Value:F0}%";
                    }
                };
                progressTimer.Start();

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    Task? copyTask = null;
                    MemoryStream? ms = null;

                    if (isMemoryStream)
                    {
                        ms = PlayerService.msReverb ?? new MemoryStream();
                        if (PlayerService.msReverb == null) PlayerService.msReverb = ms;
                        else ms.SetLength(0);

                        copyTask = process.StandardOutput.BaseStream.CopyToAsync(ms);
                    }

                    // Read error stream asynchronously to prevent deadlock
                    Task<string> errorTask = process.StandardError.ReadToEndAsync();

                    if (copyTask != null)
                    {
                        await Task.WhenAll(copyTask, errorTask, process.WaitForExitAsync());
                    }
                    else
                    {
                        await Task.WhenAll(errorTask, process.WaitForExitAsync());
                    }

                    string errorLog = await errorTask;
                    progressTimer.Stop();

                    if (process.ExitCode == 0)
                    {
                        prgApplyingReverb.Value = 100;
                        txtApplyReverbProgress.Text = "Reverb Applied Successfully!";

                        curtime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                        curtimetemp = PlayerService.Masterplayer.CurTime;

                        PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
                        PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;

                        if (isMemoryStream && ms != null)
                        {
                            ms.Position = 0;
                            if (PlayerService.Masterplayer.IsPlaying == false)
                            {
                                PlayerService.CurrentMemoryStream = ms;
                                PlayerService.curtime = curtime;
                                PlayerService.curtimetemp = curtimetemp;
                                PlayerService.JustDisposed = true;
                                PlayerService.filestreamcurrent?.Dispose();
                            }
                            else
                            {
                                PlayerService.Masterplayer.Open(ms);
                            }
                        }
                        else
                        {
                            if (PlayerService.Masterplayer.IsPlaying == false)
                            {
                                PlayerService.curtime = curtime;
                                PlayerService.curtimetemp = curtimetemp;
                                PlayerService.CurrentPlayingPath = outputMp3;
                                if (chckExporttoFile.IsChecked == true)
                                {
                                    PlayerService.UIController.MediaDisplayName = Path.GetFileName(outputMp3);
                                }
                                PlayerService.JustDisposed = true;
                                PlayerService.filestreamcurrent?.Dispose();
                            }
                            else
                            {
                                PlayerService.filestreamcurrent = new FileStream(outputMp3, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                                PlayerService.Masterplayer.Open(PlayerService.filestreamcurrent);
                            }
                        }
                    }
                    else
                    {
                        // Surface the real error from FFmpeg/SoX
                        prgApplyingReverb.Value = 0;
                        prgApplyingReverb.ShowError = true;
                        txtApplyReverbProgress.Text = "Failed to process audio stream.";

                        System.Diagnostics.Debug.WriteLine($"[AudioReverb Failure - ExitCode: {process.ExitCode}]");
                        System.Diagnostics.Debug.WriteLine(errorLog);
                        Logger.Log($"ExitCode {process.ExitCode}: {errorLog}", "AudioReverb", Logger.LogLevelType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                prgApplyingReverb.ShowError = true;
                txtApplyReverbProgress.Text = "Error applying reverb.";
                Logger.Log(ex.Message, "AudioReverb", Logger.LogLevelType.Error);
            }
        }
        public void ClearProgress()
        {
            if (txtApplyReverbProgress.Text == "Reverb Applied Successfully")
            {
                prgApplyingReverb.Value = 0;
                prgApplyingReverb.Visibility = Visibility.Collapsed;
                txtApplyReverbProgress.Visibility = Visibility.Collapsed;
                txtApplyReverbProgress.Text = "";
            }
        }
        private static long curtimetemp;
        private static TimeSpan curtime;
        public static MediaPlaybackController media => MediaPlaybackController.Instance;

        private void Masterplayer_OpenCompleted1(object? sender, OpenCompletedArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            Debug.WriteLine("Open Completed");
            PlayerService.Masterplayer.SeekAccurate((int)(curtimetemp / 10000));
            media.CurrentPosition = curtime.TotalSeconds;
            curtime = TimeSpan.Zero;
            curtimetemp = 0;

            //         PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chck)
            {
                if (chck.IsChecked == true)
                {
                    txtMore.Visibility = Visibility.Visible;
                    mnfs1.Visibility = Visibility.Visible;
                    grdMoreOptions.Visibility = Visibility.Visible;
                }
                else
                {
                    txtMore.Visibility = Visibility.Collapsed;
                    mnfs1.Visibility = Visibility.Collapsed;
                    grdMoreOptions.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk)
            {
                txtEnterFileNameWarning.Visibility = Visibility.Collapsed;

                if (chk.IsChecked == true)
                {
                    cmbOutput.IsEnabled = false;
                    txtFileName.Visibility = Visibility.Visible;
                    txtFileName.Text = Path.GetFileNameWithoutExtension(PlayerService.CurrentPlayingPath) + "_reverb";
                }
                else
                {
                    txtFileName.Visibility = Visibility.Collapsed;

                    cmbOutput.IsEnabled = true;
                }
            }
        }

        private void cmbOutput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbOutput.SelectedIndex == 0)
            {
                ToolTipService.SetToolTip(cmbOutput, "Creates reverb output in RAM");
            }
            else
            {
                ToolTipService.SetToolTip(cmbOutput, "Creates output as temporary file on Disk which is deleted when app is closed");
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

        // Helper method to eliminate copy-pasted UI assignment lines
        private void UpdateReverbFields(double rev, double damp, double scale, double depth, double delay, double gain)
        {
            numReverberance.Value = rev;
            numHfDamping.Value = damp;
            numRoomScale.Value = scale;
            numStereoDepth.Value = depth;
            numPreDelay.Value = delay;
            numWetGain.Value = gain;
        }

        private void txtFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                txtEnterFileNameWarning.Visibility = Visibility.Visible;

            }
            else
            {
                txtEnterFileNameWarning.Visibility = Visibility.Collapsed;
            }
        }
        public event Action? ReverbExpand;

        private void btnApplyMultiple_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                if (App.NavigationFrame.CurrentSourcePageType != typeof(ReverbExpanded))
                {
                    App.NavigationFrame.Navigate(typeof(ReverbExpanded));
                    ReverbExpand?.Invoke();
                }
            }
        }
    }
}
