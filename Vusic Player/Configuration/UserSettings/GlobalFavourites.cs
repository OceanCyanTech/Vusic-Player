using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.Configuration.UserSettings
{
    public class IsFavouriteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? filePath = value switch
            {
                string path => path,
                SongModel song => song.FilePath,
                _ => null
            };

            return GlobalFavourites.Instance.IsFavourite(filePath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class GlobalFavourites : INotifyPropertyChanged
    {
        private static readonly Lazy<GlobalFavourites> _lazy = new(() => new GlobalFavourites());
        public static GlobalFavourites Instance => _lazy.Value;

        // Use ordinal case-insensitive matching for file system paths
        private readonly HashSet<string> _favouritePaths = new(StringComparer.OrdinalIgnoreCase);
        private bool _isLoaded;

        /// <summary>
        /// Raised whenever a file path is added or removed from favourites.
        /// SongModel instances listen to this to refresh their properties.
        /// </summary>
        public event Action<string>? FavouritesChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private GlobalFavourites() { }

        /// <summary>
        /// Loads favorites from Settings JSON. Call this during app startup.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isLoaded) return;

            var settings = await SettingsLoader.LoadSettingsAsync();
            if (settings?.Favourites != null)
            {
                _favouritePaths.Clear();
                foreach (var item in settings.Favourites)
                {
                    if (!string.IsNullOrWhiteSpace(item.FilePath))
                    {
                        _favouritePaths.Add(item.FilePath);
                    }
                }
            }

            _isLoaded = true;
            OnPropertyChanged(nameof(Count));
        }

        /// <summary>
        /// Fast O(1) lookup to check if a track is favorited.
        /// Called directly by SongModel.IsFavourite.
        /// </summary>
        public bool IsFavourite(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            return _favouritePaths.Contains(filePath);
        }

        public int Count => _favouritePaths.Count;

        /// <summary>
        /// Toggles a single file's favorite status and notifies all active SongModels.
        /// </summary>
        public async Task ToggleFavouriteAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            bool isFav = _favouritePaths.Contains(filePath);
            await SetFavouritesRangeAsync(new[] { filePath }, add: !isFav);
        }

        /// <summary>
        /// Bulk updates favorites and notifies all active SongModels.
        /// </summary>
        public async Task SetFavouritesRangeAsync(IEnumerable<string> filePaths, bool add)
        {
            var paths = filePaths.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
            if (!paths.Any()) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.Favourites ??= new ObservableCollection<FavouriteItems>();

            bool changed = false;

            foreach (var path in paths)
            {
                if (add)
                {
                    if (_favouritePaths.Add(path))
                    {
                        currentSettings.Favourites.Add(new FavouriteItems { FilePath = path });
                        FavouritesChanged?.Invoke(path);
                        changed = true;
                    }
                }
                else
                {
                    if (_favouritePaths.Remove(path))
                    {
                        var existing = currentSettings.Favourites.FirstOrDefault(f =>
                            string.Equals(f.FilePath, path, StringComparison.OrdinalIgnoreCase));

                        if (existing != null)
                        {
                            currentSettings.Favourites.Remove(existing);
                        }

                        FavouritesChanged?.Invoke(path);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                OnPropertyChanged(nameof(Count));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
