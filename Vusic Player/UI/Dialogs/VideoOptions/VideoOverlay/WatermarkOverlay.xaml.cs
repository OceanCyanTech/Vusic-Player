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
        int index = 1;
        private void btnNewTimeCodedLabel_Click(object sender, RoutedEventArgs e)
        {
            if (timestamplabels.Count != 9)
            {
                var nextpos = PresetPositions[0];
                if (!AlreadyOccupiedPositions.Contains(nextpos))
                {

                    timestamplabels.Add(new TimestampWatermark { Label = $"Timestamp {index}", Position = nextpos });
                    lstViewAddedTimeCodeLabels.SelectedIndex = timestamplabels.Count - 1;
                    string Format = FormattedString(cmbFormat.SelectedIndex);
                    Screen.CallWatermark(index, nextpos, Format);
                    AlreadyOccupiedPositions.Add(nextpos);
                    PresetPositions.Remove(nextpos);
                    index += 1;
                }

            }
        }

        private void lstViewAddedTimeCodeLabels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestampwatermark)
            {
                cmbFormat.SelectedIndex = timestampwatermark.FormatIndex;
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
                timestamp.FormatIndex = 0;
                int index = GetSelectedLabelIndex(timestamp.Label);
                string Format = FormattedString(cmbFormat.SelectedIndex);
                if (Format == "Custom Format")
                {
                    return;
                }
                Debug.WriteLine(Format + " is the selected format");
                Screen.CallWatermark(index, timestamp.Position, Format);
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

                    Screen.CallWatermark(index, timestamp.Position, txtCustomFormat.Text);
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
    }
}
