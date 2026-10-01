using FlyleafLib;
using FlyleafLib.MediaFramework.MediaDevice;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioAdvanced
{
    public class VolumeToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null) return 0.0;

                double val = System.Convert.ToDouble(value);
                if (double.IsNaN(val) || double.IsInfinity(val)) return 0.0;

                // 0.5f in model -> 50.0 in NumberBox
                return val * 100.0;
            }
            catch
            {
                return 0.0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null) return DependencyProperty.UnsetValue;

                double val = System.Convert.ToDouble(value);
                if (double.IsNaN(val) || double.IsInfinity(val)) return DependencyProperty.UnsetValue;

                // 200.0 in NumberBox -> 2.0f in model
                float result = (float)(val / 100.0);

                // Respect targetType if the binding specifically expects float vs double
                if (targetType == typeof(double)) return (double)result;

                return result;
            }
            catch
            {
                // DependencyProperty.UnsetValue tells XAML to ignore the bad update 
                // rather than wiping out the property with 0
                return DependencyProperty.UnsetValue;
            }
        }
    }
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
                Debug.WriteLine(volume + " IS THE VOLUME");
                device.DeviceVolume = $"{(volume).ToString("F2")}%";
                var volumetosend = volume / 100;
                PlayerService.SetVolumeOfDevice(device.DeviceID, (float)volumetosend);

            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null) return;
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


        private void OceanSlider_ValueChangedWithSender(object sender, double value)
        {
            if (sender is OceanSlider slider && slider.DataContext is DeviceOutputShow device)
            {
                // Change the clamp maximum from 1.0f to 2.0f to allow up to 200%
                float newVolume = Math.Clamp((float)(value / 100.0), 0.0f, 2.0f);

                device.Volume = newVolume;
                device.DeviceVolume = $"{(newVolume * 100.0f).ToString("F2")}%";

                PlayerService.SetVolumeOfDevice(device.DeviceID, newVolume);
            }
        }

        private void NumberBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (sender is NumberBox numBox && numBox.DataContext is DeviceOutputShow device)
                {
                    TextBox? innerBox = FindVisualChild<TextBox>(numBox);

                    // 2. Read the text from the inner box (fallback to numBox.Text if null)
                    string currentRawText = innerBox != null ? innerBox.Text : numBox.Text;
                    // Safe focus move for WinUI 3 Desktop
                    if (double.TryParse(currentRawText, out double numboxval))
                    {
                        device.DeviceVolume = $"{(numboxval).ToString("F2")}%";
                        var volumetosend = numboxval / 100;
                        PlayerService.SetVolumeOfDevice(device.DeviceID, (float)volumetosend);
                        e.Handled = true;
                    }
                }
            }
        }
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
        ObservableCollection<DeviceOutputShow> searchresults = new ObservableCollection<DeviceOutputShow>();
        private void asbSearchDevices_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {

                searchresults.Clear();
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                if (btnOutputModeUI.Content.ToString() == "Mixer Mode")
                {
                    lstViewDevices.ItemsSource = ItemsSource;
                    lstViewDevices.Visibility = Visibility.Visible;
                }
                else
                {
                    Debug.WriteLine("GridView");
                    lstViewMixers.ItemsSource = ItemsSource;
                    lstViewMixers.Visibility = Visibility.Visible;

                }
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResults(sender.Text);

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };
                if (btnOutputModeUI.Content.ToString() == "Mixer Mode")
                {
                    lstViewDevices.ItemsSource = searchresults;
                }
                else
                {
                    Debug.WriteLine("searching in gridview");
                    lstViewMixers.ItemsSource = searchresults;

                }
            }

        }
        private IEnumerable<DeviceOutputShow> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<DeviceOutputShow>();

            var rawQuery = query.Trim();


            var textQuery = rawQuery.Trim();

            return ItemsSource.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.DeviceName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.DeviceID?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                );



                return textMatch;
            })
            .OrderByDescending(s => s.DeviceName.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.DeviceName);
        }


        private void asbSearchDevices_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var results = GetFilteredResults(sender.Text);

            if (results.Any())
            {
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                if (btnOutputModeUI.Content.ToString() == "Mixer Mode")
                {
                    lstViewDevices.Visibility = Visibility.Visible;
                    Grid.SetRow(grdNoSearchResults, 2);
                }
                else
                {
                    Grid.SetRow(grdNoSearchResults, 3);

                    lstViewMixers.Visibility = Visibility.Visible;
                }

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);
            }
            else if (ItemsSource.Count > 0)
            {
                if (btnOutputModeUI.Content.ToString() == "Mixer Mode")
                {


                    lstViewDevices.Visibility = Visibility.Collapsed;
                }
                else
                {

                    lstViewDevices.Visibility = Visibility.Collapsed;
                }
                grdNoSearchResults.Visibility = Visibility.Visible;
                frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbSearchDevices.Text = "";
            asbSearchDevices.ItemsSource = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
