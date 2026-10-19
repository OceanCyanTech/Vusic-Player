namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModelPlaylist
    {
        public PlaylistItem Data { get; set; } = new PlaylistItem();// Can be SongModel, VideoModel, etc.
        public string? Letter { get; set; }
        public bool IsGroupStart { get; set; }
    }
}
