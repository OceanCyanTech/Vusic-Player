using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class PlaylistItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _playlistId = "";
        private int _seasonNumber = 1;
        private int _seasonIndex = 0;
        private string _playlistCount = "0 items";
        private string? _playlistNowPlaying = "";
        private Uri? _thumbnail;
        private bool _isPlaylistVideo = false;
        private string? _playlistGenre = "";
        private DateTime _dateCreation;
        private HashSet<string> _songsPaths = new();
        private string _playlistName = "Playlist";
        private BitmapImage? _plThumb;
        public string PlaylistId
        {
            get => _playlistId;
            set => SetProperty(ref _playlistId, value);
        }

        public int SeasonNumber
        {
            get => _seasonNumber;
            set => SetProperty(ref _seasonNumber, value);
        }

        public int SeasonIndex
        {
            get => _seasonIndex;
            set => SetProperty(ref _seasonIndex, value);
        }

        public string PlaylistCount
        {
            get => _playlistCount;
            set => SetProperty(ref _playlistCount, value);
        }

        public string? PlaylistNowPlaying
        {
            get => _playlistNowPlaying;
            set => SetProperty(ref _playlistNowPlaying, value);
        }

        public Uri? Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public bool isPlaylistVideo
        {
            get => _isPlaylistVideo;
            set => SetProperty(ref _isPlaylistVideo, value);
        }

        public string? PlaylistGenre
        {
            get => _playlistGenre;
            set => SetProperty(ref _playlistGenre, value);
        }

        public DateTime DateCreation
        {
            get => _dateCreation;
            set => SetProperty(ref _dateCreation, value);
        }

        public HashSet<string> SongsPaths
        {
            get => _songsPaths;
            set => SetProperty(ref _songsPaths, value);
        }

        public string PlaylistName
        {
            get => _playlistName;
            set => SetProperty(ref _playlistName, value);
        }

        [JsonIgnore]
        public BitmapImage? plthumb
        {
            get => _plThumb;
            set => SetProperty(ref _plThumb, value);
        }

        // Helper method to keep properties clean and avoid redundant events
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Maintained for backward compatibility or explicit recalculations
        public void NotifyCountChanged()
        {
            OnPropertyChanged(nameof(PlaylistCount));
        }
    }
}