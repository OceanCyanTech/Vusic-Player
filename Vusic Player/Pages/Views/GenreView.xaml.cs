using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GenreView : Page
    {
        public GenreView()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is string genre)
            {
                txtGenreTitle.Text = genre;
            }
            base.OnNavigatedTo(e);
        }
        private void btnRenameGenre_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnShuffle_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddSongs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindGenreProfileOnline_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindGenreProfileLocal_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
