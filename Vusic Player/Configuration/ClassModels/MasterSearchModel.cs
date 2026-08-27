using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class MasterSearchModel
    {
        public string ResultMain { get; set; } = "";
        public int Score { get; set; } = 0;
        public string ImageThumbnail { get; set; } = "";
        public string SubInformation { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string PlaylistID { get; set; } = "";
        public string ShowID { get; set; } = "";
        public string FolderPath { get; set; } = "";
        public Filters SearchFilter { get; set; } = Filters.All;
        
    }
    public enum Filters
    {
        All,
        Music,
        Videos,
        Artist,
        Playlist,
        Album,
        Settings,
        Pages,
        Playlists,
        Shows,
        Genres,
        Folders
    }
}
