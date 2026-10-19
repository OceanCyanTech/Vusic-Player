namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModelArtist
    {
        public ArtistShow Data { get; set; } = new ArtistShow();// Can be SongModel, VideoModel, etc.
        public string? Letter { get; set; }
        public bool IsGroupStart { get; set; }
    }
}
