using System;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class NavigationToPlaylist
    {
        public static PlaylistItem? playlisttosend;
        public static event Action? NavigCalled;
        public static void Navigate()
        {
            NavigCalled?.Invoke();
        }
    }

}
