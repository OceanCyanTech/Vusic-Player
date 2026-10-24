using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using Vusic_Player.Configuration;
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
            if(chkVerticalMode.IsChecked == true)
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
    }
}
