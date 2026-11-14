using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.Playback;
using System.Collections.Generic;

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
        private static Process CreateDirectProcess(string exePath, IEnumerable<string> args, string workingDir, bool redirectStdin, bool redirectStdout)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = redirectStdin,
                RedirectStandardOutput = redirectStdout,
                RedirectStandardError = true
            };

            foreach (var arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            return new Process { StartInfo = psi };
        }

        private async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.CurrentPlayingPath == null || PlayerService.Masterplayer == null) return;

            string? tempOutputMp3 = null;
            Microsoft.UI.Xaml.DispatcherTimer? progressTimer = null;

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

                string tempWorkDir = Path.Combine(Path.GetTempPath(), "VusicPlayer_Reverb");
                Directory.CreateDirectory(tempWorkDir);

                string inputMp3 = PlayerService.CurrentPlayingPath ?? "";
                string outputMp3 = "";
                string finalFileName = "";
                Windows.Storage.StorageFolder? destinationFolder = null;
                bool isExportingToFile = chckExporttoFile.IsChecked == true;
                bool isMemoryStream = !isExportingToFile && cmbOutput.SelectedIndex == 0;

                prgApplyingReverb.Visibility = Visibility.Visible;
                txtApplyReverbProgress.Visibility = Visibility.Visible;
                prgApplyingReverb.ShowError = false;
                prgApplyingReverb.Value = 0;

                if (!isExportingToFile)
                {
                    if (!isMemoryStream)
                    {
                        outputMp3 = Path.Combine(tempWorkDir, $"vusic_temp_{Guid.NewGuid():N}.mp3");
                        tempOutputMp3 = outputMp3;
                    }
                }
                else
                {
                    if (App.MainWindowInstance == null) return;

                    if (string.IsNullOrWhiteSpace(txtFileName.Text))
                    {
                        txtEnterFileNameWarning.Visibility = Visibility.Visible;
                        return;
                    }
                    txtEnterFileNameWarning.Visibility = Visibility.Collapsed;

                    destinationFolder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Export", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
                    if (destinationFolder == null) return;

                    string userFileName = txtFileName.Text.Trim();
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        userFileName = userFileName.Replace(c, '_');
                    }
                    userFileName = userFileName.TrimEnd('.');

                    outputMp3 = Path.Combine(destinationFolder.Path, userFileName + ".mp3");
                    int counter = 1;
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(outputMp3);
                    string extension = Path.GetExtension(outputMp3);

                    while (File.Exists(outputMp3))
                    {
                        outputMp3 = Path.Combine(destinationFolder.Path, $"{nameWithoutExt} ({counter}){extension}");
                        counter++;
                    }

                    finalFileName = Path.GetFileName(outputMp3);
                    txtFileName.Text = finalFileName;

                    // Write to writable %TEMP% first, then move into the picked folder
                    tempOutputMp3 = Path.Combine(tempWorkDir, $"{Guid.NewGuid():N}_{finalFileName}");
                }

                // Sanitize parameters against NaN and comma-decimal locales
                string reverberance = (double.IsNaN(numReverberance.Value) ? 50 : numReverberance.Value).ToString(CultureInfo.InvariantCulture);
                string hfDamping = (double.IsNaN(numHfDamping.Value) ? 50 : numHfDamping.Value).ToString(CultureInfo.InvariantCulture);
                string roomScale = (double.IsNaN(numRoomScale.Value) ? 100 : numRoomScale.Value).ToString(CultureInfo.InvariantCulture);
                string stereoDepth = (double.IsNaN(numStereoDepth.Value) ? 100 : numStereoDepth.Value).ToString(CultureInfo.InvariantCulture);
                string preDelay = (double.IsNaN(numPreDelay.Value) ? 0 : numPreDelay.Value).ToString(CultureInfo.InvariantCulture);
                string wetGain = (double.IsNaN(numWetGain.Value) ? 0 : numWetGain.Value).ToString(CultureInfo.InvariantCulture);

                string encodeTarget = isMemoryStream ? "-" : tempOutputMp3!;

                progressTimer = new Microsoft.UI.Xaml.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                progressTimer.Tick += (s, args) =>
                {
                    if (prgApplyingReverb.Value < 95)
                    {
                        prgApplyingReverb.Value += 0.95;
                        txtApplyReverbProgress.Text = $"Applying reverb: {prgApplyingReverb.Value:F0}%";
                    }
                };
                progressTimer.Start();

                var errorBuilder = new System.Text.StringBuilder();

                // Stage 1: Decode input audio to raw SoX stream (no cmd.exe)
                using var decodeProc = CreateDirectProcess(
                    ffmpegPath,
                    new[] { "-y", "-i", inputMp3, "-vn", "-f", "sox", "-" },
                    tempWorkDir,
                    redirectStdin: false,
                    redirectStdout: true);

                // Stage 2: Apply SoX reverb effect
                using var soxProc = CreateDirectProcess(
                    soxExecutablePath,
                    new[] { "-p", "-p", "reverb", reverberance, hfDamping, roomScale, stereoDepth, preDelay, wetGain },
                    tempWorkDir,
                    redirectStdin: true,
                    redirectStdout: true);

                // Stage 3: Encode processed SoX stream to MP3 (stdout for MemoryStream, or temp file path)
                using var encodeProc = CreateDirectProcess(
                    ffmpegPath,
                    new[] { "-y", "-i", "-", "-b:a", "192k", "-f", "mp3", encodeTarget },
                    tempWorkDir,
                    redirectStdin: true,
                    redirectStdout: isMemoryStream);

                void AttachErrorLogger(string stage, Process p)
                {
                    p.ErrorDataReceived += (s, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            lock (errorBuilder)
                            {
                                errorBuilder.AppendLine($"[{stage}] {args.Data}");
                            }
                            System.Diagnostics.Debug.WriteLine($"[AudioReverb - {stage}] {args.Data}");
                        }
                    };
                }

                AttachErrorLogger("Decode", decodeProc);
                AttachErrorLogger("SoX", soxProc);
                AttachErrorLogger("Encode", encodeProc);

                decodeProc.Start();
                soxProc.Start();
                encodeProc.Start();

                decodeProc.BeginErrorReadLine();
                soxProc.BeginErrorReadLine();
                encodeProc.BeginErrorReadLine();

                // Pipe Stage 1 stdout -> Stage 2 stdin
                var pipe1To2 = Task.Run(async () =>
                {
                    try
                    {
                        await decodeProc.StandardOutput.BaseStream.CopyToAsync(soxProc.StandardInput.BaseStream);
                    }
                    catch { }
                    finally
                    {
                        try { soxProc.StandardInput.Close(); } catch { }
                    }
                });

                // Pipe Stage 2 stdout -> Stage 3 stdin
                var pipe2To3 = Task.Run(async () =>
                {
                    try
                    {
                        await soxProc.StandardOutput.BaseStream.CopyToAsync(encodeProc.StandardInput.BaseStream);
                    }
                    catch { }
                    finally
                    {
                        try { encodeProc.StandardInput.Close(); } catch { }
                    }
                });

                // Optional Stage 3 stdout -> MemoryStream
                Task? pipe3ToMemory = null;
                MemoryStream? ms = null;

                if (isMemoryStream)
                {
                    ms = PlayerService.msReverb ?? new MemoryStream();
                    if (PlayerService.msReverb == null) PlayerService.msReverb = ms;
                    else ms.SetLength(0);

                    pipe3ToMemory = encodeProc.StandardOutput.BaseStream.CopyToAsync(ms);
                }

                var waitTasks = new System.Collections.Generic.List<Task>
        {
            pipe1To2,
            pipe2To3,
            decodeProc.WaitForExitAsync(),
            soxProc.WaitForExitAsync(),
            encodeProc.WaitForExitAsync()
        };

                if (pipe3ToMemory != null)
                {
                    waitTasks.Add(pipe3ToMemory);
                }

                await Task.WhenAll(waitTasks);

                decodeProc.CancelErrorRead();
                soxProc.CancelErrorRead();
                encodeProc.CancelErrorRead();

                progressTimer.Stop();

                int exitCode = (decodeProc.ExitCode == 0 && soxProc.ExitCode == 0 && encodeProc.ExitCode == 0)
                    ? 0
                    : (encodeProc.ExitCode != 0 ? encodeProc.ExitCode : (soxProc.ExitCode != 0 ? soxProc.ExitCode : decodeProc.ExitCode));

                if (exitCode == 0)
                {
                    if (isExportingToFile && destinationFolder != null && !string.IsNullOrEmpty(tempOutputMp3) && File.Exists(tempOutputMp3))
                    {
                        try
                        {
                            File.Move(tempOutputMp3, outputMp3, overwrite: true);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            var tempStorageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempOutputMp3);
                            await tempStorageFile.MoveAsync(destinationFolder, finalFileName, Windows.Storage.NameCollisionOption.ReplaceExisting);
                        }
                    }

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
                            if (isExportingToFile)
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
                    if (!string.IsNullOrEmpty(tempOutputMp3) && File.Exists(tempOutputMp3))
                    {
                        try { File.Delete(tempOutputMp3); } catch { }
                    }

                    prgApplyingReverb.Value = 0;
                    prgApplyingReverb.ShowError = true;
                    txtApplyReverbProgress.Text = "Failed to process audio stream.";

                    string errorLog;
                    lock (errorBuilder)
                    {
                        errorLog = errorBuilder.ToString();
                    }

                    System.Diagnostics.Debug.WriteLine($"[AudioReverb Failure - ExitCode: {exitCode}]");
                    System.Diagnostics.Debug.WriteLine(errorLog);
                    Logger.Log($"ExitCode {exitCode}: {errorLog}", "AudioReverb", Logger.LogLevelType.Error);
                }
            }
            catch (Exception ex)
            {
                progressTimer?.Stop();
                prgApplyingReverb.ShowError = true;
                txtApplyReverbProgress.Text = "Error applying reverb.";
                Logger.Log(ex.Message, "AudioReverb", Logger.LogLevelType.Error);
            }
            finally
            {
                progressTimer?.Stop();
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
