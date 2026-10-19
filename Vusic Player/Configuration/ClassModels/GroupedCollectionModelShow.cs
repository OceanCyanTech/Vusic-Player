namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModelShow
    {
        public Show Data { get; set; } = new Show();// Can be SongModel, VideoModel, etc.
        public string Letter { get; set; } = "";
        public bool IsGroupStart { get; set; }

    }
}
