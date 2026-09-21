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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.SubtitlesProperties;
using Vusic_Player.Configuration.Helper.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Stream = Vusic_Player.Configuration.Helper.SubtitlesProperties.Stream;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
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

    public sealed partial class LyricEditorUI : UserControl
    {


        private void SubtitleEditorUI_Loaded(object sender, RoutedEventArgs e)
        {
           
            PlayerService.SeekCompleted += PlayerService_SeekCompleted;
        }
        

        private void PlayerService_SeekCompleted(int obj)
        {
  
            TimeSpan timespan = TimeSpan.FromMilliseconds(obj);
            Debug.WriteLine("RECEIVED SEEK: " + timespan.ToString());
            var currentCue = Subtitles.FirstOrDefault(cue =>
    timespan >= cue.StartTime && timespan <= cue.EndTime);
            if (currentCue != null)
            {
                // Avoid re-triggering selection changes if it's already selected
                if (lstViewSubtitles.SelectedItem != currentCue)
                {
                    lstViewSubtitles.SelectedItem = currentCue;
                    txtTranscript.Text = currentCue.Text;
                    txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened);
                    ToolTipService.SetToolTip(txtFileName, FilePathOpened);

                    var difference = currentCue.EndTime - currentCue.StartTime;
                    subDifference.Value = difference.TotalSeconds;
                    txtStartTime.Text = currentCue.StartString;
                    txtEndTime.Text = currentCue.EndString;
                    // Automatically scroll to keep the active subtitle in view
                    lstViewSubtitles.ScrollIntoView(currentCue);
                }
            }
            else
            {
                // Optional: Deselect if position is currently between cues
                lstViewSubtitles.SelectedItem = null;
            }
        }

        private void btnInsertAbove_Click(object sender, RoutedEventArgs e)
        {
            if (Subtitles.Count == 0)
            {
                var startTime = TimeSpan.Zero;

                var endTime = startTime.Add(TimeSpan.FromSeconds(3));
                string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                Subtitles.Add(new SubtitleCueModel { StartTime = TimeSpan.Zero, EndTime = endTime, StartString = startTime.ToString(format), EndString = endTime.ToString(endFormat) });
                return;
            }
            if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
            {
                int index = Subtitles.IndexOf(subtitle);
                Debug.WriteLine(index);
                var endTime = subtitle.EndTime;
                var startTime = subtitle.StartTime;
                string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                var newSubtitle = new SubtitleCueModel { StartTime = startTime, EndTime = endTime, StartString = startTime.ToString(format), EndString = endTime.ToString(endFormat) };
                for (int i = index; i < Subtitles.Count; i++)
                {
                    Debug.WriteLine(i);

                    var nextitem = Subtitles[i];
                    nextitem.StartTime = nextitem.StartTime.Add(TimeSpan.FromSeconds(3.5));
                    nextitem.EndTime = nextitem.EndTime.Add(TimeSpan.FromSeconds(3.5));

                    string format2 = nextitem.StartTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    string endFormat2 = nextitem.EndTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    nextitem.StartString = nextitem.StartTime.ToString(format2);
                    nextitem.EndString = nextitem.EndTime.ToString(endFormat2);

                }

                Subtitles.Insert(index, newSubtitle);
                lstViewSubtitles.SelectedIndex = index;
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
            }

        }

        private void btnInsertBelow_Click(object sender, RoutedEventArgs e)
        {
            if (Subtitles.Count == 0)
            {
                var startTime = TimeSpan.Zero;

                var endTime = startTime.Add(TimeSpan.FromSeconds(3));
                string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                Subtitles.Add(new SubtitleCueModel { StartTime = TimeSpan.Zero, EndTime = endTime, StartString = startTime.ToString(format), EndString = endTime.ToString(endFormat) });
                return;
            }
            if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
            {
                int index = Subtitles.IndexOf(subtitle);
                Debug.WriteLine(index);
                var startTime = subtitle.EndTime.Add(TimeSpan.FromMilliseconds(500));
                var endTime = startTime.Add(TimeSpan.FromSeconds(3));
                string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                var newSubtitle = new SubtitleCueModel { StartTime = startTime, EndTime = endTime, StartString = startTime.ToString(format), EndString = endTime.ToString(endFormat) };
                Subtitles.Insert(index + 1, newSubtitle);
                lstViewSubtitles.SelectedIndex = index + 1;

                for (int i = index + 2; i < Subtitles.Count; i++)
                {
                    Debug.WriteLine(i);

                    var nextitem = Subtitles[i];
                    nextitem.StartTime = nextitem.StartTime.Add(TimeSpan.FromSeconds(3.5));
                    nextitem.EndTime = nextitem.EndTime.Add(TimeSpan.FromSeconds(3.5));

                    string format2 = nextitem.StartTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    string endFormat2 = nextitem.EndTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    nextitem.StartString = nextitem.StartTime.ToString(format2);
                    nextitem.EndString = nextitem.EndTime.ToString(endFormat2);

                }
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
            }
        }


        ObservableCollection<SubtitleCueModel> Subtitles = new ObservableCollection<SubtitleCueModel>();
        private async void btnLoadSubtitleFile_Click(object sender, RoutedEventArgs e)
        {
            if (App.SubtitleEditorDialogInstance == null) return;

            Subtitles.Clear();
            var subtitlefile = await FilePickers.LyricPicker.PickSingle(App.SubtitleEditorDialogInstance, "Load Lyric");
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
         @"(?<start>\d{2}:\d{2}:\d{2}[,\.]\d{3})\s*-->\s*(?<end>\d{2}:\d{2}:\d{2}[,\.]\d{3})[^\r\n]*(?:\r?\n(?<text>(?:(?!\r?\n\r?\n|\r?\n\d+\r?\n|\r?\n\d{2}:\d{2}).)*))?",
         RegexOptions.Singleline);
            string rawContent = await FileIO.ReadTextAsync(subtitlefile);
            rawcontent = rawContent;
            if (tglView.Content.ToString() == "Visual Editor")
            {
                txtTextEditor.Text = rawContent;
                return;
            }
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
        private async void SaveFile()
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
                rawcontent = sb.ToString();

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
                picker.FileTypeChoices.Add("Lyric File LRC", new List<string> { ".lrc" });
                picker.SuggestedFileName = txtFileName.Text;

                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    string newContent = sb.ToString();
                    rawcontent = newContent;

                    await FileIO.WriteTextAsync(file, newContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    FilePathOpened = file.Path;

                    txtFileName.Text = Path.GetFileNameWithoutExtension(file.Path);
                    ToolTipService.SetToolTip(txtFileName, file.Path);

                }
            }

        }
        private async void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        private async void SaveAsFile()
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
            picker.FileTypeChoices.Add("Lyric File LRC", new List<string> { ".lrc" });
            picker.SuggestedFileName = txtFileName.Text;
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                string newContent = sb.ToString();
                rawcontent = newContent;

                await FileIO.WriteTextAsync(file, newContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                FilePathOpened = file.Path;
                txtFileName.Text = Path.GetFileNameWithoutExtension(file.Path);
                ToolTipService.SetToolTip(txtFileName, file.Path);

            }

        }
        private async void btnSaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            SaveAsFile();
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
            txtTextEditor.Text = "";

        }


        private void tglView_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string str)
            {
                if (str == "Text Editor")
                {
                    btn.Content = "Visual Editor";
                    grdTextEditor.Visibility = Visibility.Visible;
                    grdVisualEditor.Visibility = Visibility.Collapsed;
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
                    txtTextEditor.Text = sb.ToString();
                    rawcontent = sb.ToString();
                }
                else
                {
                    btn.Content = "Text Editor";
                    grdTextEditor.Visibility = Visibility.Collapsed;
                    grdVisualEditor.Visibility = Visibility.Visible;
                    Subtitles.Clear();

                    // 1. Force all newlines (lone \r, \n, or \r\n) to standard \r\n
                    string rawContent = txtTextEditor.Text;
                    rawContent = rawContent.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

                    // 2. This regex will now match consistently every time
                    var regex = new Regex(
    @"(?<start>\d{2}:\d{2}:\d{2}[,\.]\d{3})\s*-->\s*(?<end>\d{2}:\d{2}:\d{2}[,\.]\d{3})[^\r\n]*\r\n(?<text>.*?(?=(\r\n\s*\r\n|\r\n(?:\d+\r\n)?\d{2}:\d{2}:\d{2}|\z)))",
    RegexOptions.Singleline);

                    MatchCollection matches = regex.Matches(rawContent);

                    foreach (Match match in matches)
                    {
                        string text = match.Groups["text"].Value.Trim();
                        string startRaw = match.Groups["start"].Value.Replace(',', '.');
                        string endRaw = match.Groups["end"].Value.Replace(',', '.');

                        if (TimeSpan.TryParse(startRaw, out TimeSpan startTime) &&
                            TimeSpan.TryParse(endRaw, out TimeSpan endTime))
                        {
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
            if (FilePathOpened == "")
            {
                SaveAsFile();
            }
            else
            {
                ttRenameFile.IsOpen = true;
                txtRename.Text = txtFileName.Text;
            }
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


        string rawcontent = "";
        private void txtStartTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] allowedFormats = new[]
{
    @"hh\:mm\:ss\.fff", // 00:01:23.456 (WebVTT style)
    @"mm\:ss\.fff", // 00:01:23.456 (WebVTT style)
    @"mm\:ss\,fff",  // 00:01:23,456 (SRT style)
    @"hh\:mm\:ss\,fff"  // 00:01:23,456 (SRT style)
};
            bool isValid = TimeSpan.TryParseExact(
    txtStartTime.Text,
allowedFormats,
    CultureInfo.InvariantCulture,
    out TimeSpan parsedTime);

            if (!isValid)
            {
                ifbErrorStartTime.Title = "Error";
                ifbErrorStartTime.Message = "Invalid format for start time.";
                ifbErrorStartTime.Severity = InfoBarSeverity.Error;
                ifbErrorStartTime.IsOpen = true;
            }
            else
            {
                ifbErrorStartTime.IsOpen = false;
                if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
                {
                    subtitle.StartTime = parsedTime;
                    string format = parsedTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    subtitle.StartString = parsedTime.ToString(format);
                }
            }

        }

        private void txtEndTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] allowedFormats = new[]
{
    @"hh\:mm\:ss\.fff", // 00:01:23.456 (WebVTT style)
    @"mm\:ss\.fff", // 00:01:23.456 (WebVTT style)
    @"mm\:ss\,fff",  // 00:01:23,456 (SRT style)
    @"hh\:mm\:ss\,fff"  // 00:01:23,456 (SRT style)
};
            bool isValid = TimeSpan.TryParseExact(
    txtEndTime.Text,
allowedFormats,
    CultureInfo.InvariantCulture,
    out TimeSpan parsedTime);

            if (!isValid)
            {
                ifbErrorEndTime.Title = "Error";
                ifbErrorEndTime.Message = "Invalid format for end time.";
                ifbErrorEndTime.Severity = InfoBarSeverity.Error;
                ifbErrorEndTime.IsOpen = true;
            }
            else
            {
                ifbErrorEndTime.IsOpen = false;
                if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
                {
                    subtitle.EndTime = parsedTime;
                    string format = parsedTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    subtitle.EndString = parsedTime.ToString(format);
                }
            }
        }

        private void subDifference_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (lstViewSubtitles.SelectedItem is SubtitleCueModel subtitle)
            {
                var endtime = subtitle.StartTime.Add(TimeSpan.FromSeconds(subDifference.Value));

                string format = endtime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";

                txtEndTime.Text = endtime.ToString(format);

                subtitle.EndTime = endtime;
                subtitle.EndString = txtEndTime.Text;

            }
        }

        private async void btnOfficialRename_Click(object sender, RoutedEventArgs e)
        {
            if (txtRename.Text == "") return;
            if (File.Exists(FilePathOpened))
            {
                var listoflocking = GetLockingProcess.GetLockingProcesses(FilePathOpened);
                if (listoflocking.Count == 0)
                {
                    var file = await StorageFile.GetFileFromPathAsync(FilePathOpened);

                    try
                    {
                        await file.RenameAsync(txtRename.Text + ".lrc", NameCollisionOption.FailIfExists);
                    }
                    catch (Exception ex)
                    {
                        ifbErrorRename.IsOpen = true;
                        ifbErrorRename.Severity = InfoBarSeverity.Error;
                        ifbErrorRename.Message = "Cannot rename file. Unexpected Error: " + ex.Message;
                        ifbErrorRename.Title = "Rename Error";
                    }
                    finally
                    {
                        txtFileName.Text = txtRename.Text;
                        FilePathOpened = file.Path;
                        ttRenameFile.IsOpen = false;
                    }
                }
                else
                {
                    ifbErrorRename.IsOpen = true;
                    ifbErrorRename.Severity = InfoBarSeverity.Error;
                    ifbErrorRename.Message = "Cannot Rename File as it is in use by one or more processes.";
                    ifbErrorRename.Title = "Rename Error";
                }
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.Undo();
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.Redo();
        }

        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.CutSelectionToClipboard();
        }

        private void txtTextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTextEditor.Text == rawcontent)
            {
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened);
            }
            else
            {
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";

            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.CopySelectionToClipboard();
        }

        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.PasteFromClipboard();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.SelectAll();
            txtTextEditor.Focus(FocusState.Programmatic);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.Text = txtTextEditor.Text.Replace(txtTextEditor.SelectedText, "");
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            ttFind.IsOpen = true;
            asbFind.Text = txtTextEditor.SelectedText;
        }





        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            txtTextEditor.Text = txtTextEditor.Text.Replace(txtTextEditor.SelectedText, asbReplace.Text);

        }
        public void Save()
        {
            SaveFile();
        }
        private void btnFindActual_Click(object sender, RoutedEventArgs e)
        {
            string query = asbFind.Text;
            if (string.IsNullOrEmpty(query))
                return;

            bool matchCase = btnCaseSensitive.IsChecked ?? false;
            //     bool wrapAround = btnWrapAround.IsChecked ?? false;

            var comparison = matchCase
                ? StringComparison.CurrentCulture
                : StringComparison.CurrentCultureIgnoreCase;

            int startIndex = txtTextEditor.SelectionStart + txtTextEditor.SelectionLength;

            // 1. Search forward from the current cursor/selection end
            int index = txtTextEditor.Text.IndexOf(query, startIndex, comparison);

            // 2. Wrap around to the start (0) if enabled and not found ahead
            if (index == -1 && startIndex > 0)
            {
                index = txtTextEditor.Text.IndexOf(query, 0, comparison);
            }

            // 3. Handle match vs. no match
            if (index != -1)
            {
                // Dismiss InfoBar on success
                ifbFindError.IsOpen = false;

                txtTextEditor.Focus(FocusState.Programmatic);
                txtTextEditor.Select(index, query.Length);
            }
            else
            {
                // Display InfoBar with failure notice
                ifbFindError.Title = "No results found";
                ifbFindError.Message = $"Could not find \"{query}\".";
                ifbFindError.Severity = InfoBarSeverity.Warning;
                ifbFindError.IsOpen = true;
            }
        }

        private void mnftInsertSubtitleAbove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SubtitleCueModel subtitle)
            {
                int index = Subtitles.IndexOf(subtitle);
                Debug.WriteLine(index);
                var endTime = subtitle.EndTime;
                var startTime = subtitle.StartTime;
                string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                var newSubtitle = new SubtitleCueModel { StartTime = startTime, EndTime = endTime, StartString = startTime.ToString(format), EndString = endTime.ToString(endFormat) };
                for (int i = index; i < Subtitles.Count; i++)
                {
                    Debug.WriteLine(i);

                    var nextitem = Subtitles[i];
                    nextitem.StartTime = nextitem.StartTime.Add(TimeSpan.FromSeconds(3.5));
                    nextitem.EndTime = nextitem.EndTime.Add(TimeSpan.FromSeconds(3.5));

                    string format2 = nextitem.StartTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    string endFormat2 = nextitem.EndTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    nextitem.StartString = nextitem.StartTime.ToString(format2);
                    nextitem.EndString = nextitem.EndTime.ToString(endFormat2);

                }

                Subtitles.Insert(index, newSubtitle);
                lstViewSubtitles.SelectedIndex = index;
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
            }

        }
        public Stream ViewModel { get; } = new();

        private void mnftInsertSubtitleBelow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SubtitleCueModel subtitle)
            {
                int index = Subtitles.IndexOf(subtitle);
                Debug.WriteLine(index);
                var startTime = subtitle.EndTime.Add(TimeSpan.FromMilliseconds(500));
                var endTime = startTime.Add(TimeSpan.FromSeconds(3));
                string format = startTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                string endFormat = endTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                var newSubtitle = new SubtitleCueModel { StartTime = startTime, EndTime = endTime, StartString = startTime.ToString(format), EndString = endTime.ToString(endFormat) };
                Subtitles.Insert(index + 1, newSubtitle);
                lstViewSubtitles.SelectedIndex = index + 1;

                for (int i = index + 2; i < Subtitles.Count; i++)
                {
                    Debug.WriteLine(i);

                    var nextitem = Subtitles[i];
                    nextitem.StartTime = nextitem.StartTime.Add(TimeSpan.FromSeconds(3.5));
                    nextitem.EndTime = nextitem.EndTime.Add(TimeSpan.FromSeconds(3.5));

                    string format2 = nextitem.StartTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    string endFormat2 = nextitem.EndTime.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff";
                    nextitem.StartString = nextitem.StartTime.ToString(format2);
                    nextitem.EndString = nextitem.EndTime.ToString(endFormat2);

                }
                txtFileName.Text = Path.GetFileNameWithoutExtension(FilePathOpened) + "*";
                ToolTipService.SetToolTip(txtFileName, "Pending Changes to be saved");
            }

        }

   

        private void btnGetCurrentTimeStart_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer != null)
            {
                txtStartTime.Text = (TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime)).ToString(@"hh\:mm\:ss\.fff");
            }
        }
        private static string GetFFmpegPath()
        {
            // Resolves bin/Debug/.../FFmpeg/ffmpeg.exe
            string baseDir = AppContext.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                throw new FileNotFoundException(
                    $"Bundled ffmpeg.exe was not found at: {ffmpegPath}. " +
                    $"Ensure 'Copy to Output Directory' is set in your project file.");
            }

            return ffmpegPath;
        }
        private void btnGetCurrentTimeEnd_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer != null)
            {
                txtEndTime.Text = (TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime)).ToString(@"hh\:mm\:ss\.fff");
            }
        }




        public LyricEditorUI()
        {
            InitializeComponent();
        }

    }
}
