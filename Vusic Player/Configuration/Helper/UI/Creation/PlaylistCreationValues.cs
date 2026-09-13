using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.Helper.UI.Creation
{
    public class PlaylistCreationValues : INotifyPropertyChanged
    {
        public PlaylistCreationValues()
        {
            PlaylistsMaster.CollectionChanged += PlaylistsMaster_CollectionChanged;
            _mediaSongModels.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(PlaylistCount));
            };
        }

        private async void PlaylistsMaster_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
    e.Action == NotifyCollectionChangedAction.Add ||
    e.Action == NotifyCollectionChangedAction.Move)
            {
                Debug.WriteLine("Moved");
                Debug.WriteLine("Removed");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.SavedPlaylists = PlaylistsMaster;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }

        }

        public static PlaylistCreationValues Instance { get; } = new PlaylistCreationValues();
        private ObservableCollection<PlaylistItem> _playlistsMaster = new ObservableCollection<PlaylistItem>();

   
        private string _genre = "";
        private bool _isAppInstanceOcean = true;
        private List<string> _directory =  new List<string>();
        private string _playlistName = "Playlist";
        private BitmapImage _plThumb = new BitmapImage(new Uri("ms-appx:///Assets/playlistdefaultdark.png"));
        private string _playlistID = "";
        private Uri _thumbnail = new Uri("ms-appx:///Assets/playlistdefaultdark.png");
        private string _thumbnailString = "ms-appx:///Assets/playlistdefaultdark.png";
        private DateTime _creationdate = DateTime.Now;
        private string _playlistCount = "0 items";
        private HashSet<string> _songsPaths = new();
        private ObservableCollection<SongModel> _mediaSongModels = new();


        public string PlaylistName
        {
            get => _playlistName;
            set => SetProperty(ref _playlistName, value);
        }
        public bool IsAppInstanceOceanDialog
        {
            get => _isAppInstanceOcean;
            set => SetProperty(ref _isAppInstanceOcean, value);
        }
        public ObservableCollection<SongModel> MediaSongModels
        {
            get => _mediaSongModels;
            set => SetProperty(ref _mediaSongModels, value);
        }
        public BitmapImage PlCover
        {
            get => _plThumb;
            set => SetProperty(ref _plThumb, value);
        }
        public Uri Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }
        public string ThumbnailString
        {
            get => _thumbnailString;
            set => SetProperty(ref _thumbnailString, value);
        }
        public ObservableCollection<PlaylistItem> PlaylistsMaster
        {
            get => _playlistsMaster;
            set => SetProperty(ref _playlistsMaster, value);
        }


      

        public string Genre
        {
            get => _genre;
            set => SetProperty(ref _genre, value);
        }

        public string PlaylistCount => $"{_mediaSongModels.Count} {(_mediaSongModels.Count == 1 ? "item" : "items")}";
       
        public HashSet<string> MediaPaths
        {
           
            get => _songsPaths;
            set => SetProperty(ref _songsPaths, value);
        }


        public string PlaylistID
        {
            get => _playlistID;
            set => SetProperty(ref _playlistID, value);
        }

        public DateTime CreationDate
        {
            get => _creationdate;
            set => SetProperty(ref _creationdate, value);
        }
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;


        /// Compares current value with new value. If different, updates and raises notification.

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                             ?? App.MainWindowInstance?.DispatcherQueue;

            if (dispatcher != null && !dispatcher.HasThreadAccess)
            {
                dispatcher.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}
