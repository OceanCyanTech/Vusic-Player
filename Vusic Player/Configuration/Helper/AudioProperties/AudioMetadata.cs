using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.AppConfig;

namespace Vusic_Player.Configuration.Helper.AudioProperties
{
    public static class AudioMetadata
    {
        private static string GetTag(string path, System.Func<TagLib.File, string> selector)
        {
            try
            {
                using (var file = TagLib.File.Create(path))
                {
                    return selector(file) ?? "Unknown";
                }
            }
            catch (System.Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static string Title(string path) => GetTag(path, f => f.Tag.Title);
        public static string Duration(string path)
        {
            try
            {
                using (var file = TagLib.File.Create(path))
                {
                    TimeSpan t = file.Properties.Duration;

                    // If it's 1 hour or more, use h:mm:ss
                    if (t.TotalHours >= 1)
                    {
                        return t.ToString(@"h\:mm\:ss");
                    }

                    // Otherwise, just mm:ss
                    return t.ToString(@"mm\:ss");
                }
            }
            catch
            {
                return "00:00";
            }
        }
        public static async Task<TimeSpan> GetTimeSpanDuration(string path)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();
                return properties.Duration;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
        public static string Artist(string path) => GetTag(path, f => f.Tag.FirstAlbumArtist);

        public static string Album(string path) => GetTag(path, f => f.Tag.Album);

        public static uint Year(string path)
        {
            using (var file = TagLib.File.Create(path))
            {
                return file.Tag.Year;
            }
        }
        private static bool UpdateAlbumName(string path, string name)
        {
            var filelocked = GetLockingProcess.GetLockingProcesses(path);
            var file = TagLib.File.Create(path);

            if (filelocked.Count == 0)
            {
                file.Tag.Album = name;
                file.Save();
                file.Dispose();
                return true;
            }
            else
            {
                bool onlyVusicPlayer = filelocked.All(p => p.ProcessName == "Vusic Player");

                if (onlyVusicPlayer)
                {
                    if (PlayerService.Masterplayer != null)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                        PlayerService.curtime = curTime;
                        PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;

                        if (PlayerService.Masterplayer.Status == FlyleafLib.MediaPlayer.Status.Playing)
                        {
                            Debug.WriteLine("TRUEEE");
                            isPaused2 = false;
                        }
                        else
                        {
                            isPaused2 = true;
                        }
                        PlayerService.filestreamcurrent?.Dispose();
                        PlayerService.JustDisposed = true;
                        var filelocked2 = GetLockingProcess.GetLockingProcesses(path);
                        if (filelocked2.Count == 0)
                        {
                            try
                            {
                                file.Tag.Album = name;
                                file.Save();
                                file.Dispose();
                                if (isPaused2 == false)
                                {
                                    Debug.WriteLine("IsPuae");
                                    PlayerService.Play();
                                }

                                return true;

                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex.Message, "Album.Rename", Logger.LogLevelType.Error);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        private static bool UpdateGenreName(string path, string name)
        {
            var filelocked = GetLockingProcess.GetLockingProcesses(path);
            var file = TagLib.File.Create(path);

            if (filelocked.Count == 0)
            {
                Debug.WriteLine("REMOV EGENRE ZERO PROCESS");

                file.Tag.Genres =[name];
                file.Save();
                file.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("REMOV EGENRE MULTI PROCESS");

                bool onlyVusicPlayer = filelocked.All(p => p.ProcessName == "Vusic Player");

                if (onlyVusicPlayer)
                {
                    Debug.WriteLine("REMOV EGENRE ONLY VUSIC PROCESS");

                    if (PlayerService.Masterplayer != null)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                        PlayerService.curtime = curTime;
                        PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;

                        if (PlayerService.Masterplayer.Status == FlyleafLib.MediaPlayer.Status.Playing)
                        {
                            Debug.WriteLine("TRUEEE");
                            isPaused2 = false;
                        }
                        else
                        {
                            isPaused2 = true;
                        }
                        PlayerService.filestreamcurrent?.Dispose();
                        PlayerService.JustDisposed = true;
                        var filelocked2 = GetLockingProcess.GetLockingProcesses(path);
                        if (filelocked2.Count == 0)
                        {
                            try
                            {
                                file.Tag.Genres = [name];
                                file.Save();
                                file.Dispose();
                                if (isPaused2 == false)
                                {
                                    Debug.WriteLine("IsPuae");
                                    PlayerService.Play();
                                }

                                return true;

                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex.Message, "Genre.Rename", Logger.LogLevelType.Error);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private static bool UpdateArtistName(string path, string name)
        {
            var filelocked = GetLockingProcess.GetLockingProcesses(path);
            var file = TagLib.File.Create(path);

            if (filelocked.Count == 0)
            {
                file.Tag.AlbumArtists = [name];
                file.Save();
                file.Dispose();
                return true;
            }
            else
            {
                bool onlyVusicPlayer = filelocked.All(p => p.ProcessName == "Vusic Player");

                if (onlyVusicPlayer)
                {
                    if (PlayerService.Masterplayer != null)
                    {
                        var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                        PlayerService.curtime = curTime;
                        PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;

                        if (PlayerService.Masterplayer.Status == FlyleafLib.MediaPlayer.Status.Playing)
                        {
                            Debug.WriteLine("TRUEEE");
                            isPaused2 = false;
                        }
                        else
                        {
                            isPaused2 = true;
                        }
                        PlayerService.filestreamcurrent?.Dispose();
                        PlayerService.JustDisposed = true;
                        var filelocked2 = GetLockingProcess.GetLockingProcesses(path);
                        if (filelocked2.Count == 0)
                        {
                            try
                            {
                                file.Tag.AlbumArtists = [name];
                                file.Save();
                                file.Dispose();
                                if (isPaused2 == false)
                                {
                                    Debug.WriteLine("IsPuae");
                                    PlayerService.Play();
                                }

                                return true;

                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex.Message, "Artist.Rename", Logger.LogLevelType.Error);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private static bool isPaused2 = false;
        public static bool ChangeGenre(List<string> paths, string newGenre)
        {
            Debug.WriteLine("REMOV EGENRE REQUESTED");

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    Debug.WriteLine("REMOV EGENRE CONTINUE + " + path);

                    if (UpdateGenreName(path, newGenre))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static bool ChangeAlbumName(List<string> paths, string newName)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (UpdateAlbumName(path, newName))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public static bool ChangeAlbumName(string singlepath, string newName)
        {

            if (File.Exists(singlepath))
            {
                if (UpdateAlbumName(singlepath, newName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;

            }
        }
        public static bool ChangeArtistName(List<string> paths, string newName)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (UpdateArtistName(path, newName))
                    {
                        Debug.WriteLine("artist name updated to " + newName + " for " + path);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public static bool ChangeArtistName(string singlepath, string newName)
        {

            if (File.Exists(singlepath))
            {
                if (UpdateArtistName(singlepath, newName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;

            }
        }

    }
}
