using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Vusic_Player.Configuration.Helper
{
    public class FileInfo
    {
        public static string filepathmaster = "";
        public static event Action? RefreshValues;
        public static void RefreshCall()
        {
            RefreshValues?.Invoke();
        }
        public static MediaPlaybackController media => MediaPlaybackController.Instance;
        public static async void LoadFileInfo(string FilePath, XamlRoot Xamlroot)
        {
            filepathmaster = FilePath;
            media.AlbumArt = new BitmapImage(new Uri("ms-appx:///Assets/appicon.png"));
            if (App.MainWindowInstance == null) return;
            //Handle file not exist
            if (File.Exists(FilePath))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(FilePath);
                MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();
                media.Title = properties.Title;
                media.FileName = Path.GetFileNameWithoutExtension(file.Path);
                media.Year = properties.Year.ToString();
                media.ArtistNameInfo = properties.Artist;
                media.AlbumNameInfo = properties.Album;

                media.TrackNumber = properties.TrackNumber.ToString();
                media.Duration = properties.Duration.ToString(@"hh\:mm\:ss");
                media.Bitrate = (properties.Bitrate / 1000).ToString() + " kbps";
                string[] propertyKeys = new string[]
            {
    "System.Audio.SampleRate",
    "System.Audio.ChannelCount"
            };

                // Retrieve the extra properties
                var extraProperties = await file.Properties.RetrievePropertiesAsync(propertyKeys);

                // 1. Handle Sample Rate
                if (extraProperties.TryGetValue("System.Audio.SampleRate", out object? srValue))
                {
                    uint sampleRate = (uint)srValue;
                    media.SampleRate = (sampleRate / 1000.0).ToString("0.0") + " kHz";
                }

                // 2. Handle Channels (Stereo/Mono)
                if (extraProperties.TryGetValue("System.Audio.ChannelCount", out object? chValue))
                {
                    uint channels = (uint)chValue;
                    // Display as "2 Channels" or map it to "Stereo"
                    media.Channels = channels == 2 ? "Stereo" : $"{channels} Channels";
                }
                else
                {
                    media.Channels = "Unknown";
                }
                var musicProps = await file.Properties.GetMusicPropertiesAsync();

                // 1. Composers (Returns IList<string>)
                if (musicProps.Composers.Count > 0)
                {
                    media.Composers = string.Join(", ", musicProps.Composers);
                }
                else
                {
                    media.Composers = "Unknown Composer";
                }

                // 2. Conductors (Returns IList<string>)
                if (musicProps.Conductors.Count > 0)
                {
                    media.Conductors = string.Join(", ", musicProps.Conductors);
                }
                else
                {
                    media.Conductors = "N/A";
                }
                media.FilePath = file.Path;
                media.FileType = Path.GetExtension(file.Path);
                if (PlayerService.Masterplayer != null)
                {
                //    media.Speed = PlayerService.Masterplayer.Speed.ToFFmpegFormat(1) + "x";
                }
                else
                {
                    media.Speed = "1.0x";
                }

                // 1. Genre (Returns IList<string>)
                if (musicProps.Genre.Count > 0)
                {
                    media.Genre = string.Join(", ", musicProps.Genre);
                }
                else
                {
                    media.Genre = "Unknown Genre";
                }



                var filetag = TagLib.File.Create(file.Path);
                media.Comments = filetag.Tag.Comment;
                if (filetag.Tag.Performers != null)
                {
                    media.ContributingArtists = string.Join(", ", filetag.Tag.Performers);
                }
                var tfile = TagLib.File.Create(file.Path);
                var id3v2Tag = tfile.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
                int starDisplay = 0;

                if (id3v2Tag != null)
                {
                    var frame = TagLib.Id3v2.PopularimeterFrame.Get(id3v2Tag, "no@email", false);
                    if (frame != null)
                    {
                        // Direct conversion logic for 0-5 stars
                        starDisplay = frame.Rating switch
                        {
                            >= 243 => 5,
                            >= 182 => 4,
                            >= 114 => 3,
                            >= 49 => 2,
                            >= 1 => 1,
                            _ => 0
                        };
                    }
                }

                media.Rating = starDisplay;


                var image = new BitmapImage();

                if (tfile.Tag.Pictures.Length > 0)
                {
                    try
                    {
                        // 2. Get the raw bytes from the first picture
                        byte[] bin = tfile.Tag.Pictures[0].Data.Data;

                        // 3. Use an InMemoryRandomAccessStream to convert bytes to a WinUI-friendly source
                        using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                        {
                            using (var writer = new Windows.Storage.Streams.DataWriter(stream.GetOutputStreamAt(0)))
                            {
                                writer.WriteBytes(bin);
                                await writer.StoreAsync();
                            }

                            // Move back to the beginning of the stream before reading
                            stream.Seek(0);
                            await image.SetSourceAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to load album art: {ex.Message}");
                        // Fallback in case the image data is corrupted
                        image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                    }
                }
                else
                {
                    // 4. Fallback: No picture found in the metadata tags
                    image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                }

                media.AlbumArt = image;

                media.DateCreated = file.DateCreated.ToString("G");
                var basicProps = await file.GetBasicPropertiesAsync();
                media.DateModified = basicProps.DateModified.ToString("G");
                media.FileSize = FormatFileSize(basicProps.Size);
                media.Genre = musicProps.Genre.Any() ? string.Join("; ", musicProps.Genre) : "Unknown Genre";
            }

            OceanContentDialog.Show($"File Info - {media.FileName}", "Save Properties", "", "Close", OceanDialogWindow.ContentType.FileInformation, OceanContentDialogDefault.Primary, Xamlroot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "saveicon", "", "", new System.Collections.ObjectModel.ObservableCollection<Configuration.ClassModels.SongModel>(), "");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;

            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }
        private static void OceanContentDialog_PrimaryRequested()
        {
            if (_isClosing) return;
            _isClosing = true;
            try
            {
                UpdateAllFileProperties(filepathmaster);
            }
            finally
            {
                _isClosing = false;
            }
        }
        public event EventHandler<string>? FileInfoRequested;

        public void RaiseFileInfoRequested(string path)
        {
            if (FileInfoRequested == null)
            {
                Debug.WriteLine("NULLMUNDA");
            }
            FileInfoRequested?.Invoke(this, path);
        }
        public static event Action<string>? FileInfoCalled;
        public static void TriggerFileInfoDialog(string filepath)
        {
            if (FileInfoCalled == null)
            {
                Debug.WriteLine("NULLMUNDA");
            }
            FileInfoCalled?.Invoke(filepath);
            Debug.WriteLine("CHEHE1");
        }
        private static bool _isClosing = false;
        private static long curtimetemp;
        private static TimeSpan curtime;
        public static void UpdateAllFileProperties(string path)
        {

            var FilePath = path;
            if (FilePath == null) return;
            if (media.FileName != Path.GetFileNameWithoutExtension(FilePath))
            {
                string newFileName = media.FileName;
                string extension = Path.GetExtension(FilePath);
                string directory = Path.GetDirectoryName(FilePath)!;
                string newPath = Path.Combine(directory, newFileName + extension);

                // Check if destination exists BEFORE we lose the ability to go back
                if (File.Exists(newPath))
                {
                    PlayerService.FileRenameIssue = true;
                    PlayerService.ProcessUsageInvoke();
                    Debug.WriteLine("alreadyexists");
                    return;
                }
                else
                {
                    File.Move(FilePath, newPath);
                    PlayerService.CurrentPlayingPath = newPath;
                    UpdateFileMetadata(path);
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                }
            }
            else
            {
                Debug.WriteLine("NO PATH HAS BEEN CHANGED. ALERT!");
                Debug.WriteLine(path);
                UpdateFileMetadata(path);
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();
            }
            RefreshCall();
        }

        private static void Masterplayer_OpenCompleted1(object? sender, OpenCompletedArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.SeekAccurate((int)(curtimetemp / 10000));
            media.CurrentPosition = curtime.TotalSeconds;
            curtime = TimeSpan.Zero;
            curtimetemp = 0;
            PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
        }
        public static void SetValues(string path, bool isCurrentlyPlayingFile)
        {
            Debug.WriteLine("This called");
            if (File.Exists(path))
            {
                Debug.WriteLine("THIS CALLED: " + path);
                using (var tfile = TagLib.File.Create(path))
                {
                    tfile.Tag.Title = media.Title;

                    tfile.Tag.Genres = [media.Genre];

                    tfile.Tag.AlbumArtists = [media.ArtistNameInfo];

                    tfile.Tag.Album = media.AlbumNameInfo;

                    tfile.Tag.Performers = [media.ContributingArtists];

                    if (uint.TryParse(media.TrackNumber, out uint trackNum))

                    {
                        tfile.Tag.Track = trackNum;
                    }

                    else

                    {

                        tfile.Tag.Track = 0;

                    }

                    if (uint.TryParse(media.Year, out uint yearNum))

                    {

                        tfile.Tag.Year = yearNum;

                    }

                    else

                    {

                        tfile.Tag.Year = (uint)DateTime.Now.Year;

                    }
                    if (File.Exists(path))
                    {
                        if (File.Exists(media.AlbumArtFile))
                        {
                            var Picture = new TagLib.Picture(media.AlbumArtFile);
                            Picture.Type = TagLib.PictureType.FrontCover;
                            Picture.MimeType = "image/png";
                            Picture.Description = "Album Art";
                            tfile.Tag.Pictures = new TagLib.IPicture[] { Picture };
                        }
                    }
                    tfile.Tag.Composers = [media.Composers];

                    tfile.Tag.Conductor = media.Conductors;

                    tfile.Tag.Comment = media.Comments;

                    tfile.Save();

                }
                if (isCurrentlyPlayingFile)
                {
                    if (PlayerService.Masterplayer == null) return;
                    if (File.Exists(PlayerService.CurrentPlayingPath))
                    {
                        PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
                        PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
                        PlayerService.OpenPath(PlayerService.CurrentPlayingPath);
                    }
                }
            }

        }
        public static async void UpdateFileMetadata(string path)
        {
            if (path == null) return;
            string FilePath = path;
            var filelocked = GetLockingProcess.GetLockingProcesses(FilePath);
            if (filelocked.Count > 0)
            {
                bool onlyVusicPlayer = filelocked.All(p => p.ProcessName == "Vusic Player");

                if (onlyVusicPlayer)
                {
                    if (PlayerService.Masterplayer != null)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                        curtime = curTime;
                        curtimetemp = PlayerService.Masterplayer.CurTime;

                        PlayerService.filestreamcurrent?.Dispose();
                        var filelocked2 = GetLockingProcess.GetLockingProcesses(FilePath);

                        try
                        {

                            if (filelocked2.Count == 0)
                            {
                                SetValues(FilePath, true);
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Rename failed: {ex.Message}");
                            if (PlayerService.filestreamcurrent == null)
                            {
                                if (FilePath != null)
                                    PlayerService.OpenPath(FilePath);
                            }
                        }
                    }
                }
                else
                {
                    PlayerService.FileRenameIssue = false;
                    PlayerService.processlocklist = filelocked;
                    PlayerService.ProcessUsageInvoke();
                    return;
                }
            }
            else
            {
                Debug.WriteLine("Other case: " + FilePath);
                var filelocked2 = GetLockingProcess.GetLockingProcesses(FilePath);


                //try
                //{
                Debug.WriteLine(FilePath);
                if (filelocked2.Count == 0)
                {
                    Debug.WriteLine("Check1");
                    SetValues(FilePath, false);
                }

                //}
                //catch (Exception ex)
                //{
                //  Debug.WriteLine($"Rename failed: {ex.Message}");
                //if (PlayerService.filestreamcurrent == null)
                //{
                //    if (FilePath != null)
                //        PlayerService.OpenPath(FilePath);
                //}
            }
        }


        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern uint StrFormatByteSize(ulong qdw, System.Text.StringBuilder pszBuf, uint cchBuf);

        public static string FormatFileSize(ulong bytes)
        {
            var sb = new System.Text.StringBuilder(32);
            StrFormatByteSize(bytes, sb, (uint)sb.Capacity);
            return sb.ToString();
        }
    }

}
