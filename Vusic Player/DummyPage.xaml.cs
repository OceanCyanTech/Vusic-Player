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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DummyPage : Page
    {
        public ObservableCollection<SongModel> FoundSongs { get; set; } = new ObservableCollection<SongModel>();

        public DummyPage()
        {
            InitializeComponent();
            LoadFiles();
        }
        private async void LoadFiles()
        {
            try
            {
                // 1. Heavy DB work on background thread (returns plain C# objects)
                List<AudioTrackLite> rawSongs = await Task.Run(() => DatabaseService.GetAllSongs());

                // 2. Map and bind back on the UI Thread
                // (Task.Run returns execution to the UI thread automatically after 'await')

                var songModels = rawSongs.Select(s => new SongModel
                {
                    Title = s.Title,
                    Artist = s.Artist,
                    AlbumName = s.AlbumName,
                    FilePath = s.FilePath
                }).ToList();

                // 3. Assign directly to UI
                FoundSongs = new ObservableCollection<SongModel>(songModels);
                lstViewMedia.ItemsSource = FoundSongs;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load songs: {ex.Message}");
            }

        }
    }
}
