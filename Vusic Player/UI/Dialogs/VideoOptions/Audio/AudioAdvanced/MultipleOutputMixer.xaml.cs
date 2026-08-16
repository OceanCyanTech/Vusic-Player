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
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioAdvanced
{
    public sealed partial class MultipleOutputMixer : UserControl
    {
        public MultipleOutputMixer()
        {
            InitializeComponent();
        }
        public ObservableCollection<DeviceOutputShow> ItemsSource
        {
            get => (ObservableCollection<DeviceOutputShow>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<DeviceOutputShow>),
        typeof(MultipleOutputMixer),
        new PropertyMetadata(null));
        private void mnftMuteDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is DeviceOutputShow device)
            {
                if (mnft.Text == "Mute Device")
                {
                    mnft.Text = "Unmute Device";
                    PlayerService.MuteDev(device.DeviceID);
                    device.DeviceVolume = "0%";
                }
                else
                {
                    mnft.Text = "Mute Device";
                    PlayerService.UnmuteDev(device.DeviceID);
                    var volume = PlayerService.GetVolumeOfDevice(device.DeviceID);
                    device.DeviceVolume = $"{volume}%";
                }

            }
        }
        private void btnOutputModeUI_Click(object sender, RoutedEventArgs e)
        {
            if (btnOutputModeUI.Content.ToString() == "Mixer Mode")
            {
                btnOutputModeUI.Content = "List Mode";
            }
            else
            {
                btnOutputModeUI.Content = "Mixer Mode";
            }
        }
        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
        {
            if (sender is NumberBox numbox && numbox.DataContext is DeviceOutputShow device)
            {
                if (double.IsNaN(e.NewValue) || double.IsInfinity(e.NewValue))
                    return;

                // Scale 0-100 UI value to 0.0-1.0 float scale
                float volume = Math.Clamp((float)(e.NewValue / 100.0), 0.0f, 1.0f);

                device.Volume = volume;
            }
        }

        private void mnftRenameDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetVolume_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is DeviceOutputShow device)
            {
                var volume = device.Volume;
                device.DeviceVolume = $"{volume}%";
                PlayerService.SetVolumeOfDevice(device.DeviceID, volume);

            }
        }
    }
}
