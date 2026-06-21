using FlyleafLib;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral
{
    public sealed partial class Device : UserControl
    {
        public MediaProperties.AudioProperties.Device ViewModel { get; } = new();
        public Device()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {

                Bindings.Update();
            };
        }

        private void srchAudioDevice_CloseSearch(object sender, EventArgs e)
        {
            srchAudioDevice.Visibility = Visibility.Collapsed;
        }

        private void srchAudioDevice_ItemSelected(object sender, SearchModel e)
        {
            if (e is SearchModel srchmod && srchmod.Carrier is AudioEngine.AudioEndpoint dev)
            {
                GeneralInfoService.ShowInfo($"Audio Device set to {dev.Name}");
                Engine.Audio.SetDevice(dev.Id);
            }
        }

        private void cmbAudioDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSearchAudioDevice_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;

            ObservableCollection<SearchModel> searchres = new();
            foreach (var sub in Engine.Audio.Devices)
            {
                searchres.Add(new SearchModel { ResultString = $"{sub.Name}", Carrier = sub });
            }
            srchAudioDevice.ItemsSource = searchres;
            srchAudioDevice.Visibility = Visibility.Visible;
        }
    }
}
