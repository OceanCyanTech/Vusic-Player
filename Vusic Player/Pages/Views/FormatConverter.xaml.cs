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
using Vusic_Player.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FormatConverter : Page
    {
        ObservableCollection<FileFormatConverterModel> AddedFiles = new ObservableCollection<FileFormatConverterModel>();
        ObservableCollection<FileFormatConverterModel> ProcessedFiles = new ObservableCollection<FileFormatConverterModel>();
        public FormatConverter()
        {
            InitializeComponent();
        }

        private void btnAddAudioFiles_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmbFromFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbToFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConvertAll_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
