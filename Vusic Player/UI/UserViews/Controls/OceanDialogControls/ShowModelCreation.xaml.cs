using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;


namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class ShowModelCreation : UserControl
    {
        public ShowModelCreation()
        {
            InitializeComponent();
            PlaylistCreation.ShowCreationCall -= PlaylistCreation_ShowCreationCall;
            PlaylistCreation.ShowCreationCall += PlaylistCreation_ShowCreationCall;
            PlaylistCreation.ExistingShowDirectory -= PlaylistCreation_ExistingShowDirectory;
            PlaylistCreation.ExistingShowDirectory += PlaylistCreation_ExistingShowDirectory;
        }

        private void PlaylistCreation_ExistingShowDirectory()
        {
            txtFolderPath.Text = PlaylistCreation.ExistingShowDir;
        }

        private void PlaylistCreation_ShowCreationCall()
        {
            Debug.WriteLine("Add to settings");
            string showId = Guid.NewGuid().ToString("N");
            var showitem = new Show { Name = txtShowName.Text, Description = txtDescription.Text, Creators = txtCreators.Text, Crew = txtCast.Text, Genre = txtGenre.Text, Tags = txtTags.Text, ReleaseDate = dtRelease.Date, Directory = txtFolderPath.Text, Poster = posterpath, ShowID = showId };
            PlaylistCreation.showitem = showitem;
            PlaylistCreation.CallShowCreationAdd();

        }
        ShowCreationValues Instance => ShowCreationValues.Instance;

        private async void btnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.OceanDialogInstance, "Choose location", Windows.Storage.Pickers.PickerLocationId.VideosLibrary);
            if (folder != null)
            {
                txtFolderPath.Text = folder.Path;
                ToolTipService.SetToolTip(txtFolderPath, folder.Name);
                Instance.Directory = folder.Path;
            }
        }
        string posterpath = "ms-appx:///Assets/appicon.png";
        private async void btnUploadShowPoster_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose poster");
            if (image != null)
            {
                imgShowPoster.Source = new BitmapImage(new Uri(image.Path));
                posterpath = image.Path;
                Instance.PosterPath = posterpath;
            }
        }

        private void txtShowName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

}
