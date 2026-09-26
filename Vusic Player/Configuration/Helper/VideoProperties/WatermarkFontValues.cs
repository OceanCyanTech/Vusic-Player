using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.Helper.VideoProperties
{
    public class WatermarkFontValues : INotifyPropertyChanged
    {
        public WatermarkFontValues()
        {
            ShowsMaster.CollectionChanged += ShowsMaster_CollectionChanged;
        }

        private async void ShowsMaster_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
    e.Action == NotifyCollectionChangedAction.Add ||
    e.Action == NotifyCollectionChangedAction.Move)
            {
                Debug.WriteLine("Moved");
                Debug.WriteLine("Removed");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.Shows = ShowsMaster;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }

        }

        public static WatermarkFontValues Instance { get; } = new WatermarkFontValues();

        private string _showName = "Show";
        private string _description = "";
        private string _creators = "";
        private string _cast = "";
        private string _genre = "";
        private string _tags = "";
        private string _directory = "";
        private string _posterpath = "ms-appx:///Assets/appicon.png";
        private string _showID = "";
        private Visibility _visibilityViewEpisodes = Visibility.Collapsed;
        private DateTimeOffset _releasedate;

        public string ShowName
        {
            get => _showName;
            set => SetProperty(ref _showName, value);
        }

        public ObservableCollection<Show> ShowsMaster
        {
            get => _showsMaster;
            set => SetProperty(ref _showsMaster, value);
        }


        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        public Visibility VisibilityOfViewEpisodes
        {
            get => _visibilityViewEpisodes;
            set => SetProperty(ref _visibilityViewEpisodes, value);
        }

        public string Creators
        {
            get => _creators;
            set => SetProperty(ref _creators, value);
        }

        public string Cast
        {
            get => _cast;
            set => SetProperty(ref _cast, value);
        }

        public string Genre
        {
            get => _genre;
            set => SetProperty(ref _genre, value);
        }

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public string Directory
        {
            get => _directory;
            set => SetProperty(ref _directory, value);
        }

        public string PosterPath
        {
            get => _posterpath;
            set => SetProperty(ref _posterpath, value);
        }

        public string ShowID
        {
            get => _showID;
            set => SetProperty(ref _showID, value);
        }

        public DateTimeOffset ReleaseDate
        {
            get => _releasedate;
            set => SetProperty(ref _releasedate, value);
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
