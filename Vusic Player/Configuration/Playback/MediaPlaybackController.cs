using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vusic_Player;

namespace Vusic_Player.Configuration.Playback
{
    public class MediaPlaybackController : INotifyPropertyChanged
    {
        public MediaPlaybackController()
        {
            QueueService.VusicQueueNext.CollectionChanged += VusicQueueNext_CollectionChanged;
        }

        private void VusicQueueNext_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
            e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == NotifyCollectionChangedAction.Move
                )
            {
                var currentQueue = (IsFullQueueMode == true) ? QueueService.VusicQueue : QueueService.VusicQueueNext;

                int count = currentQueue.Count;
                QueuePageEmptyVisibility = (count == 0)
                       ? Visibility.Visible
                       : Visibility.Collapsed;
                ItemsCount = $"• {count} {(count == 1 ? "item" : "items")}";
                TimeSpan totalDuration = TimeSpan.Zero;

                foreach (var item in currentQueue)
                {
                   totalDuration += item.SongDuration ?? TimeSpan.Zero;
                }

                if (totalDuration.TotalHours < 1)
                {
                    TotalQueueRuntime = $"• {totalDuration.ToString(@"mm\:ss")}";
                }
                else
                {
                    TotalQueueRuntime = $"• {totalDuration.ToString(@"h\:mm\:ss")}";
                }
            }
        }




        public static MediaPlaybackController Instance { get; } = new MediaPlaybackController();

        private double _currentPosition;
        private bool isFullQueue = false;
        private double _totalDuration;
        private string _songName = "Nothing playing";
        private string itemscount = "• 0 items";
        private string TotalQueueDuration = "• 00:00:00";
        private string _albumName = "Unknown Album";
        private string _artistName = "Unknown Artist";
        private string _artistNameInfo = "Unknown Artist";
        private string _albumNameInfo = "Unknown Artist";
        //private string _aspectRatio = "16:9";
        private string _volumeText = "100%";
        private double _volumeValue = 100;
        private bool _isReversePlayback = false;
        private double _zoomValue = 100;
        private double _speedValue = 1;
        private double _pitchValue = 1;
        private string _volumeGlyph = "\uE767";
        private string _playPauseToolTip = "Play";
        private Visibility queuepageemtpyvisibility = Visibility.Visible;
        private Brush _volumeForeground = new SolidColorBrush(Colors.White);
        private ImageSource _thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
        private ImageSource _thumbnail2 = new BitmapImage(new Uri("ms-appx:///Assets/appicon.png"));

        #region Properties

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (SetProperty(ref _currentPosition, value))
                {
                    OnPropertyChanged(nameof(RunningDurationString));
                }
            }
        }
        public double VolumeValue
        {
            get => _volumeValue;
            set
            {
                if (SetProperty(ref _volumeValue, value))
                {
                    OnPropertyChanged(nameof(VolumeValue));
                }
            }
        }
        public bool IsReversePlayback
        {
            get => _isReversePlayback;
            set
            {
                if (SetProperty(ref _isReversePlayback, value))
                {
                    OnPropertyChanged(nameof(IsReversePlayback));
                }
            }
        }
        public double ZoomValue
        {
            get => _zoomValue;
            set
            {
                if (SetProperty(ref _zoomValue, value))
                {
                    OnPropertyChanged(nameof(ZoomValue));
                }
            }
        }
        public double SpeedValue
        {
            get => _speedValue;
            set
            {
                if (SetProperty(ref _speedValue, value))
                {
                    OnPropertyChanged(nameof(SpeedValue));
                }
            }
        }
        public double PitchValue
        {
            get => _pitchValue;
            set
            {
                if (SetProperty(ref _pitchValue, value))
                {
                    OnPropertyChanged(nameof(PitchValue));
                }
            }
        }
        public bool IsFullQueueMode
        {
            get => isFullQueue;
            set
            {
                if (SetProperty(ref isFullQueue, value))
                {
                    OnPropertyChanged(nameof(isFullQueue));
                }
            }
        }
        public double TotalDuration
        {
            get => _totalDuration;
            set
            {
                if (SetProperty(ref _totalDuration, value))
                {
                    OnPropertyChanged(nameof(TotalDurationString));
                }
            }
        }

        // Computed string properties
        public string RunningDurationString => TimeSpan.FromSeconds(CurrentPosition).ToString(@"hh\:mm\:ss");
        public string TotalDurationString => TimeSpan.FromSeconds(TotalDuration).ToString(@"hh\:mm\:ss");

        public string MediaDisplayName
        {
            get => _songName;
            set => SetProperty(ref _songName, value);
        }
        private string _fileName = "";

        public string FileName
        {
            get => _fileName;
            set
            {
                if (SetProperty(ref _fileName, value))
                {
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }
        private string _year = "";
        public string Year
        {
            get => _year;
            set
            {
                if (SetProperty(ref _year, value))
                {
                    OnPropertyChanged(nameof(Year));
                }
            }
        }

        private string _trackNumber = "";
        public string TrackNumber
        {
            get => _trackNumber;
            set
            {
                if (SetProperty(ref _trackNumber, value))
                {
                    OnPropertyChanged(nameof(TrackNumber));
                }
            }
        }

        private string _duration = "";
        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _bitrate = "";
        public string Bitrate
        {
            get => _bitrate;
            set => SetProperty(ref _bitrate, value);
        }
        private string _genre = "";
        public string Genre
        {
            get => _genre;

            set
            {
                if (SetProperty(ref _genre, value))
                {
                    OnPropertyChanged(nameof(Genre));
                }
            }
        }
        private string _datecreated = "07/10/2008";
        public string DateCreated
        {
            get => _datecreated;
            set => SetProperty(ref _datecreated, value);
        }
        private string _filesize = "0 MB";
        public string FileSize
        {
            get => _filesize;
            set => SetProperty(ref _filesize, value);
        }
        private string _datemodified = "07/10/2008";
        public string DateModified
        {
            get => _datemodified;
            set => SetProperty(ref _datemodified, value);
        }
        private string _comments = "";
        public string Comments
        {
            get => _comments;
            set
            {
                if (SetProperty(ref _comments, value))
                {
                    OnPropertyChanged(nameof(Comments));
                }
            }
        }
        private double _rating = 0;
        public double Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }
        private string _speed = "";
        public string Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }
        private string _sampleRate = "";
        public string SampleRate
        {
            get => _sampleRate;
            set => SetProperty(ref _sampleRate, value);
        }

        private string _channels = "";
        public string Channels
        {
            get => _channels;
            set => SetProperty(ref _channels, value);
        }
        private BitmapImage _albumart;
        public BitmapImage AlbumArt
        {
            get => _albumart;
            set => SetProperty(ref _albumart, value);
        }
        private string _storagefilealbumart;
        public string AlbumArtFile
        {
            get => _storagefilealbumart;
            set => SetProperty(ref _storagefilealbumart, value);
        }
        private string _composers = "";
        public string Composers
        {
            get => _composers;
            set
            {
                if (SetProperty(ref _composers, value))
                {
                    OnPropertyChanged(nameof(Composers));
                }
            }
        }

        private string _conductors = "";
        public string Conductors
        {
            get => _conductors;
            set
            {
                if (SetProperty(ref _conductors, value))
                {
                    OnPropertyChanged(nameof(Conductors));
                }
            }
        }
        private string _fileType = "";
        public string FileType
        {
            get => _fileType;
            set => SetProperty(ref _fileType, value);
        }
        private string _title = "";

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        private string _path = "";

        public string FilePath
        {
            get => _path;
            set
            {
                if (SetProperty(ref _path, value))
                {
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }
        private string _contributingartists = "";

        public string ContributingArtists
        {
            get => _contributingartists;
            set
            {
                if (SetProperty(ref _contributingartists, value))
                {
                    OnPropertyChanged(nameof(ContributingArtists));
                }
            }
        }
        public string TotalQueueRuntime
        {
            get => TotalQueueDuration;
            set => SetProperty(ref TotalQueueDuration, value);
        }
        public string ItemsCount
        {
            get => itemscount;
            set => SetProperty(ref itemscount, value);
        }
        public string ArtistDisplayName
        {
            get => _artistName;
            set
            {
                if (SetProperty(ref _artistName, value))
                {
                    OnPropertyChanged(nameof(ArtistDisplayName));
                }
            }
        }
        public string ArtistNameInfo
        {
            get => _artistNameInfo;
            set
            {
                if (SetProperty(ref _artistNameInfo, value))
                {
                    OnPropertyChanged(nameof(ArtistNameInfo));
                }
            }
        }
        public string AlbumNameInfo
        {
            get => _albumNameInfo;
            set
            {
                if (SetProperty(ref _albumNameInfo, value))
                {
                    OnPropertyChanged(nameof(AlbumNameInfo));
                }
            }
        }
        public Visibility QueuePageEmptyVisibility
        {
            get => queuepageemtpyvisibility;
            set => SetProperty(ref queuepageemtpyvisibility, value);
        }
        public string AlbumDisplayName
        {
            get => _albumName;
            set
            {
                if (SetProperty(ref _albumName, value))
                {
                    OnPropertyChanged(nameof(AlbumDisplayName));
                }
            }
        }

        public string VolumeString
        {
            get => _volumeText;
            set => SetProperty(ref _volumeText, value);
        }

        public string VolumeGlyph
        {
            get => _volumeGlyph;
            set => SetProperty(ref _volumeGlyph, value);
        }

        public string PlayPauseToolTip
        {
            get => _playPauseToolTip;
            set => SetProperty(ref _playPauseToolTip, value);
        }

        public Brush VolumeForeground
        {
            get => _volumeForeground;
            set => SetProperty(ref _volumeForeground, value);
        }

        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }
        public ImageSource CoverThumbnail
        {
            get => _thumbnail2;
            set => SetProperty(ref _thumbnail2, value);
        }


        #endregion

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
