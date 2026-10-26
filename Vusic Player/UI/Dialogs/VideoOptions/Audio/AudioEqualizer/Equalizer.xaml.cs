// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.MediaProperties.AudioProperties;


namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioEqualizer
{
    public class GainDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double val)
            {
                return val > 0 ? $"+{val:0} dB" : $"{val:0} dB";
            }
            return "0 dB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public sealed partial class Equalizer : UserControl
    {
        public Equalizer()
        {
            InitializeComponent();
        }
        ObservableCollection<EqualizerPreset> presets = new ObservableCollection<EqualizerPreset>();
        private async void LoadPresets()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var presetseq = currentSettings.EqPresets;
            presets = new ObservableCollection<EqualizerPreset>(presetseq);
        }
        private void OceanSlider_ValueChanged(double obj)
        {

        }

        private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void gainCommon_ValueChanged(double obj)
        {

        }

        private void btnApply_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            if (chkVerticalMode.IsChecked == true)
            {
                double[] gains = [gain31Vertical.Value, gain63Vertical.Value, gain125Vertical.Value, gain250Vertical.Value, gain500Vertical.Value, gain1000Vertical.Value, gain2000Vertical.Value, gain4000Vertical.Value, gain8000Vertical.Value, gain16000Vertical.Value];
                EqualizerService.Create(gains);
            }
            else
            {
                double[] gains = [gain31.Value, gain63.Value, gain125.Value, gain250.Value, gain500.Value, gain1000.Value, gain2000.Value, gain4000.Value, gain8000.Value, gain16000.Value];
                EqualizerService.Create(gains);
            }
        }
        private bool _isSyncing;

        private void SyncHorizontalToVertical()
        {
            if (_isSyncing) return;

            try
            {
                _isSyncing = true;
                gain31Vertical.Value = gain31.Value;
                gain63Vertical.Value = gain63.Value;
                gain125Vertical.Value = gain125.Value;
                gain250Vertical.Value = gain250.Value;
                gain500Vertical.Value = gain500.Value;
                gain1000Vertical.Value = gain1000.Value;
                gain2000Vertical.Value = gain2000.Value;
                gain4000Vertical.Value = gain4000.Value;
                gain8000Vertical.Value = gain8000.Value;
                gain16000Vertical.Value = gain16000.Value;
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void SyncVerticalToHorizontal()
        {
            if (_isSyncing) return;

            try
            {
                _isSyncing = true;
                gain31.Value = gain31Vertical.Value;
                gain63.Value = gain63Vertical.Value;
                gain125.Value = gain125Vertical.Value;
                gain250.Value = gain250Vertical.Value;
                gain500.Value = gain500Vertical.Value;
                gain1000.Value = gain1000Vertical.Value;
                gain2000.Value = gain2000Vertical.Value;
                gain4000.Value = gain4000Vertical.Value;
                gain8000.Value = gain8000Vertical.Value;
                gain16000.Value = gain16000Vertical.Value;
            }
            finally
            {
                _isSyncing = false;
            }
        }
        private void chkVerticalMode_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (chkVerticalMode.IsChecked == true)
            {
                grdHorizontalSliders.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                grdVerticalSliders.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                SyncHorizontalToVertical();
            }
            else
            {
                grdHorizontalSliders.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                grdVerticalSliders.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                SyncVerticalToHorizontal();
            }
        }

        private void btnSavePreset_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ttPresetName.IsOpen = true;
        }

        private async void btnCreatePreset_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (txtPresetName.Text == "") return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (chkVerticalMode.IsChecked == true)
            {
                currentSettings.EqPresets.Add(new EqualizerPreset
                {
                    PresetName = txtPresetName.Text,
                    Gain31 = gain31Vertical.Value,
                    Gain63 = gain63Vertical.Value,
                    Gain125 = gain125Vertical.Value,
                    Gain250 = gain250Vertical.Value,
                    Gain500 = gain500Vertical.Value,
                    Gain1000 = gain1000Vertical.Value,
                    Gain2000 = gain2000Vertical.Value,
                    Gain4000 = gain4000Vertical.Value,
                    Gain8000 = gain8000Vertical.Value,
                    Gain16000 = gain16000Vertical.Value
                });
            }
            else
            {
                currentSettings.EqPresets.Add(new EqualizerPreset
                {
                    PresetName = txtPresetName.Text,
                    Gain31 = gain31.Value,
                    Gain63 = gain63.Value,
                    Gain125 = gain125.Value,
                    Gain250 = gain250.Value,
                    Gain500 = gain500.Value,
                    Gain1000 = gain1000.Value,
                    Gain2000 = gain2000.Value,
                    Gain4000 = gain4000.Value,
                    Gain8000 = gain8000.Value,
                    Gain16000 = gain16000.Value
                }
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            ttPresetName.IsOpen = false;
        }
    }
}
