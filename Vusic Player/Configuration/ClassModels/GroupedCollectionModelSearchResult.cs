namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModelSearchResult
    {
        public MasterSearchModel Data { get; set; } = new MasterSearchModel();
        public string Letter { get; set; } = "";
        public bool IsGroupStart { get; set; }

    }
}
