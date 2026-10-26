using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.FilePickers;
using Windows.Storage.Pickers;



namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoGeneral
{
    public sealed partial class Snapshot : UserControl
    {
        public Snapshot()
        {
            InitializeComponent();
            LoadSettings();
        }
        private async void LoadSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            txtSnapshotPath.Text = currentSettings.SnapshotPath;
            ToolTipService.SetToolTip(btnSnapshot, txtSnapshotPath.Text);
            Screen.Instance.SnapshotDirect = txtSnapshotPath.Text;
        }
        #region Snapshot Settings Events

        private async void btnSnapshotDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.VideoDialogInstance == null) return;
            var folder = await FolderPickerFunct.PickFolder(App.VideoDialogInstance, "Pick Snapshot Directory", PickerLocationId.PicturesLibrary);
            if (folder != null)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();

                string folderPath = folder.Path;
                txtSnapshotPath.Text = folderPath;
                ToolTipService.SetToolTip(btnSnapshot, folderPath);
                currentSettings.SnapshotPath = folderPath;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }
        private void btnSnapshot_Click(object sender, RoutedEventArgs e)
        {
            Screen.Instance.TakeSnapshot(txtSnapshotPath.Text, chkIncludeTimestamp.IsChecked ?? false, chkIncludePosition.IsChecked ?? false);
        }

        #endregion

        private void chkIncludePosition_Checked(object sender, RoutedEventArgs e)
        {
            Screen.Instance.IsPositionIncluded = chkIncludePosition.IsChecked ?? false;
        }

        private void chkIncludePosition_Unchecked(object sender, RoutedEventArgs e)
        {
            Screen.Instance.IsPositionIncluded = chkIncludePosition.IsChecked ?? false;
        }

        private void chkIncludeTimestamp_Checked(object sender, RoutedEventArgs e)
        {
            Screen.Instance.IsTimeStampIncluded = chkIncludeTimestamp.IsChecked ?? false;
        }

        private void chkIncludeTimestamp_Unchecked(object sender, RoutedEventArgs e)
        {
            Screen.Instance.IsTimeStampIncluded = chkIncludeTimestamp.IsChecked ?? false;
        }

        private void txtSnapshotPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Screen.Instance.SnapshotDirect = txtSnapshotPath.Text;
        }
    }
}
