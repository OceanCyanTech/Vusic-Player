using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.FilePickers;
using Windows.Storage.Pickers;



namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoGeneral
{
    public sealed partial class Record : UserControl
    {
        public Record()
        {
            InitializeComponent();
        }
        #region Recording Settings Events

        private async void btnRecordDirectory_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.OceanDialogInstance != null)
            {
                var folder = await FolderPickerFunct.PickFolder(App.OceanDialogInstance, "Pick Record Directory", PickerLocationId.VideosLibrary);

                if (folder != null)
                {
                    txtRecordPath.Text = folder.Path;
                }
            }
        }
        public Screen UIState { get; } = new Screen();
        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer != null)
            {
                if (PlayerService.Masterplayer.IsRecording)
                {
                    Screen.StopRecordRequest();
                }
                else
                {
                    Screen.Record(txtRecordPath.Text);
                }
            }
        }
        #endregion

        private void txtRecordPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Screen.RecordDirect = txtRecordPath.Text;
        }
    }

}
