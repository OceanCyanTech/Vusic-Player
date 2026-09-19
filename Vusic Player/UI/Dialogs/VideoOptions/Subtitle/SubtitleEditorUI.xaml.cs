using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Subtitle
{
    public class CountToCueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} subtitle cue" : $"• {count} subtitle cues";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class SubtitleEditorUI : UserControl
    {
        public SubtitleEditorUI()
        {
            InitializeComponent();
        }

        private void btnInsertAbove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInsertBelow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tglView_Checked(object sender, RoutedEventArgs e)
        {

        }
        ObservableCollection<SubtitleCueModel> Subtitles = new ObservableCollection<SubtitleCueModel>();
        private async void btnLoadSubtitleFile_Click(object sender, RoutedEventArgs e)
        {
            if (App.SubtitleEditorDialogInstance == null) return;

            Subtitles.Clear();
            var subtitlefile = await FilePickers.SubtitlePicker.PickSingle(App.SubtitleEditorDialogInstance, "Load Subtitle");
            if (subtitlefile == null) return;
            FilePathOpened = subtitlefile.Path;
            stkNoSubtitles.Visibility = Visibility.Collapsed;
            grdColumnHeaders.Visibility = Visibility.Visible;
            mnftCopyFilePath.Visibility = Visibility.Visible;
            mnftRenameSubtitle.Visibility = Visibility.Visible;
            mnftOpenFileLoc.Visibility = Visibility.Visible;
            btnSaveFile.IsEnabled = true;
            btnSaveAsFile.IsEnabled = true;
            txtFileName.Text = Path.GetFileNameWithoutExtension(subtitlefile.Path);
            ToolTipService.SetToolTip(txtFileName, subtitlefile.Path);

            var regex = new Regex(
        @"(?<start>\d{2}:\d{2}:\d{2}[,\.]\d{3})\s*-->\s*(?<end>\d{2}:\d{2}:\d{2}[,\.]\d{3})[^\r\n]*\r?\n(?<text>(?:(?!\r?\n\r?\n|\r?\n\d+\r?\n|\r?\n\d{2}:\d{2}).)+)",
        RegexOptions.Singleline);
            string rawContent = await FileIO.ReadTextAsync(subtitlefile);
            MatchCollection matches = regex.Matches(rawContent);

            foreach (Match match in matches)
            {
                string text = match.Groups["text"].Value.Trim();
                string startRaw = match.Groups["start"].Value.Replace(',', '.');
                string endRaw = match.Groups["end"].Value.Replace(',', '.');

                if (TimeSpan.TryParse(startRaw, out TimeSpan startTime) &&
                    TimeSpan.TryParse(endRaw, out TimeSpan endTime))
                {
                    // Format options:
                    // @"hh\:mm\:ss"  -> "00:01:23"
                    // @"m\:ss"       -> "1:23" (if under an hour)
                    string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";

                    Subtitles.Add(new SubtitleCueModel
                    {
                        Text = text,
                        StartTime = startTime,
                        EndTime = endTime,
                        StartString = startTime.ToString(format),
                        EndString = endTime.ToString(endFormat)
                    });
                }
            }
        }

        private async void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            int index = 1;

            foreach (var cue in Subtitles)
            {
                // 1. Cue index (must be sequential starting from 1)
                sb.AppendLine(index.ToString());

                // 2. Timestamps formatted with comma for milliseconds
                string start = cue.StartTime.ToString(@"hh\:mm\:ss\,fff");
                string end = cue.EndTime.ToString(@"hh\:mm\:ss\,fff");
                sb.AppendLine($"{start} --> {end}");

                // 3. Subtitle text
                sb.AppendLine(cue.Text);

                // 4. Blank separator line
                sb.AppendLine();

                index++;
            }
            if (File.Exists(FilePathOpened))
            {
                var storagefile = await StorageFile.GetFileFromPathAsync(FilePathOpened);
                await FileIO.WriteTextAsync(storagefile, sb.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8);
                txtFileName.Text = Path.GetFileNameWithoutExtension(storagefile.Path);
                ToolTipService.SetToolTip(txtFileName, storagefile.Path);
            }
            else
            {

                var picker = new FileSavePicker();
                var window = App.SubtitleEditorDialogInstance;
                if (window == null) return;
                // WinUI 3 HWND association
                IntPtr hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(picker, hwnd);

                picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                picker.FileTypeChoices.Add("SubRip Subtitle", new List<string> { ".srt" });
                picker.SuggestedFileName = txtFileName.Text;

                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    string newContent = sb.ToString();
                    await FileIO.WriteTextAsync(file, newContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    FilePathOpened = file.Path;

                    txtFileName.Text = Path.GetFileNameWithoutExtension(file.Path);
                    ToolTipService.SetToolTip(txtFileName, storagefile.Path);

                }
            }
        }

        private async void btnSaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            int index = 1;

            foreach (var cue in Subtitles)
            {
                // 1. Cue index (must be sequential starting from 1)
                sb.AppendLine(index.ToString());

                // 2. Timestamps formatted with comma for milliseconds
                string start = cue.StartTime.ToString(@"hh\:mm\:ss\,fff");
                string end = cue.EndTime.ToString(@"hh\:mm\:ss\,fff");
                sb.AppendLine($"{start} --> {end}");

                // 3. Subtitle text
                sb.AppendLine(cue.Text);

                // 4. Blank separator line
                sb.AppendLine();

                index++;
            }
            var picker = new FileSavePicker();
            var window = App.SubtitleEditorDialogInstance;
            if (window == null) return;
            // WinUI 3 HWND association
            IntPtr hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("SubRip Subtitle", new List<string> { ".srt" });
            picker.SuggestedFileName = txtFileName.Text;

            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                string newContent = sb.ToString();
                await FileIO.WriteTextAsync(file, newContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                FilePathOpened = file.Path;
                txtFileName.Text = Path.GetFileNameWithoutExtension(file.Path);
                ToolTipService.SetToolTip(txtFileName, file.Path);

            }
        }

        private async void btnNewSubtitleFile_Click(object sender, RoutedEventArgs e)
        {
            Subtitles.Clear();
            txtFileName.Text = "Untitled";
            ToolTipService.SetToolTip(txtFileName, "");

            txtTranscript.Text = "";
            txtStartTime.Text = "00:00:00.000";
            txtEndTime.Text = "00:00:00.000";
            subDifference.Value = 0;
            FilePathOpened = "";
        }


        private void tglView_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string str)
            {
                if (str == "Text Editor")
                {
                    btn.Content = "Visual Editor";
                }
                else
                {
                    btn.Content = "Text Editor";
                }
            }
        }

        private void lstViewSubtitles_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is SubtitleCueModel subtitle)
            {
                txtTranscript.Text = subtitle.Text;
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened);
                ToolTipService.SetToolTip(txtFileName, FilePathOpened);

                var difference = subtitle.EndTime - subtitle.StartTime;
                subDifference.Value = difference.TotalSeconds;
                txtStartTime.Text = subtitle.StartString;
                txtEndTime.Text = subtitle.EndString;
            }
        }

        private void mnftEditSubtitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SubtitleCueModel subtitle)
            {
                txtTranscript.Text = subtitle.Text;
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened);
                ToolTipService.SetToolTip(txtFileName, FilePathOpened);

                var difference = subtitle.EndTime - subtitle.StartTime;
                subDifference.Value = difference.TotalSeconds;
                txtStartTime.Text = subtitle.StartString;
                txtEndTime.Text = subtitle.EndString;
            }
        }
        string FilePathOpened = "";
        private void mnftRemoveSubtitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SubtitleCueModel subtitle)
            {
                if (subtitle.Text == txtTranscript.Text)
                {
                    txtTranscript.Text = "";
                    subDifference.Value = 0;
                    txtStartTime.Text = "00:00:00.000";
                    txtEndTime.Text = "00:00:00.000";
                }
                Subtitles.Remove(subtitle);

                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
            }
        }

        private void mnftCopySubtitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SubtitleCueModel subtitle)
            {
                CopyToClipboard.CopyStringToClipboard(subtitle.Text);
            }
        }

        private void mnftCopySubtitlewithTimestamps_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SubtitleCueModel subtitle)
            {
                string start = subtitle.StartTime.ToString(@"hh\:mm\:ss\,fff");
                string end = subtitle.EndTime.ToString(@"hh\:mm\:ss\,fff");
                string sub = $"{start} --> {end} {subtitle.Text}";
                CopyToClipboard.CopyStringToClipboard(sub);
            }
        }

        private void mnftCopyFilePath_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard.CopyStringToClipboard(FilePathOpened);
        }

        private void mnftRenameSubtitle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftOpenFileLoc_Click(object sender, RoutedEventArgs e)
        {
            OpenFileLocation.PathSelect(FilePathOpened);
        }

        private void btnRemoveSubtitle_Click(object sender, RoutedEventArgs e)
        {
            if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
            {
                Subtitles.Remove(subtitle);
                txtTranscript.Text = "";
                subDifference.Value = 0;
                txtStartTime.Text = "00:00:00.000";
                txtEndTime.Text = "00:00:00.000";
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
            }
        }

        private void txtTranscript_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
            {
                subtitle.Text = txtTranscript.Text;
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
             
            }
        }

        private void btnAddSubtitle_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
