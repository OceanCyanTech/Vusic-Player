// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.ClassModels
{
    [Microsoft.UI.Xaml.Data.Bindable]
    public class SongModel : INotifyPropertyChanged
    {
        private string _title = "";
        public TimeSpan? SongDuration { get; set; }
        private bool _isplaying = false;
        private Brush? _titleColor;
        private int _year;
        private int seasonindexassociated;
        private Visibility visibilityofstrikethrough = Visibility.Collapsed;
        private string _artist = "";
        private string _genre = "";
        private bool _ismissing = false;
        private string _albumName = "";
        private string? _fileTypeName = "Video File";
        private bool? _isEpisode;
        private Visibility _isQueueItem = Visibility.Visible;
        private bool _isqueueitem = false;
        private bool _iscurrentactiveitem = false;
        public int Year
        {
            get => _year;
            set { _year = value; OnPropertyChanged(); }
        }
        public bool IsQueueItem
        {
            get => _isqueueitem;
            set
            {
                _isqueueitem = value;
                OnPropertyChanged();
            }
        }
        public bool IsCurrentQueueItem
        {
            get => _iscurrentactiveitem;
            set { _iscurrentactiveitem = value; OnPropertyChanged(); }
        }
        public bool IsFileMissing
        {
            get => _ismissing;
            set { _ismissing = value; OnPropertyChanged(); OnPropertyChanged(nameof(VisibilityOfMissingAttributes)); OnPropertyChanged(nameof(VisibilityOfMissingAttributesReverse));
            }
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
        public string Glyph => IsQueueItem ? (IsCurrentQueueItem ? (PlayerService.Masterplayer != null && PlayerService.Masterplayer.IsPlaying ? "\uE769" : "\uE768") : (File.Exists(FilePath) && ExtensionVerifier.FileTypeExtension(FilePath) == ExtensionVerifier.FileTypeEnum.Video ? "\uE8B2" : "\uEC4F")) : (PlayerService.PlayerServiceInstance.CurrentPlayingMediaPath == FilePath) ? (PlayerService.Masterplayer != null && PlayerService.Masterplayer.IsPlaying ? "\uE769" : "\uE768") : ((File.Exists(FilePath) && ExtensionVerifier.FileTypeExtension(FilePath) == ExtensionVerifier.FileTypeEnum.Video ? "\uE8B2" : "\uEC4F"));


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

        public Visibility VisibilityOfStrikeThrough => isCompleted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityOfMissingAttributes => IsFileMissing ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityOfMissingAttributesReverse => IsFileMissing ? Visibility.Collapsed : Visibility.Visible;
        //{
        //    get => visibilityofstrikethrough;
        //    set
        //    {
        //        if (visibilityofstrikethrough != value)
        //        {
        //            visibilityofstrikethrough = value;
        //            // This "notifies" the Button and the Menu to update their visuals
        //            OnPropertyChanged();
        //        }
        //    }
        //}
        public Visibility VisibilityofAudioMeta => (File.Exists(FilePath) && Extensions.AudioExtensions.List.Contains(Path.GetExtension(FilePath))) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityofVideoInfo => (File.Exists(FilePath) && Extensions.VideoExtensions.List.Contains(Path.GetExtension(FilePath))) ? Visibility.Visible : Visibility.Collapsed;

        public SongModel()
        {
            GlobalFavourites.Instance.FavouritesChanged += OnGlobalFavouritesChanged;
            PlayerService.PlayerServiceInstance.PropertyChanged += PlayerServiceInstance_PropertyChanged
                ;
        }

        private void PlayerServiceInstance_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerService.CurrentPlayingMediaPath))
            {
                Debug.WriteLine("YES PATH CHANGED: " + PlayerService.PlayerServiceInstance.CurrentPlayingMediaPath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Glyph)));
            }
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
        public bool IsPlaying => IsQueueItem ? (IsCurrentQueueItem) : !string.IsNullOrEmpty(FilePath) && string.Equals(FilePath, PlayerService.PlayerServiceInstance.CurrentPlayingMediaPath, StringComparison.OrdinalIgnoreCase);
        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                if (isCompleted != value)
                {
                    isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(VisibilityOfStrikeThrough));
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
        private FileSystemWatcher? _watcher;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value)
                    return;

                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
                WatchFile();
            }
        }
        private void WatchFile()
        {
            _dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
            _watcher?.Dispose();
            _watcher = null;
            if (!File.Exists(_filePath)) return;
            var fileDirectory = Path.GetDirectoryName(_filePath);
            var filename = Path.GetFileName(_filePath);
            if (fileDirectory == null) return;
            _watcher = new FileSystemWatcher(fileDirectory, filename)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };
            _watcher.Changed += _watcher_Changed;
        }
        private DispatcherQueue? _dispatcherQueue;
        private async void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath)) return;
            try
            {
                Debug.WriteLine("ALELR4");

                await Task.Delay(150); // Allow file writer to release exclusive lock
                Debug.WriteLine("ALEL5");

                if (_dispatcherQueue is null)
                {
                    Debug.WriteLine("WATCHER: _dispatcherQueue is null.");
                    return;
                }

                _dispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        var metadata = await MetadataFetcher.GetSongModel(e.FullPath);
                        Debug.WriteLine("ALELR3");
                        Debug.WriteLine("aLBUM: " + metadata.AlbumName);
                        AlbumName = metadata.AlbumName;
                        Artist = metadata.Artist;
                        Title = metadata.Title;
                        SongDuration = metadata.SongDuration;
                        OnPropertyChanged(nameof(AlbumName));
                        OnPropertyChanged(nameof(Artist));
                        OnPropertyChanged(nameof(Title));
                        OnPropertyChanged(nameof(SongDuration));
                        OnPropertyChanged(nameof(FormattedDuration));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("WATCHER UI ERROR: " + ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WATCHER ERROR: " + ex);
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
