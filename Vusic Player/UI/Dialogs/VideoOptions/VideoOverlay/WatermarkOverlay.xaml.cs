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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
        }
        ObservableCollection<TimestampWatermark> timestamplabels = new ObservableCollection<TimestampWatermark>();
        List<string> AlreadyOccupiedPositions = new List<string>();
        List<string> PresetPositions = new List<string>();
        int index = 1;
        private void btnNewTimeCodedLabel_Click(object sender, RoutedEventArgs e)
        {
            if (timestamplabels.Count != 9)
            {
                var nextpos = PresetPositions[0];
                if (!AlreadyOccupiedPositions.Contains(nextpos))
                {
                    
                    timestamplabels.Add(new TimestampWatermark { Label = $"Timestamp {index}", Position = nextpos });
                    string Format = "HH:MM:SS.FF";
                    if(cmbFormat.SelectedIndex == 0)
                    {
                        Format = @"hh\:mm\:ss\.ff";
                    }else if(cmbFormat.SelectedIndex == 1)
                    {
                        Format = @"hh\:mm\:ss\:ff";
                    }
                    Screen.CallWatermark(index, nextpos, Format);
                    AlreadyOccupiedPositions.Add(nextpos);
                    PresetPositions.Remove(nextpos);
                    index += 1;
                }

            }
        }

        private void lstViewAddedTimeCodeLabels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstViewAddedTimeCodeLabels.SelectedItem is TimestampWatermark timestampwatermark)
            {
                cmbFormat.SelectedIndex = timestampwatermark.FormatIndex;
            }
        } 
    }
}
