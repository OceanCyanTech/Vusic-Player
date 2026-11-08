using System.Threading.Tasks;

namespace Vusic_Player.Configuration.UserSettings
{
    public class SettingsFunctions
    {
        public static async Task ResetAllSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.SavedVideoProgress.Clear();
            currentSettings.DoNotShowRecommendations.Clear();
            currentSettings.SavedFoldersOpened.Clear();
            currentSettings.IsDocumentEnabled = false;
            currentSettings.IsSplashEnabled = false;
            currentSettings.StartupPage = "Home";
            currentSettings.ExpandedPanelHomeView = "expRecentsVideo";
            currentSettings.ExpandedPanelMusicLib = "expRecent";
            currentSettings.ExpandedPanelVideoLib = "expRecents";
            currentSettings.IsDownloadsEnabled = false;
            currentSettings.IsPicturesEnabled = false;
            currentSettings.IsVideosEnabled = false;
            currentSettings.IsMusicEnabled = false;
            currentSettings.Shows.Clear();
            currentSettings.SavedFoldersVideoLibraryRecommendations.Clear();
            currentSettings.VersionCounter.Clear();
            currentSettings.FoldersRecent.Clear();
            currentSettings.HiddenMedia.Clear();
            currentSettings.ArtistsList.Clear();
            currentSettings.AlbumsList.Clear();
            currentSettings.GenresList.Clear();
            currentSettings.SavedPlaylists.Clear();
            currentSettings.RecentMusic.Clear();
            currentSettings.GlowColorHex = "#FF0000";
            currentSettings.GlowAnimation = false;
            currentSettings.GlowEffect = true;
            currentSettings.GlowAnimation = false;
            currentSettings.GlowEffect = true;
            currentSettings.Favourites.Clear();
            currentSettings.VideoFilesOptions.Clear();
            currentSettings.Watermark = new ClassModels.WatermarkSettingsModel();
            currentSettings.TimestampWatermarksLabels.Clear();
            currentSettings.IsSavingWatermarkValues = false;
            currentSettings.Panel1VisibilityHome = true;
            currentSettings.SkipAhead = 10;
            currentSettings.MaxVolume = 100;
            currentSettings.MaxSearchResults = 6;
            currentSettings.SkipBack = 10;
            currentSettings.Theme = "Ocean";
            currentSettings.SnapshotPath = "";
            currentSettings.EqPresets.Clear();
            currentSettings.RecordPath = "";
            currentSettings.isFirstTimeLaunch = true;
            currentSettings.FontFamily = "Segoe UI Variable Text";
            currentSettings.Panel2VisibilityHome = true;
            currentSettings.Panel3VisibilityHome = true;
            currentSettings.Panel4VisibilityHome = true;
            currentSettings.Panel1VisibilityMusic = true;
            currentSettings.Panel2VisibilityMusic = true;
            currentSettings.Panel3VisibilityMusic = true;
            currentSettings.Panel4VisibilityMusic = true;
            currentSettings.Panel1VisibilityVideo = true;
            currentSettings.Panel2VisibilityVideo = true;
            currentSettings.Panel3VisibilityVideo = true;
            currentSettings.Panel4VisibilityVideo = true;
            currentSettings.IsShowingWatermarkValues = true;
            currentSettings.IsTimeStampEnabledOnSnapshot = false;
            currentSettings.IsPositionEnabledOnSnapshot = false;
            currentSettings.ArtistView_selectorbarindex = 0;
            currentSettings.IncludeSubDirMusLib = true;
            currentSettings.FolderPathSnapshot = "";
            currentSettings.IsPlayerTimeStampEnabledOnSnapshot = false;
            currentSettings.IsHorizontalFlip = false;
            currentSettings.IsFirstTimeLaunchMusicLib = true;
            currentSettings.IsMusicHistoryDisabled = false;
            currentSettings.IsVideoHistoryDisabled = false;
            currentSettings.IsRecommendationsDisabled = false;
            currentSettings.IsVerticalFlip = false;
            currentSettings.ShowTutorialDialog = true;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
    }
}
