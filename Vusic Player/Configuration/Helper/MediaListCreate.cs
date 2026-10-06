using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Windows.Storage;

namespace Vusic_Player.Configuration.Helper
{
    public class MediaListCreate
    {
        public static async Task<ObservableCollection<SongModel>> GetSongModelsAsync(List<string> Paths)
        {
            ObservableCollection<SongModel> Songs = new ObservableCollection<SongModel>();
            foreach(var path in Paths)
            {
                if (File.Exists(path))
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    string fileExtension = storagefile.FileType.ToLowerInvariant();

                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {
                        var videoprops = await storagefile.Properties.GetVideoPropertiesAsync();
                        var title = string.IsNullOrEmpty(videoprops.Title) ? Path.GetFileNameWithoutExtension(path) : videoprops.Title;
                        Songs.Add(new SongModel { Title = title, FilePath = path });
                    }
                    else
                    {
                        var musicprops = await storagefile.Properties.GetMusicPropertiesAsync();
                        var title = string.IsNullOrEmpty(musicprops.Title) ? Path.GetFileNameWithoutExtension(path) : musicprops.Title;
                        var artist = string.IsNullOrEmpty(musicprops.Artist) ? "Unknown Artist" : musicprops.Artist;
                        var album = string.IsNullOrEmpty(musicprops.Album) ? "Unknown Album" : musicprops.Album;
                        Songs.Add(new SongModel { Title = title, FilePath = path, AlbumName = album, Artist = artist });
                    }
                }
            }
            return Songs;
        }
    }
}
