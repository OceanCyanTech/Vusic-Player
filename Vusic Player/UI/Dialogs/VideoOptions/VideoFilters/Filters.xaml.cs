
using FlyleafLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vortice.Direct3D11;
using Vusic_Player.MediaProperties.VideoProperties;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Filter = Vusic_Player.MediaProperties.VideoProperties.Filter;


namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoFilters
{
    public sealed partial class Filters : UserControl
    {
        public Filters()
        {
            InitializeComponent();
        }
        #region Video Filter Events

        private void btnResetAll_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Brightness, FLFilters.Brightness, 0, sldBrightness, numBrightness, txtBrightnessPercentage);
            Filter.UpdateFilter(VideoProcessorFilter.Hue, FLFilters.Hue, 0, sldHue, numHue, txtHueDisplay);
            Filter.UpdateFilter(VideoProcessorFilter.Contrast, FLFilters.Contrast, 0, sldContrast, numContrast, txtContrastDisplay);
            Filter.UpdateFilter(VideoProcessorFilter.Saturation, FLFilters.Saturation, 0, sldSaturation, numSaturation, txtSaturationDisplay);
        }

        // --- Brightness ---
        private void sldBrightness_ValueChanged(double obj)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Brightness, FLFilters.Brightness, (int)obj, sldBrightness, numBrightness, txtBrightnessPercentage);
        }

        private void numBrightness_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Brightness, FLFilters.Brightness, (int)numBrightness.Value, sldBrightness, numBrightness, txtBrightnessPercentage);
        }

        private void btnResetBrightness_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Brightness, FLFilters.Brightness, 0, sldBrightness, numBrightness, txtBrightnessPercentage);
        }
        //Hue
        private void sldHue_ValueChanged(double obj)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Hue, FLFilters.Hue, (int)obj, sldHue, numHue, txtHueDisplay);
        }

        private void numHue_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Hue, FLFilters.Hue, (int)numHue.Value, sldHue, numHue, txtHueDisplay);
        }

        private void btnResetHue_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Hue, FLFilters.Hue, 0, sldHue, numHue, txtHueDisplay);
        }

        // --- Contrast ---
        private void sldContrast_ValueChanged(double obj)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Contrast, FLFilters.Contrast, (int)obj, sldContrast, numContrast, txtContrastDisplay);
        }

        private void numContrast_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Contrast, FLFilters.Contrast, (int)numContrast.Value, sldContrast, numContrast, txtContrastDisplay);
        }

        private void btnResetContrast_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Contrast, FLFilters.Contrast, 0, sldContrast, numContrast, txtContrastDisplay);
        }

        // --- Saturation ---
        private void sldSaturation_ValueChanged(double obj)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Saturation, FLFilters.Saturation, (int)obj, sldSaturation, numSaturation, txtSaturationDisplay);
        }

        private void numSaturation_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Saturation, FLFilters.Saturation, (int)numSaturation.Value, sldSaturation, numSaturation, txtSaturationDisplay);
        }

        private void btnResetSaturation_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Filter.UpdateFilter(VideoProcessorFilter.Saturation, FLFilters.Saturation, 0, sldSaturation, numSaturation, txtSaturationDisplay);
        }
        #endregion

    }
}
