using FlyleafLib;
using FlyleafLib.MediaFramework.MediaDevice;
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
    public class CountToDeviceTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? "Device (1)" : $"Devices ({count})";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public sealed partial class MultipleOutputMixer : UserControl
    {
        public MultipleOutputMixer()
        {
            InitializeComponent();
            if (PlayerService._multiAudioEngine != null)
            {
                PlayerService._multiAudioEngine.DeviceDisconnected -= _multiAudioEngine_DeviceDisconnected;
                PlayerService._multiAudioEngine.DeviceDisconnected += _multiAudioEngine_DeviceDisconnected;
            }
        }

        private void _multiAudioEngine_DeviceDisconnected(object? sender, string devID)
        {
            var existingDevice = ItemsSource.FirstOrDefault(p => p.DeviceID == devID);
            if (existingDevice != null)
            {
                ItemsSource.Remove(existingDevice);
            }
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

        public Visibility NoMediaPlaying
        {
            get => (Visibility)GetValue(NoMediaPlay);
            set => SetValue(NoMediaPlay, value);
        }
        public static readonly DependencyProperty NoMediaPlay =
    DependencyProperty.Register(
        nameof(NoMediaPlay),
        typeof(Visibility),
        typeof(MultipleOutputMixer),
        new PropertyMetadata(Visibility.Collapsed));
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
                lstViewMixers.Visibility = Visibility.Visible;
                lstViewDevices.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnOutputModeUI.Content = "Mixer Mode";

                lstViewMixers.Visibility = Visibility.Collapsed;
                lstViewDevices.Visibility = Visibility.Visible;
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

                device.Volume = (float)(Math.Truncate(volume * 10.0f) / 10.0f);
            }
        }

        private void mnftRenameDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is DeviceOutputShow device)
            {
                ttRenameDevice.IsOpen = true;
                txtRenameDevice.Text = device.DeviceUserName;
                btnRenameDevice.Click += (object sender, RoutedEventArgs e) =>
                {

                };
            }
        }

        private void btnSetVolume_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DeviceOutputShow device)
            {
                var volume = device.Volume;
                device.DeviceVolume = $"{volume * 100.0f}%";
                PlayerService.SetVolumeOfDevice(device.DeviceID, (float)volume);

            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ItemsSource.Clear();
            foreach (var device in Engine.Audio.Devices)
            {
                bool isDefault = (device.Name?.Contains("Default", StringComparison.OrdinalIgnoreCase) ?? false);
                if (!isDefault)
                {
                    var volume = PlayerService.GetVolumeOfDevice(device.Id);
                    ItemsSource.Add(new DeviceOutputShow { DeviceID = device.Id, DeviceName = device.Name ?? "Unknown Device", DeviceVolume = $"{volume * 100.0f}%", Volume = volume * 100.0f });
                }
            }
        }

        private void OceanSlider_ValueChanged(double obj)
        {

        }

        private void OceanSlider_ValueChangedWithSender(object sender, double value)
        {
            if (sender is OceanSlider oceanslider && oceanslider.DataContext is DeviceOutputShow device)
            {
                var volume = device.Volume;
                device.DeviceVolume = $"{volume * 100.0f}%";
                PlayerService.SetVolumeOfDevice(device.DeviceID, (float)volume);
            }
        }
    }
}
