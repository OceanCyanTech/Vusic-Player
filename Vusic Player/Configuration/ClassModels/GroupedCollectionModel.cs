namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModel
    {
        public SongModel Data { get; set; } = new SongModel();// Can be SongModel, VideoModel, etc.
        public string? Letter { get; set; }
        public bool IsGroupStart { get; set; }
    }

}
