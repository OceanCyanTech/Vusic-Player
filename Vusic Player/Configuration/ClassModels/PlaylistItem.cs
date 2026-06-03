using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class PlaylistItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? PlaylistId { get; set; }
        public string? PlaylistName { get; set; } = "";
        public string? PlaylistCount { get; set; } = "";
        public string? PlaylistNowPlaying { get; set; } = "";

        public Uri? Thumbnail { get; set; }
        public bool isPlaylistVideo { get; set; } = false;
        public string? PlaylistGenre { get; set; } = "";
        public HashSet<string> SongsPaths { get; set; } = new();
        public DateTime DateCreation { get; set; }
        public void NotifyCountChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCount)));
        }

        [JsonIgnore]
        public BitmapImage? plthumb { get; set; }
    }
}