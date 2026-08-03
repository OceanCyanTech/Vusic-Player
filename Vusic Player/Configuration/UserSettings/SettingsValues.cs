
using System.Collections.ObjectModel;

using Vusic_Player.Configuration.ClassModels;
using PlaylistItem = Vusic_Player.Configuration.ClassModels.PlaylistItem;

namespace Vusic_Player.Configuration.UserSettings
{
    public class SettingsValues
    {
        public ObservableCollection<VideoProgress> SavedVideoProgress { get; set; } = new();
        public ObservableCollection<VideoProgress> DoNotShowRecommendations { get; set; } = new();
        public ObservableCollection<FoldersListOpened> SavedFoldersOpened { get; set; } = new();
        public ObservableCollection<Show> Shows { get; set; } = new();
        public ObservableCollection<FoldersListOpened> SavedFoldersVideoLibraryRecommendations { get; set; } = new ObservableCollection<FoldersListOpened>();

        public ObservableCollection<int> VersionCounter { get; set; } = new();
        public ObservableCollection<FolderModel> FoldersRecent { get; set; } = new();
        public ObservableCollection<ArtistModel> ArtistsList { get; set; } = new();
        public ObservableCollection<AlbumModel> AlbumsList { get; set; } = new();
        public ObservableCollection<GenreModel> GenresList { get; set; } = new();
        public ObservableCollection<PlaylistItem> SavedPlaylists { get; set; } = new();
        //public ObservableCollection<AppPersonalization> UserSettings { get; set; } = new();
        public ObservableCollection<RecentMusicModel> RecentMusic { get; set; } = new();
        //public ObservableCollection<QueueList> QueueSave { get; set; } = new();

        public ObservableCollection<FavouriteItems> Favourites { get; set; } = new();
        public ObservableCollection<MediaOptions> VideoFilesOptions { get; set; } = new();
        public bool IsTimeStampEnabledOnSnapshot { get; set; } = false;
        public int ArtistView_selectorbarindex { get; set; } = 0;
        public bool IncludeSubDirMusLib { get; set; } = true;
        public string FolderPathSnapshot { get; set; } = "";
        public bool IsPlayerTimeStampEnabledOnSnapshot { get; set; } = false;
        public bool IsHorizontalFlip { get; set; } = false;
        public bool IsFirstTimeLaunchMusicLib { get; set; } = true;
        public bool IsMusicHistoryDisabled { get; set; } = false;
        public bool IsVerticalFlip { get; set; } = false;
        public double VideoRotation { get; set; } = 0;
        public double VideoBrightness { get; set; } = 0;
        public double VideoHue { get; set; } = 0;
        public double VideoSaturation { get; set; } = 0;
        public bool ShowDefaultMessage { get; set; }

    }

}
