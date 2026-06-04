using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.WindowsAppSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class ShowModelCreation : UserControl
    {
        public ShowModelCreation()
        {
            InitializeComponent();
            PlaylistCreation.ShowCreationCall -= PlaylistCreation_ShowCreationCall;
            PlaylistCreation.ShowCreationCall += PlaylistCreation_ShowCreationCall;
        }

        private void PlaylistCreation_ShowCreationCall()
        {
            Debug.WriteLine("Add to settings");
            string showId = Guid.NewGuid().ToString("N");
            var showitem = new Show { Name = txtShowName.Text, Description = txtDescription.Text, Creators = txtCreators.Text, Crew = txtCast.Text, Genre = txtGenre.Text, Tags = txtTags.Text, ReleaseDate = dtRelease.Date, Directory = txtFolderPath.Text, Poster = posterpath, ShowID =showId  };
            PlaylistCreation.showitem = showitem;
            PlaylistCreation.CallShowCreationAdd();

        }

        private async void btnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.OceanDialogInstance, "Choose location", Windows.Storage.Pickers.PickerLocationId.VideosLibrary);
            if (folder != null)
            {
                txtFolderPath.Text = folder.Path;
                ToolTipService.SetToolTip(txtFolderPath, folder.Name);
            }
        }
        string? posterpath;
        private async void btnUploadShowPoster_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose poster");
            if (image != null)
            {
                imgShowPoster.Source = new BitmapImage(new Uri(image.Path));
                posterpath = image.Path;
            }
        }
    }

}
