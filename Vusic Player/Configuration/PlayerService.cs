using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Devices.Spi;
using Windows.Storage;
using Windows.UI;
using Logger = Vusic_Player.Configuration.AppConfig.Logger;

namespace Vusic_Player.Configuration
{
    public class PlayerService
    {
        public static Player? Masterplayer { get; set; }
        public static bool MediaCompleted { get; set; }
        public static event Action? PlayPauseChanged;


        public static MediaPlaybackController UIController => MediaPlaybackController.Instance;
        public static int? originalvolume;
        public static string currentvol = "0";

        public static DispatcherTimer? maintimer { get; set; }
        public static FileStream? filestreamcurrent;
        public static string CurrentPlayingPath { get; set; } = "";

        public static event Action? OnVideoCalled;
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
            if (PlayerService.CurrentPlayingPath == "" || PlayerService.CurrentPlayingPath == null)
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

            if (Masterplayer.IsPlaying) PlayerService.Pause();
            else PlayerService.Play();
        }

        public static void SeekBefore()
        {
            if (Masterplayer == null) return;
            long currentMs = Masterplayer.CurTime / 10000;


            int targetMs = (int)(currentMs - 10000);
            PlayerService.Masterplayer.SeekAccurate(targetMs);
            var curTime = TimeSpan.FromTicks(Masterplayer.CurTime);
            UIController.CurrentPosition = curTime.TotalSeconds;
            //Helper.SeekInfoService.ShowSeek(-10);
        }
        public static void SeekAhead()
        {
            if (PlayerService.Masterplayer == null) return;
            long currentMs = PlayerService.Masterplayer.CurTime / 10000;


            int targetMs = (int)(currentMs + 10000);
            PlayerService.Masterplayer.SeekAccurate(targetMs);
            var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
            UIController.CurrentPosition = curTime.TotalSeconds;
            //   Helper.SeekInfoService.ShowSeek(10);
        }
        public static async void OpenPath(string fiPath)
        {
            if (File.Exists(fiPath))
            {
                if (Masterplayer == null)
                {
                    //conf.Video.VideoProcessor = VideoProcessors.Flyleaf;
                    //conf.Video.SuperResolution = true;
                    Masterplayer = new Player();
                    QueueService.VusicQueueNext.CollectionChanged -= QueueService.VusicQueueNext_CollectionChanged;

                    QueueService.VusicQueueNext.CollectionChanged += QueueService.VusicQueueNext_CollectionChanged;

                    QueueService.VusicQueue.CollectionChanged -= QueueService.VusicQueue_CollectionChanged;
                    QueueService.VusicQueue.CollectionChanged += QueueService.VusicQueue_CollectionChanged;

                }
                CurrentPlayingPath = fiPath;
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
                UIController.TotalDuration = duration.TotalSeconds;
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

                filestreamcurrent = new FileStream(fiPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                Masterplayer.Open(filestreamcurrent);
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
                if (Masterplayer != null)
                {
                    var keyConfig = Masterplayer.Config.Player.KeyBindings;

                    keyConfig.Remove(Key.Left);
                    keyConfig.Remove(Key.Right);

                    keyConfig.Remove(Key.Left, ctrl: true);
                    keyConfig.Remove(Key.Right, ctrl: true);
                    keyConfig.Remove(Key.Left, shift: true);
                    keyConfig.Remove(Key.Right, shift: true);
                }
                App.MainWindowInstance?.DispatcherQueue.TryEnqueue(() =>
                {
                    var bitm = new BitmapImage(new Uri("ms-appx:///Assets/pause.png"));
                    UIController.PlayPauseToolTip = "Pause";
                    UIController.Thumbnail = bitm;
                    maintimer?.Start();
                });
            }
        }

        private static void Masterplayer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Debug.WriteLine("SJHFUH");
            //if (Masterplayer != null)
            //{
            //    if (Masterplayer.Status == Status.Failed || Masterplayer.Status == Status.Stopped)
            //    {
            //        Debug.WriteLine("FATAL ERROR:");
            //        filestreamcurrent?.Dispose();
            //        JustDisposed = true;
            //        Masterplayer.Stop();
            //    }
            //}
        }

        public static void ProcessUsageInvoke()
        {
            CheckProcesses?.Invoke();
        }
        public static async void SaveRecents()
        {
            if (CurrentPlayingPath == null) return;
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
        public static void Pause()
        {

            if (Masterplayer == null) return;
            Masterplayer.Pause();
            //CurrentPlayState?.Invoke("Paused");
            PlayPauseChanged?.Invoke();
            PlayCalled?.Invoke();

            App.MainWindowInstance?.DispatcherQueue.TryEnqueue(async () =>
            {
                var bitm = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
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
                if (System.IO.File.Exists(CurrentPlayingPath))
                {
                    Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
                    Debug.WriteLine("CALLEDS");
                    if (Masterplayer.MainDemuxer.Status == FlyleafLib.MediaFramework.Status.Stopped)
                    {
                        Debug.WriteLine("SJDHDH223");
                    }
                    else
                    {
                        Debug.WriteLine("FSEC2" + Masterplayer.MainDemuxer.Status.ToString());
                    }
                    OpenPath(CurrentPlayingPath);
                    if (Masterplayer.MainDemuxer.Status == FlyleafLib.MediaFramework.Status.Stopped)
                    {
                        Debug.WriteLine("SJDHDH");
                    }
                    else
                    {
                        Debug.WriteLine( "FSEC"+ Masterplayer.MainDemuxer.Status.ToString());
                    }
                }
                return;
            }
            if (Masterplayer == null) return;
            if (MediaCompleted == true)
            {
                Masterplayer.CurTime = 0;
            }
            if (CurrentPlayingPath == "") return;
            if (Masterplayer.MainDemuxer.Status == FlyleafLib.MediaFramework.Status.Stopped)
            {
                Debug.WriteLine("DFIH FAILURE");
                UIController.ErrorMessage = "The file path is unavailable or access to it is denied. Please reopen the file to continue playing it.";

                ErrorCalled?.Invoke();

                Logger.Log("Unexpected error: Failed/Stopped Demuxer", "PlayerService.Play", Logger.LogLevelType.Error);
                //Masterplayer.Stop();
                // filestreamcurrent?.Dispose();
                JustDisposed = true;

                return;
            }
            PlayPauseChanged?.Invoke();
            Masterplayer.Play();
            PlayCalled?.Invoke();
            App.MainWindowInstance?.DispatcherQueue.TryEnqueue(() =>
            {
                var bitm = new BitmapImage(new Uri("ms-appx:///Assets/pause.png"));
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
        private static void Masterplayer_OpenCompleted1(object? sender, OpenCompletedArgs e)
        {
            if (Masterplayer == null) return;
            Masterplayer.SeekAccurate((int)(curtimetemp / 10000));
            UIController.CurrentPosition = curtime.TotalSeconds;
            curtime = TimeSpan.Zero;
            curtimetemp = 0;

            Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;

        }
        public static async void LookForProgressForNextVideo(string Path)
        {
            if (Masterplayer == null) return;
            Debug.WriteLine("Looking for video Progress");
            Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var videoprogress = currentSettings.SavedVideoProgress;

            var FileToBePlayed = Path;
            Debug.WriteLine("Looking for video Progress of " + FileToBePlayed);
            var exist = videoprogress.FirstOrDefault(p => p.FilePath == FileToBePlayed);
            if (exist != null)
            {
                curtimetemp = (long)exist.CurrentDuration;
                //           PlayerService.Masterplayer.SeekAccurate((int)(vdprg.CurrentDuration / 10000));

                Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
            }

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
                        var bitm = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));

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

            Masterplayer.CurTime = TimeSpan.FromSeconds(slider.Value).Ticks;

            _isDragging = false;
            maintimer?.Start();
        }

    }
}
