using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.ClassModels
{
    public class SongModel : INotifyPropertyChanged
    {
        private string _title = "";
        public TimeSpan? SongDuration { get; set; }

        private Brush? _titleColor;
        private int _year;
        private int seasonindexassociated;
        private Visibility visibilityofstrikethrough = Visibility.Collapsed;
        private string _artist = "";
        private string _genre = "";
        private string _albumName = "";
        private string? _fileTypeName = "Video File";
        private bool? _isEpisode;
        private Visibility _isQueueItem = Visibility.Visible;
        public int Year
        {
            get => _year;
            set { _year = value; OnPropertyChanged(); }
        }
        public int SeasonIndexAssoc
        {
            get => seasonindexassociated;
            set { seasonindexassociated = value; OnPropertyChanged(); }
        }
        public Visibility QueueControls
        {
            get => _isQueueItem;
            set { _isQueueItem = value; OnPropertyChanged(); }
        }
        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }
        public string Genre
        {
            get => _genre;
            set { _genre = value; OnPropertyChanged(); }
        }
        public bool? IsEpisode
        {
            get => _isEpisode;
            set { _isEpisode = value; OnPropertyChanged(); }
        }
        public string AlbumName
        {
            get => _albumName;
            set { _albumName = value; OnPropertyChanged(); }
        }
        public string? FileTypeName
        {
            get => _fileTypeName;
            set { _fileTypeName = value; OnPropertyChanged(); }
        }
        public Brush TitleColor
        {
            get
            {
                // Instantiates the Brush only when XAML binds to it on the UI Thread
                return _titleColor ??= new SolidColorBrush(Microsoft.UI.Colors.White);
            }
            set
            {
                _titleColor = value;
                OnPropertyChanged();
            }
        }
        public string Glyph => (PlayerService.CurrentPlayingPath == FilePath) ? (PlayerService.Masterplayer != null && PlayerService.Masterplayer.IsPlaying ? "\uE768" : "\uE769") : ((File.Exists(FilePath) && Extensions.VideoExtensions.List.Contains(Path.GetExtension(FilePath))) ? "\uE8B2" : "\uEC4F");


        private string mediatype = "Playlist";// Default color
        public string MediaType
        {
            get => mediatype;
            set { mediatype = value; OnPropertyChanged(nameof(MediaType)); }
        }
        private string removetext = "Remove";// Default color
        public string Remove
        {
            get => removetext;
            set { removetext = value; OnPropertyChanged(nameof(Remove)); }
        }
        private DateTime _dateCreated;
        public DateTime DateCreated
        {
            get => _dateCreated;
            set
            {
                _dateCreated = value;
                OnPropertyChanged(nameof(DateCreated));
            }
        }
        private DateTime _datemodified;
        public DateTime DateModified
        {
            get => _datemodified;
            set
            {
                _datemodified = value;
                OnPropertyChanged(nameof(DateModified));
            }
        }

        private Visibility isMovableitem = Visibility.Visible;
        public Visibility IsMovableItem
        {
            get => isMovableitem;
            set
            {
                if (isMovableitem != value)
                {
                    isMovableitem = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsAudioItem => (File.Exists(FilePath) && Extensions.AudioExtensions.List.Contains(Path.GetExtension(FilePath)));
        public string FormattedDuration
        {
            get
            {
                if (!SongDuration.HasValue)
                    return "00:00";

                TimeSpan ts = SongDuration.Value;

                return ts.TotalHours >= 1
                    ? $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                    : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private string _filePath = "";
        private bool isCompleted = false;

        public Visibility VisibilityOfStrikeThrough
        {
            get => visibilityofstrikethrough;
            set
            {
                if (visibilityofstrikethrough != value)
                {
                    visibilityofstrikethrough = value;
                    // This "notifies" the Button and the Menu to update their visuals
                    OnPropertyChanged();
                }
            }
        }
        public Visibility VisibilityofAudioMeta => (File.Exists(FilePath) && Extensions.AudioExtensions.List.Contains(Path.GetExtension(FilePath))) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityofVideoInfo => (File.Exists(FilePath) && Extensions.VideoExtensions.List.Contains(Path.GetExtension(FilePath))) ? Visibility.Visible : Visibility.Collapsed;

        public SongModel()
        {
            GlobalFavourites.Instance.FavouritesChanged += OnGlobalFavouritesChanged;
        }

        private void OnGlobalFavouritesChanged(string changedPath)
        {
            // Only react if this specific track's path was modified
            if (!string.Equals(FilePath, changedPath, StringComparison.OrdinalIgnoreCase))
                return;

            // Ensure PropertyChanged fires on the UI Thread
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcher != null && !dispatcher.HasThreadAccess)
            {
                dispatcher.TryEnqueue(NotifyFavouriteProperties);
            }
            else
            {
                NotifyFavouriteProperties();
            }
        }

        private void NotifyFavouriteProperties()
        {
            OnPropertyChanged(nameof(IsFavourite));
            OnPropertyChanged(nameof(FavOpacity));
            OnPropertyChanged(nameof(FavString));
        }
        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                if (isCompleted != value)
                {
                    isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }
        public bool IsFavourite => GlobalFavourites.Instance.IsFavourite(FilePath);
        public double FavOpacity => IsFavourite ? 1.0 : 0.0;
        public string FavString => IsFavourite ? "Remove from Favourites" : "Add to Favourites";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;

                OnPropertyChanged(nameof(FilePath));
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
        [JsonIgnore]
        private BitmapImage? _videothumbnail;
        public BitmapImage? VideoThumbnail
        {
            get => _videothumbnail;
            set
            {
                if (_videothumbnail != value)
                {
                    _videothumbnail = value;
                    OnPropertyChanged();
                }
            }
        }
    }

}
