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
using Vusic_Player.Configuration.Helper.VideoProperties;
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
                string fileExtension = file.FileType.ToLowerInvariant();

                media.FileName = Path.GetFileNameWithoutExtension(file.Path);

                if (Extensions.AudioExtensions.List.Contains(fileExtension))
                {
                    // ==========================================
                    // AUDIO METADATA LOADING (Fast & Consolidated)
                    // ==========================================
                    MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();

                    media.Title = properties.Title;
                    media.Year = properties.Year > 0 ? properties.Year.ToString() : "";
                    media.ArtistNameInfo = properties.Artist;
                    media.AlbumNameInfo = properties.Album;
                    media.Bitrate = (properties.Bitrate / 1000).ToString() + " kbps";
                    media.TrackNumber = properties.TrackNumber.ToString();
                    media.Duration = properties.Duration.ToString(@"hh\:mm\:ss");

                    // Fetch extra audio properties in a single combined batch
                    string[] propertyKeys = new string[] { "System.Audio.SampleRate", "System.Audio.ChannelCount" };
                    var extraProperties = await file.Properties.RetrievePropertiesAsync(propertyKeys);

                    if (extraProperties.TryGetValue("System.Audio.SampleRate", out object? srValue) && srValue != null)
                    {
                        uint sampleRate = (uint)srValue;
                        media.SampleRate = (sampleRate / 1000.0).ToString("0.0") + " kHz";
                    }

                    if (extraProperties.TryGetValue("System.Audio.ChannelCount", out object? chValue) && chValue != null)
                    {
                        uint channels = (uint)chValue;
                        media.Channels = channels == 2 ? "Stereo" : $"{channels} Channels";
                    }
                    else
                    {
                        media.Channels = "Unknown";
                    }

                    media.Composers = properties.Composers.Count > 0 ? string.Join(", ", properties.Composers) : "Unknown Composer";
                    media.Conductors = properties.Conductors.Count > 0 ? string.Join(", ", properties.Conductors) : "N/A";

                    // Use TagLib safely to extract strings, disposing of the handle instantly
                    using (var filetag = TagLib.File.Create(file.Path))
                    {
                        media.Comments = filetag.Tag.Comment;
                        if (filetag.Tag.Performers != null)
                        {
                            media.ContributingArtists = string.Join(", ", filetag.Tag.Performers);
                        }

                        // If Windows shell missed the audio year, let TagLib back it up
                        if (string.IsNullOrEmpty(media.Year) || media.Year == "0")
                        {
                            media.Year = filetag.Tag.Year > 0 ? filetag.Tag.Year.ToString() : "";
                        }
                        media.Genre = filetag.Tag.Genres.Length > 0
    ? string.Join("; ", filetag.Tag.Genres)
    : "Unknown Genre";
                    }

                    media.AudioMetadataVisibilityFileInfo = Visibility.Visible;
                    media.VideoMetadataVisibilityFileInfo = Visibility.Collapsed;
                }
                else if (Extensions.VideoExtensions.List.Contains(fileExtension))
                {
                    // ==========================================
                    // VIDEO METADATA LOADING (Safe & Independent)
                    // ==========================================
                    media.AudioMetadataVisibilityFileInfo = Visibility.Collapsed;
                    media.VideoMetadataVisibilityFileInfo = Visibility.Visible;

                    using (var filetag = TagLib.File.Create(file.Path))
                    {
                        Debug.WriteLine("THE TITLE IS " + filetag.Tag.Title);
                        string? rawTitle = null;

                        // Look into low-level MKV structures first
                        if (filetag.GetTag(TagLib.TagTypes.Xiph) is TagLib.Ogg.XiphComment xiphTag)
                        {
                            rawTitle = xiphTag.GetField("TITLE")?.FirstOrDefault();
                        }

                        // If empty, look into low-level MP4 structures
                        if (string.IsNullOrEmpty(rawTitle) && filetag.GetTag(TagLib.TagTypes.Apple) is TagLib.Mpeg4.AppleTag appleTag)
                        {
                            // Apple tag layout sometimes maps name directly
                            rawTitle = appleTag.Title;
                        }

                        // If still empty, check legacy ID3v2 structures
                        if (string.IsNullOrEmpty(rawTitle) && filetag.GetTag(TagLib.TagTypes.Id3v2) is TagLib.Id3v2.Tag id3Tag)
                        {
                            rawTitle = id3Tag.Title;
                        }

                        // Final Fallback chain: Container Tag -> File Name
                        media.Title = !string.IsNullOrEmpty(rawTitle) ? rawTitle
                                     : (!string.IsNullOrEmpty(filetag.Tag.Title) ? filetag.Tag.Title
                                     : media.FileName);
                        media.Year = filetag.Tag.Year > 0 ? filetag.Tag.Year.ToString() : "";
                        media.Comments = filetag.Tag.Comment;
                        media.Genre = filetag.Tag.Genres.Length > 0 ? string.Join("; ", filetag.Tag.Genres) : "Unknown Genre";
                        media.Duration = filetag.Properties.Duration.ToString(@"hh\:mm\:ss");



                    }

                    var fileinfo = await VideoMetadata.GetVideoMetadata(file.Path);
                    media.Codec = fileinfo.Codec;
                    media.FrameRate = fileinfo.FrameRate + " FPS";
                    media.DisplayResolution = fileinfo.DisplayResolution;
                }
                // Retrieve the extra properties

                // 1. Handle Sample Rate


                // ... (Keep your top video/audio tag parsing blocks completely as they are) ...

                media.FilePath = file.Path;
                media.FileType = Path.GetExtension(file.Path);
                media.Speed = PlayerService.Masterplayer != null ? media.SpeedValue + "x" : "1.0x";

                var image = new BitmapImage();

                // CONSOLIDATE: Open tfile ONCE inside a using block for the remaining metadata properties
                using (var tfile = TagLib.File.Create(file.Path))
                {
                    // 1. Handle Ratings via ID3v2 safely
                    int starDisplay = 0;
                    if (tfile.GetTag(TagLib.TagTypes.Id3v2) is TagLib.Id3v2.Tag id3v2Tag)
                    {
                        var frame = TagLib.Id3v2.PopularimeterFrame.Get(id3v2Tag, "no@email", false);
                        if (frame != null)
                        {
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

                    // 2. Extract Album Art from the same open file handle
                    if (tfile.Tag.Pictures.Length > 0)
                    {
                        try
                        {
                            byte[] bin = tfile.Tag.Pictures[0].Data.Data;
                            using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                            {
                                using (var writer = new Windows.Storage.Streams.DataWriter(stream.GetOutputStreamAt(0)))
                                {
                                    writer.WriteBytes(bin);
                                    await writer.StoreAsync();
                                }
                                stream.Seek(0);
                                await image.SetSourceAsync(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to load album art: {ex.Message}");
                            image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                        }
                    }
                    else
                    {
                        image.UriSource = new Uri("ms-appx:///Assets/appicon.png");
                    }
                } // Handle is cleanly closed and unlocked right here!

                media.AlbumArt = image;

                // 3. File System Timestamps
                media.DateCreated = file.DateCreated.ToString("G");
                var basicProps = await file.GetBasicPropertiesAsync();
                media.DateModified = basicProps.DateModified.ToString("G");
                media.FileSize = FormatFileSize(basicProps.Size);
            }

            // FIX: Change {media.FileName} to {media.Title} so your UI actually renders the updated Tag Title!
            OceanContentDialog.Show($"File Info - {media.Title}", "Save Properties", "", "Close", OceanDialogWindow.ContentType.FileInformation, OceanContentDialogDefault.Primary, Xamlroot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "saveicon", "", "", new System.Collections.ObjectModel.ObservableCollection<Configuration.ClassModels.SongModel>(), "");
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
        public static string JustUpdatedRenamePath = "";
        public static void UpdateAllFileProperties(string path)
        {

            var FilePath = path;
            if (FilePath == null) return;
            if (media.FileName != Path.GetFileNameWithoutExtension(FilePath))
            {
                Debug.WriteLine("Rename initiated");
                string newFileName = media.FileName;
                string extension = Path.GetExtension(FilePath);
                string directory = Path.GetDirectoryName(FilePath)!;
                string newPath = Path.Combine(directory, newFileName + extension);
                Debug.WriteLine("Rename continuing");

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
                    Debug.WriteLine("Rename finalizing: from " + FilePath + " to " + newPath);

                    File.Move(FilePath, newPath);
                    JustUpdatedRenamePath = newPath;
                    if (PlayerService.CurrentPlayingPath != "")
                    {
                        PlayerService.CurrentPlayingPath = newPath;
                    }
                    Debug.WriteLine("Rename success");

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
                JustUpdatedRenamePath = path;
                using (var tfile = TagLib.File.Create(path))
                {
                    Debug.WriteLine("NEW TITLE IS " + tfile.Tag.Title);
                }
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
        //public static void SetValues(string path, bool isCurrentlyPlayingFile)
        //{
        //    Debug.WriteLine("This called");
        //    if (File.Exists(path))
        //    {
        //        Debug.WriteLine("THIS CALLED: " + path);
        //        using (var tfile = TagLib.File.Create(path))
        //        {
        //            tfile.Tag.Title = media.Title;

        //            tfile.Tag.Genres = [media.Genre];

        //            tfile.Tag.AlbumArtists = [media.ArtistNameInfo];

        //            tfile.Tag.Album = media.AlbumNameInfo;

        //            tfile.Tag.Performers = [media.ContributingArtists];

        //            if (uint.TryParse(media.TrackNumber, out uint trackNum))

        //            {
        //                tfile.Tag.Track = trackNum;
        //            }

        //            else

        //            {

        //                tfile.Tag.Track = 0;

        //            }

        //            if (uint.TryParse(media.Year, out uint yearNum))

        //            {
        //                Debug.WriteLine("YEAR BEING CHANGED " + yearNum);
        //                tfile.Tag.Year = yearNum;

        //            }

        //            else

        //            {

        //                tfile.Tag.Year = (uint)DateTime.Now.Year;

        //            }
        //            if (File.Exists(path))
        //            {
        //                if (File.Exists(media.AlbumArtFile))
        //                {
        //                    var Picture = new TagLib.Picture(media.AlbumArtFile);
        //                    Picture.Type = TagLib.PictureType.FrontCover;
        //                    Picture.MimeType = "image/png";
        //                    Picture.Description = "Album Art";
        //                    tfile.Tag.Pictures = new TagLib.IPicture[] { Picture };
        //                }
        //            }
        //            tfile.Tag.Composers = [media.Composers];

        //            tfile.Tag.Conductor = media.Conductors;

        //            tfile.Tag.Comment = media.Comments;

        //            tfile.Save();

        //        }
        //        if (isCurrentlyPlayingFile)
        //        {
        //            Debug.WriteLine("FILE LOCKED CURRENTL PLAYING PATH");

        //            if (PlayerService.Masterplayer == null) return;
        //            if (File.Exists(PlayerService.CurrentPlayingPath))
        //            {
        //                PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted1;
        //                PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted1;
        //                PlayerService.OpenPath(PlayerService.CurrentPlayingPath);
        //            }
        //        }
        //    }

        //}
        public static void SetValues(string path, bool isCurrentlyPlayingFile)
        {
            Debug.WriteLine("SetValues called");
            if (!File.Exists(path)) return;

            string fileExtension = Path.GetExtension(path).ToLowerInvariant();
            if (fileExtension == ".mkv")
            {
                try
                {

                    System.Threading.Thread.Sleep(50); // Give the OS a breather

                    // 2. Open it explicitly using the low-level Matroska Engine wrapper
                    using (var mkvFile = new TagLib.Matroska.File(path, TagLib.ReadStyle.Average))
                    {
                        // This forces TagLibSharp to construct a dedicated structural Matroska Tag object
                        var mkvTag = mkvFile.GetTag(TagLib.TagTypes.Matroska, true) as TagLib.Matroska.Tag;

                        if (mkvTag != null)
                        {
                            // Set the low-level container track properties directly
                            mkvTag.Title = media.Title;
                            mkvTag.Album = media.AlbumNameInfo;
                            mkvTag.Comment = media.Comments;
                            mkvTag.Year = uint.TryParse(media.Year, out uint y) ? y : (uint)DateTime.Now.Year;

                            mkvTag.AlbumArtists = string.IsNullOrWhiteSpace(media.ArtistNameInfo)
                                ? Array.Empty<string>()
                                : media.ArtistNameInfo.Split(',').Select(a => a.Trim()).ToArray();
                        }

                        // Also blanket the abstract fallback property just in case
                        mkvFile.Tag.Title = media.Title;

                        // Commit the raw bytes directly to the file payload
                        mkvFile.Save();
                        Debug.WriteLine("MKV Title updated perfectly via low-level Matroska engine!");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Low-level MKV Write failed: {ex.Message}");
                }
            }
            else
            {

                using (var tfile = TagLib.File.Create(path))
                {
                    // 1. Set the standard abstract layer
                    tfile.Tag.Title = media.Title;
                    tfile.Tag.Album = media.AlbumNameInfo;
                    tfile.Tag.Conductor = media.Conductors;
                    tfile.Tag.Comment = media.Comments;

                    // 2. Safe Array Mappings
                    tfile.Tag.Genres = string.IsNullOrWhiteSpace(media.Genre) ? Array.Empty<string>() : media.Genre.Split(';').Select(g => g.Trim()).ToArray();
                    tfile.Tag.AlbumArtists = string.IsNullOrWhiteSpace(media.ArtistNameInfo) ? Array.Empty<string>() : media.ArtistNameInfo.Split(',').Select(a => a.Trim()).ToArray();
                    tfile.Tag.Performers = string.IsNullOrWhiteSpace(media.ContributingArtists) ? Array.Empty<string>() : media.ContributingArtists.Split(',').Select(p => p.Trim()).ToArray();

                    // 3. Track and Year
                    tfile.Tag.Track = uint.TryParse(media.TrackNumber, out uint trackNum) ? trackNum : 0;
                    tfile.Tag.Year = uint.TryParse(media.Year, out uint yearNum) ? yearNum : (uint)DateTime.Now.Year;

                    // ==========================================
                    // FORCE-WRITE LOW-LEVEL CONTAINER ATOMS
                    // ==========================================

                    // Target MKV low-level fields (Xiph Comment Headers)
                    if (tfile.GetTag(TagLib.TagTypes.Xiph) is TagLib.Ogg.XiphComment xiphTag)
                    {
                        xiphTag.SetField("TITLE", media.Title);
                        xiphTag.SetField("DATE_RELEASED", tfile.Tag.Year.ToString());
                        xiphTag.SetField("DATE", tfile.Tag.Year.ToString());
                    }

                    // Target MP4 low-level fields (Apple QuickTime Atoms)
                    if (tfile.GetTag(TagLib.TagTypes.Apple) is TagLib.Mpeg4.AppleTag appleTag)
                    {
                        appleTag.SetDashBox("©nam", "©nam", media.Title);
                        appleTag.SetDashBox("©day", "©day", tfile.Tag.Year.ToString());
                    }

                    // Target Legacy ID3v2 fields (Often hidden inside audio tracks of video streams)
                    if (tfile.GetTag(TagLib.TagTypes.Id3v2) is TagLib.Id3v2.Tag id3v2Tag)
                    {
                        id3v2Tag.Title = media.Title;
                    }

                    // 4. Album Art
                    if (!string.IsNullOrEmpty(media.AlbumArtFile) && File.Exists(media.AlbumArtFile))
                    {
                        string ext = Path.GetExtension(media.AlbumArtFile).ToLowerInvariant();
                        string mimeType = (ext == ".jpg" || ext == ".jpeg") ? "image/jpeg" : "image/png";
                        tfile.Tag.Pictures = new TagLib.IPicture[] {
                new TagLib.Picture(media.AlbumArtFile) { Type = TagLib.PictureType.FrontCover, MimeType = mimeType, Description = "cover" }
            };
                    }

                    tfile.Save();
                }
            }
            // Post-Save Player Reload Logic
            if (isCurrentlyPlayingFile && PlayerService.Masterplayer != null && File.Exists(PlayerService.CurrentPlayingPath) && PlayerService.Masterplayer.IsPlaying == true)
            {
                Debug.WriteLine("FilePathth reopened");
                Debug.WriteLine(PlayerService.CurrentPlayingPath);
                media.MediaDisplayName = media.Title;
                if (PlayerService.JustDisposed == true)
                {
                    Debug.WriteLine("JUST DISPOSED HESH");
                }
                PlayerService.Play();
            }
        }
        public static async void UpdateFileMetadata(string path)
        {
            if (path == null) return;
            string FilePath = path;
            var filelocked = GetLockingProcess.GetLockingProcesses(FilePath);
            if (filelocked.Count > 0)
            {
                Debug.WriteLine("FILE LOCKED COUNT");
                bool onlyVusicPlayer = filelocked.All(p => p.ProcessName == "Vusic Player");

                if (onlyVusicPlayer)
                {
                    Debug.WriteLine("FILE LOCKED COUNT ONLY VUSIC");

                    if (PlayerService.Masterplayer != null)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                        curtime = curTime;
                        curtimetemp = PlayerService.Masterplayer.CurTime;

                        PlayerService.filestreamcurrent?.Dispose();
                        var filelocked2 = GetLockingProcess.GetLockingProcesses(FilePath);
                        PlayerService.JustDisposed = true;
                        try
                        {

                            if (filelocked2.Count == 0)
                            {
                                Debug.WriteLine("FILE LOCKED SABED");

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
                    Debug.WriteLine("FILE LOCKED SKSK");
                    foreach (var item in filelocked)
                    {
                        Debug.WriteLine(" File is locked by " + item.ProcessName + " " + item.MainModule?.FileName);
                    }
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
