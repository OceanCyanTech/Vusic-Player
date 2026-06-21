using FlyleafLib.MediaFramework.MediaStream;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.SubtitlesProperties;
using Vusic_Player.Configuration.Helper.SubtitlesProperties.ExternalSubtitles;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Subtitle.SubtitleGeneral
{
    public sealed partial class General : UserControl
    {
        public Stream ViewModel { get; } = new();
        public General()
        {
            InitializeComponent();
            Stream.SubtitlePathChanged += Stream_SubtitlePathChanged;
            Stream.SubtitleSearch += Stream_SubtitleSearch;
            Stream.ExternalAdded += Stream_ExternalAdded;

        }

        private void Stream_ExternalAdded()
        {
            lstViewExternalList.ItemsSource = External.ExternalSubtitles;
        }

        private void Stream_SubtitleSearch()
        {
            srchSubtitles.Visibility = Visibility.Visible;
        }

        private void Stream_SubtitlePathChanged()
        {
            txtExternalSubPath.Text = Stream.ExternalSubtitlePath;
        }
        #region Subtitle Management Events

        private void srchSubtitles_CloseSearch(object sender, EventArgs e)
        {
            srchSubtitles.Visibility = Visibility.Collapsed;
        }

        private void srchSubtitles_ItemSelected(object sender, SearchModel e)
        {
            if (e is SearchModel srchmod && e.Carrier is SubtitlesStream subtitlesStream)
            {
                GeneralInfoService.ShowInfo($"Subtitles set to {subtitlesStream.Language}");
                Stream.Set(subtitlesStream);
            }
        }

        private void cmbEmbeddedSubTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Logic to switch between tracks stored in the video file
        }

        private void btnSearchSubtitles_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;

            ObservableCollection<SearchModel> searchres = new();
            foreach (var sub in PlayerService.Masterplayer.Subtitles.Streams)
            {
                searchres.Add(new SearchModel { ResultString = $"{sub.StreamIndex}. {sub.Language}", Carrier = sub });
            }
            srchSubtitles.ItemsSource = searchres;
            srchSubtitles.Visibility = Visibility.Visible;
        }

        private void tglDisableSubtitles_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Stream.Disable(tglDisableSubtitles.IsChecked ?? false);
        }

        private void tglDisableSubtitles_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Stream.Disable(tglDisableSubtitles.IsChecked ?? false);
        }

        private async void btnBrowseExternalSubtitles_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            else
            {
                Debug.WriteLine("NULL");
            }
            var file = await SubtitlePicker.PickSingle(App.OceanDialogInstance, "Choose subtitle file");

            if (file != null)
            {
                txtExternalSubPath.Text = file.Path;
                if (PlayerService.Masterplayer != null)
                {
                    PlayerService.Masterplayer.Open(file.Path);

                    GeneralInfoService.ShowInfo($"Subtitles set to external path {file.Path}");
                    Stream.ExternalSubtitleAdded();
                }
            }
        }

        private void btnSubtitleEditor_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Open your subtitle editing tool/window
        }

        #endregion

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as HyperlinkButton;
            if (btn == null) return;
            var file = await StorageFile.GetFileFromPathAsync(btn.Content.ToString());
            var options = new FolderLauncherOptions();
            options.ItemsToSelect.Add(file);

            await Launcher.LaunchFolderAsync(await file.GetParentAsync(), options);
        }

        private async void mnftOpenFileLocationExtSub_Click(object sender, RoutedEventArgs e)
        {
            var menufly = sender as MenuFlyoutItem;
            var data = menufly?.DataContext as ExternalModel;

            if (data == null) return;

            var path = data?.Path;
            var file = await StorageFile.GetFileFromPathAsync(data?.Path);
            var options = new FolderLauncherOptions();
            options.ItemsToSelect.Add(file);

            await Launcher.LaunchFolderAsync(await file.GetParentAsync(), options);
        }

        private void mnftRemoveFromListExtSub_Click(object sender, RoutedEventArgs e)
        {
            var menufly = sender as MenuFlyoutItem;
            var data = menufly?.DataContext as ExternalModel;
            if (data != null)
                External.ExternalSubtitles.Remove(data);
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.decoder.OpenSuggestedSubtitles();
            foreach (var stream in PlayerService.Masterplayer.Subtitles.Streams)
            {
                if (stream.Language == PlayerService.Masterplayer.Config.Subtitles.Languages[0])
                {
                    PlayerService.Masterplayer.Open(stream);
                }
            }
            PlayerService.Masterplayer.Subtitles.Streams
      .Where(s => s.Enabled)
      .ToList()
      .ForEach(s => GeneralInfoService.ShowInfo($"Subtitles set to {s.StreamIndex}. {s.Language}"));
            PlayerService.Masterplayer.Config.Subtitles.Enabled = true;
        }

        private void lstViewExternalList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = lstViewExternalList.SelectedItem as ExternalModel;
            if (PlayerService.Masterplayer != null && selected != null)
            {
                PlayerService.Masterplayer.Open(selected.Path);
                GeneralInfoService.ShowInfo($"Subtitles set to external path {selected.Path}");

            }
        }

        private async void btnBrowseExternalSubtitles_Click_1(object sender, RoutedEventArgs e)
        {
            if (App.VideoDialogInstance == null) return;
            else
            {
                Debug.WriteLine("NULL");
            }
            var file = await SubtitlePicker.PickSingle(App.VideoDialogInstance, "Choose subtitle file");

            if (file != null)
            {
                txtExternalSubPath.Text = file.Path;
                if (PlayerService.Masterplayer != null)
                {
                    PlayerService.Masterplayer.Open(file.Path);

                    GeneralInfoService.ShowInfo($"Subtitles set to external path {file.Path}");
                    Stream.ExternalSubtitleAdded();
                }
            }
        }
    }
}
