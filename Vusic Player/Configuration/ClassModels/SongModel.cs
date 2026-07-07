using System;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Vusic_Player.Configuration.ClassModels
{
    public class SongModel : INotifyPropertyChanged
    {
        private string? _title;
        public TimeSpan? SongDuration { get; set; }
      
        private Brush _titleColor = new SolidColorBrush(Microsoft.UI.Colors.White); // Safe for any thread!
        private int _year;
        private int seasonindexassociated;
        private Visibility visibilityofstrikethrough = Visibility.Collapsed;
        private Visibility visibilityofaudiometadata = Visibility.Visible;
        private Visibility visibilityofvidinfo = Visibility.Collapsed;
        private string _artist = "";
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
            get => _titleColor;
            set
            {
                _titleColor = value;
                OnPropertyChanged(nameof(TitleColor));
            }
        }
        private string _glyph = "\uEC4F";// Default color
        public string Glyph
        {
            get => _glyph;
            set { _glyph = value; OnPropertyChanged(nameof(Glyph)); }
        }
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
        private bool isAudioItem = true;
        public bool IsAudioItem
        {
            get => isAudioItem;
            set
            {
                if (isAudioItem != value)
                {
                    isAudioItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FormattedDuration => SongDuration.HasValue
    ? $"{(int)SongDuration.Value.TotalMinutes:D2}:{SongDuration.Value.Seconds:D2}"
    : "00:00";

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
        public Visibility VisibilityofAudioMeta
        {
            get => visibilityofaudiometadata;
            set
            {
                if (visibilityofaudiometadata != value)
                {
                    visibilityofaudiometadata = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility VisibilityofVideoInfo
        {
            get => visibilityofvidinfo;
            set
            {
                if (visibilityofvidinfo != value)
                {
                    visibilityofvidinfo = value;
                    OnPropertyChanged();
                }
            }
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
        private bool _isFavorite;
        public bool IsFavourite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    // This "notifies" the Button and the Menu to update their visuals
                    OnPropertyChanged();
                }
            }
        }
        private double opaci;
        public double FavOpacity
        {
            get => opaci;
            set
            {
                if (opaci != value)
                {
                    opaci = value;
                    // This "notifies" the Button and the Menu to update their visuals
                    OnPropertyChanged();
                }
            }
        }
        private string favtext = "Add to favourites";
        public string FavString
        {
            get => favtext;
            set
            {
                if (favtext != value)
                {
                    favtext = value;
                    // This "notifies" the Button and the Menu to update their visuals
                    OnPropertyChanged();
                }
            }
        }
        public string? Title
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
