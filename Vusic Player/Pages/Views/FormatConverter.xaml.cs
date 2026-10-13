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
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public class CountToMediaFilesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} media file added" : $"• {count} media files added";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class CountToMediaFilesProcessedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? $"• {count} media file processed" : $"• {count} media files processed";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class FormatConverter : Page
    {

        ObservableCollection<FileFormatConverterModel> AddedFiles = new ObservableCollection<FileFormatConverterModel>();
        ObservableCollection<FileFormatConverterModel> ProcessedFiles = new ObservableCollection<FileFormatConverterModel>();
        public FormatConverter()
        {
            InitializeComponent();
        }

        private async void btnAddAudioFiles_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var files = await FilePickers.MediaPicker.PickMultipleMediaFilesAsync(App.MainWindowInstance, "Add Audio Files");
            foreach (var file in files)
            {
                try
                {
                    if (Extensions.AudioExtensions.List.Contains(Path.GetExtension(file.Path)))
                    {
                        var newExtensions = new List<string>();
                        foreach(var ext in AudioExtensions.List)
                        {
                            if(ext != Path.GetExtension(file.Path))
                            {
                                newExtensions.Add(ext);
                            }
                        }

                        var audioprops = await file.Properties.GetMusicPropertiesAsync();
                        AddedFiles.Add(new FileFormatConverterModel { FilePath = file.Path, FromFormat = Path.GetExtension(file.Path).ToLower(), FileName = Path.GetFileNameWithoutExtension(file.Path), Duration = audioprops.Duration, AudioExtensionsList = new List<string>(newExtensions) });
                    }
                    else
                    {
                        var newExtensions = new List<string>();
                        foreach (var ext in AudioExtensions.List)
                        {
                            if (ext != Path.GetExtension(file.Path))
                            {
                                newExtensions.Add(ext);
                            }
                        }
                        foreach (var ext in VideoExtensions.List)
                        {
                            if (ext != Path.GetExtension(file.Path))
                            {
                                newExtensions.Add(ext);
                            }
                        }
                        var videoprops = await file.Properties.GetVideoPropertiesAsync();
                        AddedFiles.Add(new FileFormatConverterModel {Glyph= "\uE8B2",FromFormat=Path.GetExtension(file.Path).ToLower(), FilePath = file.Path, FileName = Path.GetFileNameWithoutExtension(file.Path), Duration = videoprops.Duration, AudioExtensionsList = new List<string>(newExtensions) });
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

    
        private async void btnConvert_Click(object sender, RoutedEventArgs e)
        {

        }
        private async Task<string> ConvertSingleFile(string inputpath, string toformat, string outputpath)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);
            string baseDir = AppContext.BaseDirectory;

            string ffmpegPath = Path.Combine(baseDir, "FFmpeg", "ffmpeg.exe");

            var tasks = AddedFiles.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    if (!File.Exists(item.FilePath))
                    {
                        return $"ERROR: Input file does not exist: {item.FilePath}";
                    }

                    string commandPipeline = $"\"{ffmpegPath}\" -i \"{item.FilePath}\" -c copy \"{item.Ou}\"";
                }
            }
            }
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConvertAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnCustomOutput_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is FileFormatConverterModel formatter)
            {
                if (App.MainWindowInstance == null) return;

                var folderpick = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose custom output location", Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary);

                if(folderpick != null)
                {
                    formatter.OutputPath = Path.Combine(folderpick.Path, formatter.FileName + "_" + Guid.NewGuid().ToString());
                }
            }
        }
    }
} 
