using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ArtistDiscAlbumModel : INotifyPropertyChanged
    {
        private string _albumName = "";
        private string _albumCount = "";
        private string _albumYear = "";
        private string _albumArtists = "";
        private string _thumbnail = "";
        private List<SongModel> _songs = new();
        private BitmapImage? _albumCoverThumbnail;

        public string AlbumName
        {
            get => _albumName;
            set => SetProperty(ref _albumName, value);
        }

        public string AlbumCount
        {
            get => _albumCount;
            set => SetProperty(ref _albumCount, value);
        }

        public string AlbumYear
        {
            get => _albumYear;
            set => SetProperty(ref _albumYear, value);
        }

        public string AlbumArtists
        {
            get => _albumArtists;
            set => SetProperty(ref _albumArtists, value);
        }

        public string Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public List<SongModel> Songs
        {
            get => _songs;
            set => SetProperty(ref _songs, value);
        }

        [JsonIgnore]
        public BitmapImage? AlbumCoverThumbnail
        {
            get => _albumCoverThumbnail;
            set => SetProperty(ref _albumCoverThumbnail, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
