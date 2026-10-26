using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.FilePickers;
using Windows.Storage.Pickers;



namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoGeneral
{
    public sealed partial class Record : UserControl
    {
        public Record()
        {
            InitializeComponent();
            LoadSettings();
        }
        private async void LoadSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            txtRecordPath.Text = currentSettings.RecordPath;
            ToolTipService.SetToolTip(btnRecord, txtRecordPath.Text);
            Screen.Instance.RecordDirect = txtRecordPath.Text;
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
                    ToolTipService.SetToolTip(btnRecord, txtRecordPath.Text);
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();

                    currentSettings.RecordPath = folder.Path;
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
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
                    Screen.Instance.StopRecordRequest();
                }
                else
                {
                    Screen.Instance.Record(txtRecordPath.Text);
                }
            }
        }
        #endregion

        private void txtRecordPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Screen.Instance.RecordDirect = txtRecordPath.Text;
        }
    }

}
