using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.FilePickers;
using Windows.Storage.Pickers;



namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoGeneral
{
    public sealed partial class Snapshot : UserControl
    {
        public Snapshot()
        {
            InitializeComponent();
        }
        #region Snapshot Settings Events

        private async void btnSnapshotDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.VideoDialogInstance == null) return;
            var folder = await FolderPickerFunct.PickFolder(App.VideoDialogInstance, "Pick Snapshot Directory", PickerLocationId.PicturesLibrary);
            if (folder != null)
            {
                string folderPath = folder.Path;
                txtSnapshotPath.Text = folderPath;
                ToolTipService.SetToolTip(btnSnapshot, folderPath);
            }
        }
        private void btnSnapshot_Click(object sender, RoutedEventArgs e)
        {
            Screen.TakeSnapshot(txtSnapshotPath.Text, chkIncludeTimestamp.IsChecked ?? false, chkIncludePosition.IsChecked ?? false);
        }

        #endregion

        private void chkIncludePosition_Checked(object sender, RoutedEventArgs e)
        {
            Screen.IsPositionIncluded = chkIncludePosition.IsChecked ?? false;
        }

        private void chkIncludePosition_Unchecked(object sender, RoutedEventArgs e)
        {
            Screen.IsPositionIncluded = chkIncludePosition.IsChecked ?? false;
        }

        private void chkIncludeTimestamp_Checked(object sender, RoutedEventArgs e)
        {
            Screen.IsTimeStampIncluded = chkIncludeTimestamp.IsChecked ?? false;
        }

        private void chkIncludeTimestamp_Unchecked(object sender, RoutedEventArgs e)
        {
            Screen.IsTimeStampIncluded = chkIncludeTimestamp.IsChecked ?? false;
        }

        private void txtSnapshotPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Screen.SnapshotDirect = txtSnapshotPath.Text;
        }
    }
}
