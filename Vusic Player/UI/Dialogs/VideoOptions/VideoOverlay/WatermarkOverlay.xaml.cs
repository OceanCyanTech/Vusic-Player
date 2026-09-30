using Microsoft.UI.Text;
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.Configuration.Playback;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoOverlay
{
    public class CountToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class WatermarkOverlay : UserControl
    {
        public WatermarkOverlay()
        {
            InitializeComponent();
            PresetPositions.Add("Top Left");
            PresetPositions.Add("Top Center");
            PresetPositions.Add("Top Right");
            PresetPositions.Add("Right");
            PresetPositions.Add("Left");
            PresetPositions.Add("Center");
            PresetPositions.Add("Bottom Left");
            PresetPositions.Add("Bottom Right");
            PresetPositions.Add("Bottom Center");

            var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies()
                               .OrderBy(f => f)
                               .ToList();

            cmbFontFamily.ItemsSource = systemFonts;

            // Fallback selection to Segoe UI if available
            var defaultFont = systemFonts.FirstOrDefault(f => f.Equals("Segoe UI Variable Text", StringComparison.OrdinalIgnoreCase))
                              ?? systemFonts.FirstOrDefault(f => f.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
                              ?? systemFonts.FirstOrDefault();

            cmbFontFamily.SelectedItem = defaultFont;
        }
        ObservableCollection<TimestampWatermark> timestamplabels = new ObservableCollection<TimestampWatermark>();
        List<string> AlreadyOccupiedPositions = new List<string>();
        List<string> PresetPositions = new List<string>();
        private string FormattedString(int index)
        {
            string Format = "";
            txtCustomFormat.Visibility = Visibility.Collapsed;
            switch (index)
            {
                case 0: Format = @"hh\:mm\:ss\:ff"; break;
                case 1: Format = @"hh\:mm\:ss\.ff"; break;
                case 2: Format = @"hh\:mm\:ss\;ff"; break;
                case 3: Format = @"hh\:mm\:ss\:fff"; break;
                case 4: Format = @"hh\:mm\:ss\.fff"; break;
                case 5: Format = @"hh\:mm\:ss\;fff"; break;
                case 6: Format = @"dd-MM-yyyy hh\:mm\:ss\:ff"; break;
                case 7: Format = @"MM-dd-yyyy hh\:mm\:ss\:ff"; break;
                case 8: Format = @"mm\:ss"; break;
                case 9: Format = @"mm\:ss\:ff"; break;
                case 10: Format = @"mm\:ss\:fff"; break;
                case 11: txtCustomFormat.Visibility = Visibility.Visible; Format = "Custom Format"; break;
            }

            return Format;
        }
        private void chkShow_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is TimestampWatermark watermark)
            {
                watermark.IsShown = true;
                int targetIndex = timestamplabels.IndexOf(watermark) + 1;

                // Push update to screen or unhide the label
                Screen.SetWatermarkVisibility(targetIndex, true);
            }
        }
        private void MenuFlyoutItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is TimestampWatermark itemToRemove)
            {
                int labelIndex = timestamplabels.IndexOf(itemToRemove) + 1;

                // 1. Hide the corresponding watermark TextBlock on screen
                Screen.SetWatermarkVisibility(labelIndex, false);

                // 2. Free the occupied position so it can be reused
                string freedPosition = itemToRemove.Position;
                AlreadyOccupiedPositions.Remove(freedPosition);

                if (!PresetPositions.Contains(freedPosition))
                {
                    PresetPositions.Insert(0, freedPosition); // Put back to available list
                }

                // 3. Remove from the data collection
                timestamplabels.Remove(itemToRemove);

                // 4. Update ListView selection gracefully
                if (timestamplabels.Count > 0)
                {
                    lstViewAddedTimeCodeLabels.SelectedIndex = Math.Min(labelIndex - 1, timestamplabels.Count - 1);
                }
                else
                {
                    lstViewAddedTimeCodeLabels.SelectedIndex = -1;
                }
            }
        }
        private void chkShow_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.DataContext is TimestampWatermark watermark)
            {
                watermark.IsShown = false;
                int targetIndex = timestamplabels.IndexOf(watermark) + 1;

                // Hide the label on the overlay
                Screen.SetWatermarkVisibility(targetIndex, false);
            }
        }
        int index = 1;
        private int GetPositionIndex(string positionName)
        {
            for (int i = 0; i < cmbPosition.Items.Count; i++)
            {
                string content = cmbPosition.Items[i] switch
                {
                    ComboBoxItem item => item.Content?.ToString() ?? "",
                    string str => str,
                    _ => cmbPosition.Items[i].ToString() ?? ""
                };

                if (string.Equals(content, positionName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }
        private void numBpm_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (_isUpdatingUi) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark selectedTimestamp)
            {
                double newBpm = double.IsNaN(args.NewValue) || args.NewValue <= 0 ? 120.0 : args.NewValue;
                selectedTimestamp.BPM = newBpm;

                int selectedIndex = timestamplabels.IndexOf(selectedTimestamp) + 1;
                Screen.CallWatermark(
                    selectedIndex,
                    selectedTimestamp.Position,
                    selectedTimestamp.Format,
                    selectedTimestamp.Offset,
                    selectedTimestamp.Mode,
                    DateTime.Now,
                    selectedTimestamp.FrameRate,
                    selectedTimestamp.BPM,
                    selectedTimestamp.BeatsPerBar
                );
            }
        }

        private void cmbBeatsPerBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingUi || cmbBeatsPerBar.SelectedItem == null) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark selectedTimestamp)
            {
                selectedTimestamp.BeatsPerBarIndex = cmbBeatsPerBar.SelectedIndex;
                selectedTimestamp.BeatsPerBar = GetSelectedBeatsPerBar();

                int selectedIndex = timestamplabels.IndexOf(selectedTimestamp) + 1;
                Screen.CallWatermark(
                    selectedIndex,
                    selectedTimestamp.Position,
                    selectedTimestamp.Format,
                    selectedTimestamp.Offset,
                    selectedTimestamp.Mode,
                    DateTime.Now,
                    selectedTimestamp.FrameRate,
                    selectedTimestamp.BPM,
                    selectedTimestamp.BeatsPerBar
                );
            }
        }

        private void cmbFrameRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingUi || cmbFrameRate.SelectedItem == null) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark selectedTimestamp)
            {
                selectedTimestamp.FrameRateIndex = cmbFrameRate.SelectedIndex;
                selectedTimestamp.FrameRate = GetSelectedFrameRate();

                int selectedIndex = timestamplabels.IndexOf(selectedTimestamp) + 1;
                Screen.CallWatermark(
                    selectedIndex,
                    selectedTimestamp.Position,
                    selectedTimestamp.Format,
                    selectedTimestamp.Offset,
                    selectedTimestamp.Mode,
                    DateTime.Now,
                    selectedTimestamp.FrameRate,
                    selectedTimestamp.BPM,
                    selectedTimestamp.BeatsPerBar
                );
            }
        }
        private void btnNewTimeCodedLabel_Click(object sender, RoutedEventArgs e)
        {
            if (timestamplabels.Count < 9 && PresetPositions.Count > 0)
            {
                var nextpos = PresetPositions[0];
                if (!AlreadyOccupiedPositions.Contains(nextpos))
                {
                    string format = FormattedString(cmbFormat.SelectedIndex);
                    var mode = (WatermarkMode)cmbMode.SelectedIndex;
                    TimeSpan offset = TimeSpan.FromMilliseconds(numOffset.Value);
                    int posIndex = GetPositionIndex(nextpos);

                    double frameRate = GetSelectedFrameRate();
                    double bpm = GetSelectedBpm();
                    int beatsPerBar = GetSelectedBeatsPerBar();

                    var newLabel = new TimestampWatermark
                    {
                        Label = $"Timestamp {index}",
                        Position = nextpos,
                        PositionIndex = posIndex,
                        Format = format,
                        FormatIndex = cmbFormat.SelectedIndex,
                        Offset = offset,
                        Mode = mode,
                        ModeIndex = cmbMode.SelectedIndex,
                        FrameRate = frameRate,
                        BPM = bpm,
                        BeatsPerBar = beatsPerBar
                    };

                    timestamplabels.Add(newLabel);

                    _isUpdatingUi = true;
                    try
                    {
                        lstViewAddedTimeCodeLabels.SelectedIndex = timestamplabels.Count - 1;
                        cmbPosition.SelectedIndex = posIndex;
                    }
                    finally
                    {
                        _isUpdatingUi = false;
                    }

                    Screen.CallWatermark(
                        index,
                        nextpos,
                        format,
                        offset,
                        mode,
                        DateTime.Now,
                        frameRate,
                        bpm,
                        beatsPerBar
                    );

                    AlreadyOccupiedPositions.Add(nextpos);
                    PresetPositions.Remove(nextpos);

                    index += 1;
                }
            }
        }
        private WatermarkMode WatermarkModeObtain()
        {
            WatermarkMode mode = WatermarkMode.ElapsedPlaybackTime;
            switch (cmbMode.SelectedIndex)
            {
                case 0: mode = WatermarkMode.ElapsedPlaybackTime; break;
                case 1: mode = WatermarkMode.RemainingPlaybackTime; break;
                case 2: mode = WatermarkMode.CurrentVsTotalDuration; break;
                case 3: mode = WatermarkMode.CurrentSystemTime; break;
                case 4: mode = WatermarkMode.CustomInitialOffset; break;
                case 5: mode = WatermarkMode.RunningFrames; break;
                case 6: mode = WatermarkMode.MusicalTimecode; break;
            }
            return mode;
        }
        private void lstViewAddedTimeCodeLabels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark selected)
            {
                _isUpdatingUi = true;
                try
                {
                    cmbPosition.SelectedIndex = selected.PositionIndex;
                    cmbFormat.SelectedIndex = selected.FormatIndex;
                    cmbMode.SelectedIndex = selected.ModeIndex;
                    numOffset.Value = selected.Offset.TotalMilliseconds;

                    // Update mode-specific inputs
                    numBpm.Value = selected.BPM > 0 ? selected.BPM : 120;

                    // Set frame rate combo matching string
                    string fpsString = selected.FrameRate.ToString(CultureInfo.InvariantCulture);
                    for (int i = 0; i < cmbFrameRate.Items.Count; i++)
                    {
                        if ((cmbFrameRate.Items[i] as ComboBoxItem)?.Content?.ToString() == fpsString)
                        {
                            cmbFrameRate.SelectedIndex = i;
                            break;
                        }
                    }

                    // Update panels visibility
                    pnlFramesConfig.Visibility = (selected.Mode == WatermarkMode.RunningFrames)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    pnlMusicalConfig.Visibility = (selected.Mode == WatermarkMode.MusicalTimecode)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                finally
                {
                    _isUpdatingUi = false;
                }
            }
        }
        WatermarkFontValues watermarkFontValues => WatermarkFontValues.Instance;
        private int GetSelectedLabelIndex(string label)
        {
            // Extracts the digit from strings like "Timestamp 1" -> 1
            if (string.IsNullOrWhiteSpace(label)) return -1;
            var parts = label.Split(' ');
            if (parts.Length > 1 && int.TryParse(parts[^1], out int index))
            {
                return index;
            }
            return -1;
        }
        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItems.Count != 1) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp &&
                cmbFontFamily.SelectedItem is string fontName)
            {
                timestamp.FontName = fontName;

                int index = GetSelectedLabelIndex(timestamp.Label);
                switch (index)
                {
                    case 1: watermarkFontValues.Label1FontFamily = fontName; break;
                    case 2: watermarkFontValues.Label2FontFamily = fontName; break;
                    case 3: watermarkFontValues.Label3FontFamily = fontName; break;
                    case 4: watermarkFontValues.Label4FontFamily = fontName; break;
                    case 5: watermarkFontValues.Label5FontFamily = fontName; break;
                    case 6: watermarkFontValues.Label6FontFamily = fontName; break;
                    case 7: watermarkFontValues.Label7FontFamily = fontName; break;
                    case 8: watermarkFontValues.Label8FontFamily = fontName; break;
                    case 9: watermarkFontValues.Label9FontFamily = fontName; break;
                }
            }
        }

        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItems.Count != 1) return;
            if (double.IsNaN(args.NewValue) || args.NewValue <= 0) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
            {
                double fontSize = args.NewValue;
                // If your TimestampWatermark has a FontSize property, assign it here:
                // timestamp.FontSize = fontSize;

                int index = GetSelectedLabelIndex(timestamp.Label);
                switch (index)
                {
                    case 1: watermarkFontValues.Label1FontSize = fontSize; break;
                    case 2: watermarkFontValues.Label2FontSize = fontSize; break;
                    case 3: watermarkFontValues.Label3FontSize = fontSize; break;
                    case 4: watermarkFontValues.Label4FontSize = fontSize; break;
                    case 5: watermarkFontValues.Label5FontSize = fontSize; break;
                    case 6: watermarkFontValues.Label6FontSize = fontSize; break;
                    case 7: watermarkFontValues.Label7FontSize = fontSize; break;
                    case 8: watermarkFontValues.Label8FontSize = fontSize; break;
                    case 9: watermarkFontValues.Label9FontSize = fontSize; break;
                }
            }
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItems.Count != 1) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
            {
                var brush = new SolidColorBrush(args.NewColor);
                // If your TimestampWatermark has a FontColor property, assign it here:
                // timestamp.FontColor = brush;

                int index = GetSelectedLabelIndex(timestamp.Label);
                switch (index)
                {
                    case 1: watermarkFontValues.Label1FontColor = brush; break;
                    case 2: watermarkFontValues.Label2FontColor = brush; break;
                    case 3: watermarkFontValues.Label3FontColor = brush; break;
                    case 4: watermarkFontValues.Label4FontColor = brush; break;
                    case 5: watermarkFontValues.Label5FontColor = brush; break;
                    case 6: watermarkFontValues.Label6FontColor = brush; break;
                    case 7: watermarkFontValues.Label7FontColor = brush; break;
                    case 8: watermarkFontValues.Label8FontColor = brush; break;
                    case 9: watermarkFontValues.Label9FontColor = brush; break;
                }
            }
        }

        private void cmbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
            {
                txtInvalidFormat.Visibility = Visibility.Collapsed;
                timestamp.FormatIndex = cmbFormat.SelectedIndex;
                int index = GetSelectedLabelIndex(timestamp.Label);
                string Format = FormattedString(cmbFormat.SelectedIndex);
                timestamp.Format = Format;
                if (Format == "Custom Format")
                {
                    return;
                }
                Debug.WriteLine(Format + " is the selected format");
                var mode = WatermarkModeObtain();
                Screen.CallWatermark(index, timestamp.Position, Format, TimeSpan.FromMilliseconds(numOffset.Value), mode, DateTime.Now);
            }
        }
        private void txtWatermarkCustomText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUi) return;

            if (lstViewAddedTimeCodeLabels?.SelectedItem is TimestampWatermark selectedTimestamp
                && selectedTimestamp.Mode == WatermarkMode.CustomStaticWatermark)
            {
                selectedTimestamp.Label = txtCustomStaticWatermark.Text;

                int selectedIndex = timestamplabels.IndexOf(selectedTimestamp) + 1;
                Screen.CallWatermark(
                    selectedIndex,
                    selectedTimestamp.Position,
                    selectedTimestamp.Format,
                    selectedTimestamp.Offset,
                    selectedTimestamp.Mode,
                    DateTime.Now,
                    selectedTimestamp.FrameRate,
                    selectedTimestamp.BPM,
                    selectedTimestamp.BeatsPerBar,
                    selectedTimestamp.Label
                );
            }
        }
        private void txtCustomFormat_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = txtCustomFormat.Text.Trim();
            bool isValid = FormatValidator.IsValidCustomFormat(input);

            if (isValid)
            {
                // Visual cue: reset error styling
                txtCustomFormat.BorderBrush = null;
                txtInvalidFormat.Visibility = Visibility.Collapsed;
                if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
                {
                    timestamp.FormatIndex = 0;
                    int index = GetSelectedLabelIndex(timestamp.Label);

                    var mode = WatermarkModeObtain();
                    Screen.CallWatermark(index, timestamp.Position, txtCustomFormat.Text, TimeSpan.FromMilliseconds(numOffset.Value), mode, DateTime.Now);
                }
            }
            else
            {
                // Visual cue: show red border and disable submit
                txtCustomFormat.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                txtInvalidFormat.Visibility = Visibility.Visible;

            }
        }

        private void tglBold_Checked(object sender, RoutedEventArgs e)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItems.Count != 1) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
            {
                var fontweight = tglBold.IsChecked ?? false ? FontWeights.Bold : FontWeights.Normal;
                // If your TimestampWatermark has a FontColor property, assign it here:
                // timestamp.FontColor = brush;

                int index = GetSelectedLabelIndex(timestamp.Label);
                switch (index)
                {
                    case 1: watermarkFontValues.Label1FontWeight = fontweight; break;
                    case 2: watermarkFontValues.Label2FontWeight = fontweight; break;
                    case 3: watermarkFontValues.Label3FontWeight = fontweight; break;
                    case 4: watermarkFontValues.Label4FontWeight = fontweight; break;
                    case 5: watermarkFontValues.Label5FontWeight = fontweight; break;
                    case 6: watermarkFontValues.Label6FontWeight = fontweight; break;
                    case 7: watermarkFontValues.Label7FontWeight = fontweight; break;
                    case 8: watermarkFontValues.Label8FontWeight = fontweight; break;
                    case 9: watermarkFontValues.Label9FontWeight = fontweight; break;
                }
            }
        }

        private void tglItalics_Checked(object sender, RoutedEventArgs e)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItems.Count != 1) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
            {
                var fontstyle = tglItalics.IsChecked ?? false ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
                // timestamp.FontColor = brush;

                int index = GetSelectedLabelIndex(timestamp.Label);
                switch (index)
                {
                    case 1: watermarkFontValues.Label1FontStyle = fontstyle; break;
                    case 2: watermarkFontValues.Label2FontStyle = fontstyle; break;
                    case 3: watermarkFontValues.Label3FontStyle = fontstyle; break;
                    case 4: watermarkFontValues.Label4FontStyle = fontstyle; break;
                    case 5: watermarkFontValues.Label5FontStyle = fontstyle; break;
                    case 6: watermarkFontValues.Label6FontStyle = fontstyle; break;
                    case 7: watermarkFontValues.Label7FontStyle = fontstyle; break;
                    case 8: watermarkFontValues.Label8FontStyle = fontstyle; break;
                    case 9: watermarkFontValues.Label9FontStyle = fontstyle; break;
                }
            }
        }

        private void numOffset_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestamp)
            {

                int index = GetSelectedLabelIndex(timestamp.Label);
                string Format = FormattedString(cmbFormat.SelectedIndex);
                if (Format == "Custom Format")
                {
                    return;
                }
                Debug.WriteLine(Format + " is the selected format");
                var mode = WatermarkModeObtain();
                Screen.CallWatermark(index, timestamp.Position, Format, TimeSpan.FromMilliseconds(numOffset.Value), mode, DateTime.Now);
            }
        }
        private double GetSelectedFrameRate()
        {
            if (cmbFrameRate.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Content?.ToString(), CultureInfo.InvariantCulture, out double fps))
            {
                return fps;
            }
            return 30.0; // Fallback default
        }

        private double GetSelectedBpm()
        {
            return double.IsNaN(numBpm.Value) || numBpm.Value <= 0 ? 120.0 : numBpm.Value;
        }

        private int GetSelectedBeatsPerBar()
        {
            return cmbBeatsPerBar.SelectedIndex switch
            {
                0 => 3,
                1 => 4,
                2 => 6,
                3 => 7,
                _ => 4
            };
        }
        private void cmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard against early firing during InitializeComponent()
            if (pnlFramesConfig == null || pnlMusicalConfig == null) return;

            var mode = (WatermarkMode)cmbMode.SelectedIndex;
            bool isStaticText = mode == WatermarkMode.CustomStaticWatermark;

            // Toggle panels according to the selected mode
            pnlFramesConfig.Visibility = (mode == WatermarkMode.RunningFrames)
                ? Visibility.Visible
                : Visibility.Collapsed;

            pnlMusicalConfig.Visibility = (mode == WatermarkMode.MusicalTimecode)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Toggle Static Text input vs Timecode-specific controls (Format / Offset)
            if (txtCustomStaticWatermark != null)
            {
                txtCustomStaticWatermark.Visibility = isStaticText ? Visibility.Visible : Visibility.Collapsed;
            }

            if (cmbFormat != null)
            {
                cmbFormat.Visibility = isStaticText ? Visibility.Collapsed : Visibility.Visible;
            }

            if (numOffset != null)
            {
                numOffset.Visibility = isStaticText ? Visibility.Collapsed : Visibility.Visible;
            }

            // If a label is selected in the list, update its settings and push to the screen
            if (!_isUpdatingUi && lstViewAddedTimeCodeLabels?.SelectedItem is TimestampWatermark selectedTimestamp)
            {
                selectedTimestamp.Mode = mode;
                selectedTimestamp.ModeIndex = cmbMode.SelectedIndex;
                selectedTimestamp.FrameRate = GetSelectedFrameRate();
                selectedTimestamp.BPM = GetSelectedBpm();
                selectedTimestamp.BeatsPerBar = GetSelectedBeatsPerBar();

                if (isStaticText && txtCustomStaticWatermark != null)
                {
                    selectedTimestamp.Label = txtCustomStaticWatermark.Text;
                }

                int selectedIndex = timestamplabels.IndexOf(selectedTimestamp) + 1;
                Screen.CallWatermark(
                    selectedIndex,
                    selectedTimestamp.Position,
                    selectedTimestamp.Format,
                    selectedTimestamp.Offset,
                    selectedTimestamp.Mode,
                    DateTime.Now,
                    selectedTimestamp.FrameRate,
                    selectedTimestamp.BPM,
                    selectedTimestamp.BeatsPerBar,
                    selectedTimestamp.Label // Pass custom text string
                );
            }
        }
        private bool _isUpdatingUi = false;

        private void cmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingUi || cmbPosition.SelectedItem == null) return;

            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark selectedTimestamp)
            {
                string newPosition = cmbPosition.SelectedItem switch
                {
                    ComboBoxItem item => item.Content?.ToString() ?? "Top Left",
                    string str => str,
                    _ => cmbPosition.SelectedItem.ToString() ?? "Top Left"
                };

                string oldPosition = selectedTimestamp.Position;
                if (oldPosition == newPosition) return;

                // Find if another label occupies the target position
                var conflictingLabel = timestamplabels.FirstOrDefault(p => p != selectedTimestamp && p.Position == newPosition);

                if (conflictingLabel != null)
                {
                    // Swap: Give conflicting label the old position & sync its PositionIndex
                    conflictingLabel.Position = oldPosition;
                    conflictingLabel.PositionIndex = GetPositionIndex(oldPosition);

                    int displacedIndex = timestamplabels.IndexOf(conflictingLabel) + 1;
                    Screen.CallWatermark(
                        displacedIndex,
                        oldPosition,
                        conflictingLabel.Format,
                        conflictingLabel.Offset,
                        conflictingLabel.Mode,
                        DateTime.Now
                    );
                }
                else
                {
                    // If it wasn't occupied, update the tracking lists
                    AlreadyOccupiedPositions.Remove(oldPosition);
                    AlreadyOccupiedPositions.Add(newPosition);

                    PresetPositions.Remove(newPosition);
                    if (!PresetPositions.Contains(oldPosition))
                    {
                        PresetPositions.Add(oldPosition);
                    }
                }

                // Apply new position and position index to selected timestamp
                selectedTimestamp.Position = newPosition;
                selectedTimestamp.PositionIndex = cmbPosition.SelectedIndex;

                int selectedIndex = timestamplabels.IndexOf(selectedTimestamp) + 1;
                Screen.CallWatermark(
                    selectedIndex,
                    newPosition,
                    selectedTimestamp.Format,
                    selectedTimestamp.Offset,
                    selectedTimestamp.Mode,
                    DateTime.Now
                );
            }
        }
    }
}
