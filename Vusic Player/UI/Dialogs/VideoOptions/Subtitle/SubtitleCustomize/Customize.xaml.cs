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
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Customize = Vusic_Player.Configuration.Helper.SubtitlesProperties.Customize;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Subtitle.SubtitleCustomize
{
    public sealed partial class Customize : UserControl
    {
        public Customize()
        {
            InitializeComponent();
            cmbFonts.ItemsSource = Configuration.Helper.SubtitlesProperties.Customize.LoadFonts();
            cmbFonts.SelectedItem = "Segoe UI Variable Display";
        }
        #region Subtitle Customization Events

        private void cmbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFonts.SelectedItem is string fontName)
            {
                txtSample.FontFamily = new FontFamily(fontName);
            }
        }
        private void UpdateFontSizeUIFromStyle(string styleKey)
        {
            // These are the standard WinUI 3 sizes for the type ramp
            switch (styleKey)
            {
                case "DisplayTextBlockStyle": numFontSize.Value = 68; break;
                case "TitleLargeTextBlockStyle": numFontSize.Value = 40; break;
                case "TitleTextBlockStyle": numFontSize.Value = 28; break;
                case "SubtitleTextBlockStyle": numFontSize.Value = 20; break;
                case "BodyStrongTextBlockStyle":
                case "BodyTextBlockStyle": numFontSize.Value = 14; break;
                case "CaptionTextBlockStyle": numFontSize.Value = 12; break;
            }
        }
        private void cmbStyles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStyles.SelectedItem is ComboBoxItem selectedItem && txtSample != null)
            {
                if (selectedItem.Tag.ToString() is string styleKey)
                    if (App.Current.Resources.TryGetValue(styleKey, out object style))
                    {
                        txtSample.ClearValue(TextBlock.FontSizeProperty);
                        UpdateFontSizeUIFromStyle(styleKey);
                        txtSample.Style = style as Style;
                    }
            }
        }

        // --- Bold & Italics ---
        private void tglItalics_Checked(object sender, RoutedEventArgs e)
        {
            txtSample.FontStyle = (tglItalics.IsChecked == true) ?
                          Windows.UI.Text.FontStyle.Italic :
                          Windows.UI.Text.FontStyle.Normal;
        }

        private void tglItalics_Unchecked(object sender, RoutedEventArgs e)
        {
            txtSample.FontStyle = (tglItalics.IsChecked == true) ?
               Windows.UI.Text.FontStyle.Italic :
               Windows.UI.Text.FontStyle.Normal;
        }

        private void tglBold_Checked(object sender, RoutedEventArgs e)
        {
            txtSample.FontWeight = (tglBold.IsChecked == true) ? FontWeights.Bold : FontWeights.Normal;
        }

        private void tglBold_Unchecked(object sender, RoutedEventArgs e)
        {
            txtSample.FontWeight = (tglBold.IsChecked == true) ? FontWeights.Bold : FontWeights.Normal;
        }

        // --- Size & Color ---
        private void numFontSize_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (txtSample != null) txtSample.FontSize = args.NewValue;
        }

        private void clrPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            txtSample.Foreground = new SolidColorBrush(args.NewColor);
            rctColor.Fill = new SolidColorBrush(args.NewColor);
        }

        // --- Positioning ---
        private void cmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPosition == null || sldMargin == null) return;

            // Get the selected ComboBoxItem
            var selectedItem = cmbPosition.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            if (selectedItem.Tag == null) return;
            // Use the Tag property to determine the alignment


            switch (selectedItem.Tag.ToString())
            {

                case "BottomCenter":
                    Configuration.Helper.SubtitlesProperties.Customize.horizontalAlignment = HorizontalAlignment.Center;
                    Configuration.Helper.SubtitlesProperties.Customize.verticalAlignment = VerticalAlignment.Bottom;
                    Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(0, 0, 0, sldMargin.Value);
                    break;

                case "BottomLeft":
                    Configuration.Helper.SubtitlesProperties.Customize.horizontalAlignment = HorizontalAlignment.Left;
                    Configuration.Helper.SubtitlesProperties.Customize.verticalAlignment = VerticalAlignment.Bottom;
                    // 20px side padding, Slider for bottom margin
                    Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(20, 0, 0, sldMargin.Value);
                    break;

                case "BottomRight":
                    Configuration.Helper.SubtitlesProperties.Customize.horizontalAlignment = HorizontalAlignment.Right;
                    Configuration.Helper.SubtitlesProperties.Customize.verticalAlignment = VerticalAlignment.Bottom;
                    // 20px side padding, Slider for bottom margin
                    Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(0, 0, 20, sldMargin.Value);
                    break;

                case "TopCenter":
                    Configuration.Helper.SubtitlesProperties.Customize.horizontalAlignment = HorizontalAlignment.Center;
                    Configuration.Helper.SubtitlesProperties.Customize.verticalAlignment = VerticalAlignment.Top;
                    // Slider for top margin
                    Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(0, sldMargin.Value, 0, 0);
                    break;

            }


            // Default margin update for BottomCenter (using only the slider value)
            UpdateSubtitleMargin();
        }
        private void UpdateSubtitleMargin()
        {
            if (sldMargin == null) return;

            // If Position is Top, apply margin to Top; otherwise apply to Bottom
            var selectedItem = cmbPosition.SelectedItem as ComboBoxItem;
            if (selectedItem?.Tag?.ToString() == "TopCenter")
            {
                Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(0, sldMargin.Value, 0, 0);
            }
            else
            {
                Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(0, 0, 0, sldMargin.Value);
            }
        }

        // Handler for OceanSlider (using double obj signature)
        private void sldMargin_ValueChanged(double obj)
        {
            UpdateSubtitleMargin();
        }

        private void btnResetPlacement_Click(object sender, RoutedEventArgs e)
        {

            cmbPosition.SelectedIndex = 0;
            // Manually force the subtitle position to the bottom center
            Configuration.Helper.SubtitlesProperties.Customize.verticalAlignment = VerticalAlignment.Bottom;
            Configuration.Helper.SubtitlesProperties.Customize.horizontalAlignment = HorizontalAlignment.Center;
            sldMargin.Value = 0;
            // Manually kill the margin without affecting the slider thumb position
            Configuration.Helper.SubtitlesProperties.Customize.thickness = new Thickness(0, 0, 0, 0);

        }
        private void StoreValues()
        {
            Configuration.Helper.SubtitlesProperties.Customize.fontFamily = txtSample.FontFamily;
            // Syncing from txtSample to Configuration.Helper.SubtitlesProperties.Customize
            Configuration.Helper.SubtitlesProperties.Customize.FontSize = txtSample.FontSize;
            Configuration.Helper.SubtitlesProperties.Customize.FontWeight = txtSample.FontWeight;
            Configuration.Helper.SubtitlesProperties.Customize.FontStyle = txtSample.FontStyle;
            Configuration.Helper.SubtitlesProperties.Customize.FontStretch = txtSample.FontStretch;

            Configuration.Helper.SubtitlesProperties.Customize.Foreground = txtSample.Foreground;
            Configuration.Helper.SubtitlesProperties.Customize.TextDecorations = txtSample.TextDecorations;
            Configuration.Helper.SubtitlesProperties.Customize.CharacterSpacing = txtSample.CharacterSpacing;
            Configuration.Helper.SubtitlesProperties.Customize.TextAlignment = txtSample.TextAlignment;

            // Using 'style' (lowercase) as per your field definition
            Configuration.Helper.SubtitlesProperties.Customize.style = txtSample.Style;
        }
        // --- Global Actions ---
        private void btnApplySettings_Click(object sender, RoutedEventArgs e)
        {
            StoreValues();
            Configuration.Helper.SubtitlesProperties.Customize.Call();
        }

        private void btnResetSubtitleSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSubtitleSettings();

        }
        private void ResetSubtitleSettings()
        {
            // 1. Reset to standard WinUI 3 Typography
            // This applies the "Title" style (Semi-bold, larger font)
            if (App.Current.Resources.TryGetValue("TitleTextBlockStyle", out object titleStyle))
            {
                txtSample.Style = titleStyle as Style;
            }

            // 2. Revert to System Default Font Family
            // Setting this to null/default allows the Style to take over
            txtSample.FontFamily = (FontFamily)Application.Current.Resources["ContentControlThemeFontFamily"];

            // 3. Reset Foreground to Theme Default
            // Using 'null' or the system brush ensures it switches correctly between Light/Dark mode
            txtSample.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];

            // 4. Reset specific overrides
            txtSample.FontSize = 28; // Default for TitleTextBlockStyle is usually around 28px
            txtSample.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
            txtSample.FontStyle = Windows.UI.Text.FontStyle.Normal;
            txtSample.TextDecorations = Windows.UI.Text.TextDecorations.None;
            txtSample.CharacterSpacing = 0;

            // 5. Update UI Controls to match the reset
            cmbFonts.SelectedItem = "Segoe UI Variable Display";
            numFontSize.Value = 28;
            tglBold.IsChecked = false;
            tglItalics.IsChecked = false;
            rctColor.Fill = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            StoreValues();
            Configuration.Helper.SubtitlesProperties.Customize.Call();
        }

        #endregion


    }
}
