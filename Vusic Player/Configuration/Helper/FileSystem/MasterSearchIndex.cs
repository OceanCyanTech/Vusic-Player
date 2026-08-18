using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
   
    public class MasterSearchIndex
    {
        public enum Filters
        {
            Music,
            Videos,
            Artist,
            Playlist,
            Album,
            Settings,
            Pages,
            Playlists,
            Shows,
            Genres
        }
        public static void GetSearchResults(string query, Filters filters)
        {

        }
    }
}
