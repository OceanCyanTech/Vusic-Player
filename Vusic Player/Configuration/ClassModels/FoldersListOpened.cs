namespace Vusic_Player.Configuration.ClassModels
{
    public class FoldersListOpened
    {
        public string FolderPath { get; set; } = "";
        public bool IsHighLevelFolder { get; set; } = false;
        public string? FolderName { get; set; }
        public bool isChecked { get; set; } = false;
        public bool Show { get; set; } = true;
    
    }
}
