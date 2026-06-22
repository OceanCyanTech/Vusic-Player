using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.Configuration.Playback;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Web;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class InfoBox : UserControl
    {
        public MediaPlaybackController media => MediaPlaybackController.Instance;
        public InfoBox()
        {
            InitializeComponent();
            PlayerService.CheckProcesses += PlayerService_CheckProcesses;
        }
        public void ScrollIntoView()
        {
            stkMain.StartBringIntoView(new BringIntoViewOptions()
            {
                AnimationDesired = true,
                VerticalAlignmentRatio = 0
            });
        }
        private async void PlayerService_CheckProcesses()
        {
            if (PlayerService.FileRenameIssue == true)
            {
                ifbError.Title = "Rename Error";
                ifbError.Message = $"File with the name '{media.FileName}' already exists. Try again with a different name.";
                ifbError.Severity = InfoBarSeverity.Error;
                ifbError.IsOpen = true;
                ifbError.StartBringIntoView(new BringIntoViewOptions()
                {
                    AnimationDesired = true,
                    VerticalAlignmentRatio = 0 // 0 means scroll until it's at the very top
                });
            }
            else
            {
                var list = PlayerService.processlocklist;
                ifbError.Title = "Error";
                if (list == null)
                {
                    return;
                }

                var processNames = string.Join(", ", list.Select(p => p.ProcessName));

                ifbError.Message = $"Cannot save changes because the file is in use by: {processNames}.";
                ifbError.Severity = InfoBarSeverity.Error;
                ifbError.IsOpen = true;
                ifbError.StartBringIntoView(new BringIntoViewOptions()
                {
                    AnimationDesired = true,
                    VerticalAlignmentRatio = 0 // 0 means scroll until it's at the very top
                });
            }
        }

        private void Masterplayer_PlaybackStopped(object? sender, PlaybackStoppedArgs e)
        {
            Debug.WriteLine("PLAYBACKSTOPPED FOR A SECOND");
            //var tfile = TagLib.File.Create(PlayerService.CurrentPlayingPath);
            //tfile.Tag.Genres = [media.Genre];
            //tfile.Save();
        }



        private void Masterplayer_OpenCompleted(object? sender, FlyleafLib.MediaPlayer.OpenCompletedArgs e)
        {
            Debug.WriteLine("Successfully restored");
        }

        private async void btnChangeAlbumArt_Click(object sender, RoutedEventArgs e)
        {
            var getvideoinfo = await VideoMetadata.GetVideoMetadata(PlayerService.CurrentPlayingPath);
            Debug.WriteLine("VIDEO METADATA: " + getvideoinfo.Codec + " " + getvideoinfo.Height + " x" + getvideoinfo.Width + " " + getvideoinfo.DisplayResolution + " " + getvideoinfo.FrameRate + " ");
            if (App.OceanDialogInstance != null)
            {
                var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose Image");
                if (image == null) return;
                media.AlbumArtFile = image.Path;
                albumArt.Source = new BitmapImage(new Uri(image.Path));
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(media.FilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{media.FilePath}\"");
            }
        }

        private void mnftOpenFileLoc_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(media.FilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{media.FilePath}\"");
            }
        }

        private async void mnftCopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(media.FilePath))
            {
                CopyToClipboard.CopyStringToClipboard(media.FilePath);
                ttFilePath.IsOpen = true;
                await Task.Delay(2000);
                ttFilePath.IsOpen = false;
            }
        }
    }
}
