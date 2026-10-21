// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Storage;
using Windows.UI;
using static Vusic_Player.Configuration.AppConfig.Logger;
using FileAttributes = System.IO.FileAttributes;
using Logger = Vusic_Player.Configuration.AppConfig.Logger;

namespace Vusic_Player.Configuration
{
    public class PlayerService
    {
        public static Player? Masterplayer { get; set; }
        public static bool MediaCompleted { get; set; }
        public static event Action? PlayPauseChanged;
        public static event Action<string, LogLevelType>? LoggedMessage;
        public static MemoryStream? msReverb { get; set; }
        public static MediaPlaybackController UIController => MediaPlaybackController.Instance;
        public static int? originalvolume;
        public static string currentvol = "0";

        public static DispatcherTimer? maintimer { get; set; }
        public static FileStream? filestreamcurrent;
        public static string CurrentPlayingPath { get; set; } = "";

        public static event Action? ErrorCalled;
        public static string ErrorString { get; set; } = "";
        public static event Action? PlayCalled;
        public static event Action? PIPRestore;
        public static event Action? CheckProcesses;
        public static List<Process>? processlocklist;
        public static bool FileRenameIssue = false;
        public static bool isProgress = true;
        private static bool _isDragging = false;
        public static long curtimetemp;
        public static TimeSpan curtime;
        public static bool InVideoPage = false;
        public static XAudio2MultiOutputEngine? _multiAudioEngine;


        public static bool JustDisposed = false;
        public static void PIPRestoreAction()
        {
            PIPRestore?.Invoke();
        }
        private static void Maintimer_Tick(object? sender, object e)
        {
            if (!_isDragging && Masterplayer != null)
            {
                var curTime = TimeSpan.FromTicks(Masterplayer.CurTime);
                UIController.CurrentPosition = curTime.TotalSeconds;
                if (UIController.IsTotalDurationTapped)
                {
                    UIController.TotalDuration = UIController.TotalDurationTemp - UIController.CurrentPosition;
                }
            }
        }
        private const uint SHARD_PATHW = 0x00000003;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHAddToRecentDocs(uint uFlags, IntPtr pv);
        public static void RemoveFileFromExplorerHome(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                // 1. Locate the Shell Recent directory
                string recentDir = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                string fileNameOnly = Path.GetFileName(filePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

                var dir = new DirectoryInfo(recentDir);
                if (!dir.Exists) return;

                // 2. Find and delete the shortcut Explorer generated
                foreach (var file in dir.GetFiles("*.lnk"))
                {
                    // Check if the link name contains the file name
                    if (file.Name.Contains(fileNameWithoutExt, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {
                            // File might be briefly locked while Explorer creates the tile
                        }
                    }
                }

                // 3. Notify the shell that the recent docs list changed
                // Passing 0 and IntPtr.Zero to SHAddToRecentDocs clears the shell cache
                SHAddToRecentDocs(0, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExplorerHomePrivacy] Error: {ex.Message}");
            }
        }
        public static void VolumeChange(double obj)
        {
            UIController.VolumeString = ((int)obj).ToString() + "%";
            if (Masterplayer == null) return;

            int vol = (int)obj;
            if (vol != 0)
            {
                originalvolume = vol;
            }
            currentvol = vol.ToString();
            UIController.VolumeString = currentvol + "%";
            Masterplayer.Audio.Volume = vol;
            UIController.VolumeGlyph = vol switch
            {
                0 => "\uE74F", // Mute
                < 10 => "\uE992", // Low
                < 40 => "\uE993", // Med-Low
                < 80 => "\uE994", // Med
                _ => "\uE995"  // High / Max
            };

            // 2. Determine the Color (Defaults to White)
            Color iconColor = vol switch
            {
                >= 200 => Colors.Red,      // Red for 200+
                >= 150 => Colors.Orange,   // Orange for 150-199
                > 100 => Colors.Yellow,   // Yellow for 101-149
                _ => Colors.White     // White for 0-100
            };
            UIController.VolumeValue = vol;

            UIController.VolumeForeground = new SolidColorBrush(iconColor);
        }

        public static void PlayPause()
        {
            if (CurrentPlayingPath == "" || CurrentPlayingPath == null)
            {
                Debug.WriteLine("ssh");
                if (QueueService.VusicQueueNext.Count != 0)
                {
                    Debug.WriteLine("ssh2");

                    //     QueueService.PlayNext();
                    return;
                }
            }
            else
            {
                Debug.WriteLine("jd" + CurrentPlayingPath);
                Debug.WriteLine("shsh");
                if (CurrentPlayingPath == null)
                {
                    Debug.WriteLine("null");
                }
            }
            if (Masterplayer == null) return;

            if (Masterplayer.IsPlaying) Pause();
            else Play();
        }

        public static void SeekBefore()
        {
            if (Masterplayer == null) return;
            var currentTicks = Masterplayer.CurTime;
            var targetTime = TimeSpan.FromTicks(currentTicks).Subtract(TimeSpan.FromSeconds(SeekServiceInstance.SkipBack));
            if (targetTime < TimeSpan.Zero) targetTime = TimeSpan.Zero;

            // 2. Seek using the clean millisecond conversion
            Masterplayer.SeekAccurate((int)targetTime.TotalMilliseconds);
            UIController.CurrentPosition = targetTime.TotalSeconds;
            //         UIController.CurrentPosition = targetTime.TotalSeconds;
            Configuration.Helper.UI.SeekInfoService.ShowSeek(-SeekServiceInstance.SkipBack);
        }
        public static SeekService SeekServiceInstance => SeekService.Instance;
        public static void SeekAhead()
        {
            if (Masterplayer == null) return;

            // 1. Calculate the new time (Adding 10 seconds via TimeSpan)
            var currentTicks = Masterplayer.CurTime;
            var targetTime = TimeSpan.FromTicks(currentTicks).Add(TimeSpan.FromSeconds(SeekServiceInstance.SkipAhead));
            // 2. Seek to the target (TotalMilliseconds handles the math safely)
            Masterplayer.SeekAccurate((int)targetTime.TotalMilliseconds);
            UIController.CurrentPosition = targetTime.TotalSeconds;
            // 3. Update UI based on the intended target time
            //  UIController.CurrentPosition = targetTime.TotalSeconds;
            Configuration.Helper.UI.SeekInfoService.ShowSeek(SeekServiceInstance.SkipAhead);
        }
        public static async void OpenPath(string fiPath)
        {
            CurrentMemoryStream?.Dispose();
            CurrentMemoryStream = null;
            if (File.Exists(fiPath))
            {

                if (PinService.IsPinEnabled() || PinService.IsWindowsAuthEnabled())
                {
                    RemoveFileFromExplorerHome(fiPath);
                }
                if (Masterplayer == null)
                {
                    //conf.Video.VideoProcessor = VideoProcessors.Flyleaf;
                    //conf.Video.SuperResolution = true;
                    Masterplayer = new Player();
                }
                CurrentPlayingPath = fiPath;
                FileAttributes originalAttributes = File.GetAttributes(fiPath);
                bool wasHidden = originalAttributes.HasFlag(FileAttributes.Hidden);

                try
                {
                    if (wasHidden)
                    {
                        // Temporarily strip the Hidden flag so FFmpeg can open the handle
                        File.SetAttributes(fiPath, originalAttributes & ~FileAttributes.Hidden);
                    }

                    Masterplayer.Open(fiPath);
                }
                finally
                {
                    if (wasHidden)
                    {
                        // Re-apply the Hidden flag immediately after Flyleaf grabs the file handle
                        File.SetAttributes(fiPath, originalAttributes);
                    }
                }

                // Open directly by path (recommended for Flyleaf)



                QueueService.VusicQueueNext.CollectionChanged -= QueueService.VusicQueueNext_CollectionChanged;

                QueueService.VusicQueueNext.CollectionChanged += QueueService.VusicQueueNext_CollectionChanged;

                QueueService.VusicQueue.CollectionChanged -= QueueService.VusicQueue_CollectionChanged;
                QueueService.VusicQueue.CollectionChanged += QueueService.VusicQueue_CollectionChanged;
                Masterplayer.SeekCompleted += (sender, completedMs) =>
                {
                    if (completedMs != -1)
                    {
                        if (App.MainWindowInstance != null)
                        {
                            // Safely jump back onto the WinUI 3 UI Thread
                            App.MainWindowInstance.DispatcherQueue.TryEnqueue(() =>
                            {
                                UIController.CurrentPosition = TimeSpan.FromMilliseconds(completedMs).TotalSeconds;
                            });
                        }
                    }
                };




                PlayCalled?.Invoke();
                StorageFile file = await StorageFile.GetFileFromPathAsync(fiPath);
                var musicProps = await file.Properties.GetMusicPropertiesAsync();
                if (maintimer == null)
                {
                    maintimer = new DispatcherTimer();
                    maintimer.Interval = TimeSpan.FromMilliseconds(250);
                    maintimer.Tick += Maintimer_Tick;
                }

                TimeSpan duration = musicProps.Duration;
                Debug.WriteLine("DURATION: " + duration);
                UIController.TotalDuration = duration.TotalSeconds;
                UIController.TotalDurationTemp = duration.TotalSeconds;
                string title = !string.IsNullOrWhiteSpace(musicProps.Title) ? musicProps.Title : Path.GetFileNameWithoutExtension(file.Path);

                UIController.MediaDisplayName = title;

                UIController.AlbumDisplayName = musicProps.Album;
                if (musicProps.Album == "")
                {
                    UIController.AlbumDisplayName = "Unknown Album";
                }
                UIController.ArtistDisplayName = musicProps.Artist;
                if (musicProps.Artist == "")
                {
                    UIController.ArtistDisplayName = "Unknown Artist";
                }
                string fileExtension = file.FileType.ToLowerInvariant();
                bool isAudio = false;
                if (Extensions.AudioExtensions.List.Contains(fileExtension))
                {
                    isAudio = true;
                }
                if (isAudio)
                {
                    //if (InVideoPage == true)
                    //{
                    //    if (App.UltimateFrame != null && App.RootFrameAudio != null)
                    //    {
                    //        App.UltimateFrame.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    //        App.RootFrameAudio.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    //        App.UltimateFrame.Content = null;
                    //        NavigationManager.AlreadyNavigated = false;

                    //        InVideoPage = false;
                    //    }
                    //}

                }

                var headphonedevice = Engine.Audio.Devices.FirstOrDefault(p => p.Name.Contains("Headphone"));
                var speakerdevice = Engine.Audio.Devices.FirstOrDefault(p => p.Name.Contains("Speaker"));
                if (UIController.MultiDeviceOutputFeature)
                {
                    SetupMultiAudioOutput();
                }
                //var devices = FlyleafNAudioMultiOutput.GetAvailableDevices();
                //foreach (var device in devices)
                //{
                //    Debug.WriteLine($"Name: {device.FriendlyName}");
                //    Debug.WriteLine($"ID: {device.ID}");

                //    // Inspect physical connection state (jack detection)
                //    var property = device.Properties;
                //    Debug.WriteLine($"State: {device.State}\n-------------------");
                //}
                // Select two audio endpoints (e.g., Headphones and Speakers)
                //var allDevices = FlyleafNAudioMultiOutput.GetAvailableDevices();

                //// Find Speaker and Headphone endpoints explicitly
                //var targetDevices = new List<MMDevice>();

                //var speakerDevice = allDevices.FirstOrDefault(d => d.FriendlyName.Contains("Speaker"));
                //var headphoneDevice = allDevices.FirstOrDefault(d => d.FriendlyName.Contains("Headphone"));

                //if (speakerDevice != null) targetDevices.Add(speakerDevice);
                //if (headphoneDevice != null) targetDevices.Add(headphoneDevice);

                // Initialize NAudio multi-device player
                // 2. Pass your Flyleaf Player instance to the multi-output handler

                //var multiAudioService = new FlyleafNAudioMultiOutput(Masterplayer);

                //// 3. Initialize NAudio outputs and silence Flyleaf's native output
                //multiAudioService.InitializeOutputs(targetDevices, muteFlyleafNativeAudio: true);
                //SecondaryPlayer.Audio.Device = headphonedevice;
                //Masterplayer.Audio.Device = speakerdevice;
                //if (headphonedevice != null)
                //{
                //    Engine.Audio.ToggleDevice(headphonedevice.Id, true);
                //}Masterplayer.Config.Audio.Cus
                //if (speakerdevice != null)
                //{
                //    Engine.Audio.ToggleDevice(speakerdevice.Id, true);
                //}
                //  SecondaryPlayer.Open(filestreamcurrent);

                PlayCalled?.Invoke();

                try
                {
                    var tfile = TagLib.File.Create(fiPath);
                    var image = new BitmapImage();

                    if (tfile.Tag.Pictures.Length > 0)
                    {
                        try
                        {
                            byte[] bin = tfile.Tag.Pictures[0].Data.Data;

                            // 1. Create a standard .NET MemoryStream from your byte array
                            using (var memoryStream = new System.IO.MemoryStream(bin))
                            {
                                // 2. Convert it to a WinRT IRandomAccessStream using System.IO extensions
                                using (var randomAccessStream = memoryStream.AsRandomAccessStream())
                                {
                                    // 3. Set the source safely
                                    await image.SetSourceAsync(randomAccessStream);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading metadata image: {ex.Message}");
                            image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                        }
                    }
                    else
                    {
                        image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                    }

                    UIController.CoverThumbnail = image;

                }
                catch
                {
                    var image = new BitmapImage();
                    image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                    UIController.CoverThumbnail = image;
                }
                MediaCompleted = false;
                Masterplayer.PlaybackStopped -= Masterplayer_PlaybackStopped;
                Masterplayer.PlaybackStopped += Masterplayer_PlaybackStopped;
                SaveRecents();
                if (Masterplayer.MainDemuxer.Status == FlyleafLib.MediaFramework.Status.Stopped)
                {
                    Debug.WriteLine("SJDHDH23hd");
                }
                else
                {
                    Debug.WriteLine("FSEC092" + Masterplayer.MainDemuxer.Status.ToString());
                }



                var keyConfig = Masterplayer.Config.Player.KeyBindings;

                keyConfig.Remove(Key.Left);
                keyConfig.Remove(Key.Space);
                keyConfig.Remove(Key.Right);

                keyConfig.Remove(Key.Left, ctrl: true);
                keyConfig.Remove(Key.Right, ctrl: true);
                keyConfig.Remove(Key.Left, shift: true);
                keyConfig.Remove(Key.Right, shift: true);
            }
            App.MainWindowInstance?.DispatcherQueue.TryEnqueue(() =>
            {
                var bitm = new BitmapImage(new Uri(IconHolder.Instance.Pause));
                UIController.PlayPauseToolTip = "Pause";
                UIController.Thumbnail = bitm;
                maintimer?.Start();
            });
        }

        public static float GetVolumeOfDevice(string deviceID)
        {
            if (_multiAudioEngine != null)
            {
                var floatvolume = _multiAudioEngine.GetDeviceVolume(deviceID);
                return floatvolume;
            }
            return 0;
        }
        public static void MuteDevice(string deviceId)
        {
            if (_multiAudioEngine != null)
            {
                _multiAudioEngine.SetDeviceVolume(deviceId, 0);
            }
        }
        public static void MuteDev(string deviceID)
        {

            if (_multiAudioEngine != null)
            {
                _multiAudioEngine.MuteDevice(deviceID);
            }
        }
        public static void CallJumpToTime()
        {
            JumpToTimeCalled?.Invoke();
        }
        public  static event Action? JumpToTimeCalled;
        public static void UnmuteDev(string deviceID)
        {

            if (_multiAudioEngine != null)
            {
                _multiAudioEngine.UnmuteDevice(deviceID);
            }
        }
        public static void SetVolumeOfDevice(string deviceId, float volume)
        {
            if (_multiAudioEngine != null)
            {
                _multiAudioEngine.SetDeviceVolume(deviceId, volume);
            }
        }
        public static void SetupMultiAudioOutput()
        {
            if (Masterplayer == null) return;
            // Create engine instance attached to Flyleaf player
            _multiAudioEngine = new XAudio2MultiOutputEngine(Masterplayer);

            var targetDeviceIds = new List<string>();

            // Fetch active hardware endpoints enumerated by Flyleaf
            foreach (var device in FlyleafLib.Engine.Audio.Devices)
            {
                System.Diagnostics.Debug.WriteLine($"Device Found: Name='{device.Name}', ID='{device.Id}'");
                // Exclude default placeholder ("0") to target specific physical endpoints
                if (device.Id != "0")
                {
                    targetDeviceIds.Add(device.Id);
                }
            }

            // Initialize dual/multi mastering voice routing
            if (targetDeviceIds.Count > 0)
            {
                _multiAudioEngine.InitializeOutputs(targetDeviceIds);
            }
        }
        public static void ProcessUsageInvoke()
        {
            CheckProcesses?.Invoke();
        }
        public static async void SaveRecents()
        {
            if (CurrentPlayingPath == null) return;
            if (!File.Exists(CurrentPlayingPath)) return;
            StorageFile file = await StorageFile.GetFileFromPathAsync(CurrentPlayingPath);

            string fileExtension = file.FileType.ToLowerInvariant();


            if (Extensions.VideoExtensions.List.Contains(fileExtension))
            {
                return;
            }
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recents = currentSettings.RecentMusic;
            var musicProps = await file.Properties.GetMusicPropertiesAsync();
            var existing = recents.FirstOrDefault(p => p.SongPath == CurrentPlayingPath);
            var favourites = currentSettings.Favourites;
            var existingfav = favourites.FirstOrDefault(p => p.FilePath == CurrentPlayingPath);
            if (existingfav != null)
            {
                UIController.IsFavourite = true;
            }
            else
            {
                UIController.IsFavourite = false;
            }
            foreach (var item in recents)
            {
                item.LastPlayed = "";
            }
            if (existing == null)
            {
                string songname = musicProps.Title;
                if (songname == "")
                {
                    songname = Path.GetFileNameWithoutExtension(CurrentPlayingPath);
                }
                recents.Insert(0, new Configuration.ClassModels.RecentMusicModel { SongPath = CurrentPlayingPath, LastPlayed = "(Last Played)", PlayCount = 1, SongName = songname });
            }
            else
            {
                recents.Remove(existing);
                recents.Insert(0, existing);

                existing.LastPlayed = "(Last Played)";
                existing.PlayCount++;
            }

            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        public static DispatcherTimer statsTimerRealTime = new DispatcherTimer();

        public static void Pause()
        {

            if (Masterplayer == null) return;
            Masterplayer.Pause();
            statsTimerRealTime.Stop();
            LoggedMessage?.Invoke("Pause", LogLevelType.Information);

            //CurrentPlayState?.Invoke("Paused");
            PlayPauseChanged?.Invoke();
            PlayCalled?.Invoke();

            App.MainWindowInstance?.DispatcherQueue.TryEnqueue(async () =>
            {
                var bitm = new BitmapImage(new Uri(IconHolder.Instance.Play));
                UIController.PlayPauseToolTip = "Play";

                UIController.Thumbnail = bitm;
                maintimer?.Stop();

            });
            var curTime = TimeSpan.FromTicks(Masterplayer.CurTime);
            curtime = curTime;
            curtimetemp = Masterplayer.CurTime;

            //filestreamcurrent?.Dispose();
            //JustDisposed = true;

        }
        public static Stream? CurrentMemoryStream { get; set; }
        public static bool IsMemoryStreamMode => CurrentMemoryStream != null;
        public static void Play()
        {
            if (JustDisposed == true)
            {
                GeneralInfoService.ShowInfo("Reloading video with updated properties...");
                Debug.WriteLine("disposed file stream");
                JustDisposed = false;
                if (Masterplayer == null) return;

                Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
                if (Masterplayer == null) return;
                Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;

                if (IsMemoryStreamMode && CurrentMemoryStream != null)
                {
                    // Rewind the in-memory stream to the start before handing to demuxer
                    if (CurrentMemoryStream.CanSeek)
                    {
                        CurrentMemoryStream.Position = 0;
                    }

                    Debug.WriteLine("Re-opening from in-memory stream");
                    Masterplayer.Open(CurrentMemoryStream);
                    LoggedMessage?.Invoke("Player Status: Playing", LogLevelType.Information);

                    PlayCalled?.Invoke();
                    App.MainWindowInstance?.DispatcherQueue.TryEnqueue(() =>
                    {
                        var bitm = new BitmapImage(new Uri(IconHolder.Instance.Pause));
                        UIController.PlayPauseToolTip = "Pause";
                        UIController.Thumbnail = bitm;
                        maintimer?.Start();
                    });
                }
                else if (!string.IsNullOrEmpty(CurrentPlayingPath) && System.IO.File.Exists(CurrentPlayingPath))
                {
                    Debug.WriteLine("Re-opening from file path: " + CurrentPlayingPath);
                    OpenPath(CurrentPlayingPath);

                }
                return;
            }
            if (Masterplayer == null) return;
            if (MediaCompleted == true)
            {
                Masterplayer.CurTime = 0;
            }
            if (CurrentPlayingPath == "") return;
            if (!File.Exists(CurrentPlayingPath))
            {
                UIController.ErrorMessage = "The file path is unavailable or access to it is denied. Please reopen the file to continue playing it.";
                LoggedMessage?.Invoke("Unexpected error: Failed/Stopped Demuxer", LogLevelType.Error);
                ErrorCalled?.Invoke();

                Logger.Log("Unexpected error: Failed/Stopped Demuxer", "PlayerService.Play", Logger.LogLevelType.Error);
                //Masterplayer.Stop();
                // filestreamcurrent?.Dispose();
                JustDisposed = true;

                return;
            }
            if (Masterplayer.MainDemuxer.Status == FlyleafLib.MediaFramework.Status.Stopped)
            {
                Debug.WriteLine("DFIH FAILURE");
                UIController.ErrorMessage = "The file path is unavailable or access to it is denied. Please reopen the file to continue playing it.";
                LoggedMessage?.Invoke("Unexpected error: Failed/Stopped Demuxer", LogLevelType.Error);
                ErrorCalled?.Invoke();

                Logger.Log("Unexpected error: Failed/Stopped Demuxer", "PlayerService.Play", Logger.LogLevelType.Error);
                //Masterplayer.Stop();
                // filestreamcurrent?.Dispose();
                JustDisposed = true;

                return;
            }
            PlayPauseChanged?.Invoke();
            statsTimerRealTime.Start();
            Masterplayer.Play();
            LoggedMessage?.Invoke("Player Status: Playing", LogLevelType.Information);

            PlayCalled?.Invoke();
            App.MainWindowInstance?.DispatcherQueue.TryEnqueue(() =>
            {
                var bitm = new BitmapImage(new Uri(IconHolder.Instance.Pause));
                UIController.PlayPauseToolTip = "Pause";
                UIController.Thumbnail = bitm;
                maintimer?.Start();
            });
            NavigateToVideoPage();
        }

        private static void OceanContentDialog_PrimaryRequested()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        public static XamlRoot? mainXamlRoot;
        private static async void NavigateToVideoPage()
        {

            if (Masterplayer == null) return;
            if (!File.Exists(CurrentPlayingPath)) return;
            var file = await StorageFile.GetFileFromPathAsync(CurrentPlayingPath);

            string fileExtension = file.FileType.ToLowerInvariant();
            bool isVideo = false;
            if (Extensions.VideoExtensions.List.Contains(fileExtension))
            {
                isVideo = true;
            }
            if (isVideo)
            {
                if (App.NavigationFrame != null)
                {
                    var videoprogress = new VideoProgress { FilePath = CurrentPlayingPath, CurrentDuration = Masterplayer.CurTime, TotalDuration = Masterplayer.Duration, ShowInformationOfOpen = false };
                    ContinuePlaying.videoProgressMain = videoprogress;
                    if (InVideoPage == false)
                    {
                        Debug.WriteLine("TRUEU");
                        App.NavigationFrame.Navigate(typeof(VideoPlayer), true);
                    }

                }
            }
        }
        public static bool isEpisodeVid = false;
        private static void Masterplayer_OpenCompleted1(object? sender, OpenCompletedArgs e)
        {
            Debug.WriteLine("SKHDHD");
            if (Masterplayer == null) return;
            Masterplayer.SeekAccurate((int)(curtimetemp / 10000));
            Debug.WriteLine("DONGD " + curtime.TotalSeconds);
            var curTime = TimeSpan.FromTicks(Masterplayer.CurTime);
            UIController.CurrentPosition = curTime.TotalSeconds;
            Debug.WriteLine("DONGDd " + TimeSpan.FromSeconds(curtime.TotalSeconds).ToString(@"hh\:mm\:ss"));
            GeneralInfoService.ShowInfo($"Opened playback of '{CurrentPlayingPath}' at {TimeSpan.FromSeconds(curTime.TotalSeconds).ToString(@"hh\:mm\:ss")}");

            curtime = TimeSpan.Zero;
            curtimetemp = 0;

            Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;

        }
        public static bool DONTSHOWGENERALINFORMATION = false;
        public static async void LookForProgressForNextVideo(string Path)
        {
            if (Masterplayer == null)
            {
                Masterplayer = new Player();
            }
            Debug.WriteLine("Looking for video Progress");
            Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var videoprogress = currentSettings.SavedVideoProgress;

            var FileToBePlayed = Path;
            Debug.WriteLine("Looking for video Progress of " + FileToBePlayed);
            var exist = videoprogress.FirstOrDefault(p => p.FilePath == FileToBePlayed);
            if (exist != null)
            {
                Debug.WriteLine(exist.FileName + " found " + exist.CurrentDuration);
                curtimetemp = (long)exist.CurrentDuration;
                //           PlayerService.Masterplayer.SeekAccurate((int)(vdprg.CurrentDuration / 10000));
                DONTSHOWGENERALINFORMATION = true;
                Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
            }
            OpenPath(Path);
        }
        private static async void Masterplayer_PlaybackStopped(object? sender, PlaybackStoppedArgs e)
        {
            Debug.WriteLine("FINISHED1");
            Debug.WriteLine(UIController.TotalDurationString);
            Debug.WriteLine(UIController.RunningDurationString);
            if (Masterplayer != null)
            {
                if (Masterplayer.Status == Status.Ended)
                {
                    MediaCompleted = true;
                    Debug.WriteLine("FINISHED");

                    App.MainWindowInstance?.DispatcherQueue.TryEnqueue(async () =>
                    {
                        var bitm = new BitmapImage(new Uri(IconHolder.Instance.Play));

                        UIController.CurrentPosition = 0;
                        UIController.PlayPauseToolTip = "Play";
                        UIController.Thumbnail = bitm;
                        maintimer?.Stop();

                        // QueueService.PlayNext();
                    });


                    return;
                }
            }
            if (UIController.TotalDurationString == UIController.RunningDurationString)
            {

            }
        }

        public static void SldMain_DragStarted()
        {
            if (Masterplayer == null) return;
            _isDragging = true;
            maintimer?.Stop();
        }
        public static event Action<int>? SeekCompleted;
        public static void SldMain_DragCompleted(OceanSlider slider)
        {
            if (Masterplayer == null) return;

            if (Masterplayer.MainDemuxer.Status == FlyleafLib.MediaFramework.Status.Stopped)
            {
                Debug.WriteLine("DFIH FAILURE");
                UIController.ErrorMessage = "The file path is unavailable or access to it is denied. Please reopen the file to continue playing it.";

                ErrorCalled?.Invoke();
                //   Masterplayer.Stop();
                //filestreamcurrent?.Dispose();
                JustDisposed = true;
                return;
            }
            if (JustDisposed == true)
            {
                curtime = TimeSpan.FromSeconds(slider.Value);
                curtimetemp = TimeSpan.FromSeconds(slider.Value).Ticks;
                JustDisposed = false;
            }

            int targetMs = (int)TimeSpan.FromSeconds(slider.Value).TotalMilliseconds;
            SeekCompleted?.Invoke(targetMs);
            // FIX 2: Use SeekAccurate so it doesn't roll back to the nearest keyframe
            Masterplayer.SeekAccurate(targetMs);

            _isDragging = false;
            maintimer?.Start();
        }

    }
}
