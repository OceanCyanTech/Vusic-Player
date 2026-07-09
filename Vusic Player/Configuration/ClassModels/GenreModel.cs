using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class GenreModel : INotifyPropertyChanged
    {
        private string _genreName = "";
        private string _genreCover = "";
        private string _genreCount = "";
        private List<SongModel> _songs = new();
        private string? _genreTag;

        public string GenreName
        {
            get => _genreName;
            set => SetProperty(ref _genreName, value);
        }

        public string GenreCover
        {
            get => _genreCover;
            set => SetProperty(ref _genreCover, value);
        }

        public string GenreCount
        {
            get => _genreCount;
            set => SetProperty(ref _genreCount, value);
        }

        public List<SongModel> Songs
        {
            get => _songs;
            set => SetProperty(ref _songs, value);
        }

        public string? GenreTag
        {
            get => _genreTag;
            set => SetProperty(ref _genreTag, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
