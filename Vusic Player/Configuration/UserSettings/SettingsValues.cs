
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Playback;
using PlaylistItem = Vusic_Player.Configuration.ClassModels.PlaylistItem;

namespace Vusic_Player.Configuration.UserSettings
{
    public class SettingsValues
    {
        public ObservableCollection<VideoProgress> SavedVideoProgress { get; set; } = new();
        public ObservableCollection<VideoProgress> DoNotShowRecommendations { get; set; } = new();
        public ObservableCollection<FoldersListOpened> SavedFoldersOpened { get; set; } = new();
        public bool IsDocumentEnabled { get; set; } = true;
        public bool IsSplashEnabled { get; set; } = true;
        public string StartupPage { get; set; } = "Home";
        public string ExpandedPanelHomeView { get; set; } = "expRecentsVideo";
        public string ExpandedPanelMusicLib { get; set; } = "expRecent";
        public string ExpandedPanelVideoLib { get; set; } = "expRecents";
        public bool IsDownloadsEnabled { get; set; } = true;
        public bool IsPicturesEnabled { get; set; } = true;
        public bool IsVideosEnabled { get; set; } = true;
        public bool IsMusicEnabled { get; set; } = true;
        public ObservableCollection<Show> Shows { get; set; } = new();
        public ObservableCollection<FoldersListOpened> SavedFoldersVideoLibraryRecommendations { get; set; } = new ObservableCollection<FoldersListOpened>();

        public ObservableCollection<int> VersionCounter { get; set; } = new();
        public ObservableCollection<FolderModel> FoldersRecent { get; set; } = new();
        public HashSet<HiddenMediaItem> HiddenMedia { get; set; } = new();
        public ObservableCollection<ArtistModel> ArtistsList { get; set; } = new();
        public ObservableCollection<AlbumModel> AlbumsList { get; set; } = new();
        public ObservableCollection<GenreModel> GenresList { get; set; } = new();
        public ObservableCollection<PlaylistItem> SavedPlaylists { get; set; } = new();
        //public ObservableCollection<AppPersonalization> UserSettings { get; set; } = new();
        public ObservableCollection<RecentMusicModel> RecentMusic { get; set; } = new();
        //public ObservableCollection<QueueList> QueueSave { get; set; } = new();

        public ObservableCollection<FavouriteItems> Favourites { get; set; } = new();
        public ObservableCollection<MediaOptions> VideoFilesOptions { get; set; } = new();
        public WatermarkSettingsModel Watermark { get; set; } = new();
        public ObservableCollection<WatermarkLabelSettingsModel> TimestampWatermarksLabels { get; set; } = new();

        public bool IsSavingWatermarkValues { get; set; } = false;
        public bool Panel1VisibilityHome { get; set; } = true;
        public double SkipAhead { get; set; } = 10;
        public double SkipBack { get; set; } = 10;
        public string Theme { get; set; } = "Ocean";
        public string FontFamily { get; set; } = "Ocean";
        public bool Panel2VisibilityHome { get; set; } = true;
        public bool Panel3VisibilityHome { get; set; } = true;
        public bool Panel4VisibilityHome { get; set; } = true;
        public bool Panel1VisibilityMusic { get; set; } = true;
        public bool Panel2VisibilityMusic { get; set; } = true;
        public bool Panel3VisibilityMusic { get; set; } = true;
        public bool Panel4VisibilityMusic { get; set; } = true;
        public bool Panel1VisibilityVideo { get; set; } = true;
        public bool Panel2VisibilityVideo { get; set; } = true;
        public bool Panel3VisibilityVideo { get; set; } = true;
        public bool Panel4VisibilityVideo { get; set; } = true;
        public bool IsShowingWatermarkValues { get; set; } = true;
        public bool IsTimeStampEnabledOnSnapshot { get; set; } = false;
        public int ArtistView_selectorbarindex { get; set; } = 0;
        public bool IncludeSubDirMusLib { get; set; } = true;
        public string FolderPathSnapshot { get; set; } = "";
        public bool IsPlayerTimeStampEnabledOnSnapshot { get; set; } = false;
        public bool IsHorizontalFlip { get; set; } = false;
        public bool IsFirstTimeLaunchMusicLib { get; set; } = true;
        public bool IsMusicHistoryDisabled { get; set; } = false;
        public bool IsVideoHistoryDisabled { get; set; } = false;
        public bool IsRecommendationsDisabled { get; set; } = false;
        public bool IsVerticalFlip { get; set; } = false;
        public double VideoRotation { get; set; } = 0;
        public double VideoBrightness { get; set; } = 0;
        public double VideoHue { get; set; } = 0;
        public double VideoSaturation { get; set; } = 0;
        public bool ShowDefaultMessage { get; set; }

    }

}
