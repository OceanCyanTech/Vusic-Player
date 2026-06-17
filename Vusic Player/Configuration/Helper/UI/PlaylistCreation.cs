using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class PlaylistCreation
    {
        public static ObservableCollection<SongModel> existingitems = new();
        public static event Action? CreationCall;
        public static event Action? ShowCreationCall;
        public static event Action? ExistingItems;
        public static event Action? ExistingShowDirectory;
        public static event Action? CreationCallAdd;
        public static event Action? ShowCreationCallAdd;
        public static PlaylistItem? playlistItem;
        public static Show? showitem;
        public static string? suggestedplaylistname = "Playlist";
        public static string? ExistingShowDir = "";
        public static void CallPlaylistCreation()
        {
            CreationCall?.Invoke();
        }
        public static void CallShowCreation()
        {
            ShowCreationCall?.Invoke();
        }
        public static void CallExistingItems(ObservableCollection<SongModel> ItemsToAdd)
        {

            existingitems.Clear();
            foreach (var item in ItemsToAdd)
            {
                Debug.WriteLine(item.Title + " Item:");
                existingitems.Add(item);
            }
            ExistingItems?.Invoke();
        }
        public static void CallExistingShowDirectory(string FolderPath)
        {
            ExistingShowDir = FolderPath;
            ExistingShowDirectory?.Invoke();
        }
        public static void CallPlaylistCreationAdd()
        {
            CreationCallAdd?.Invoke();
        }
        public static void CallShowCreationAdd()
        {
            ShowCreationCallAdd?.Invoke();
        }
    }

}
