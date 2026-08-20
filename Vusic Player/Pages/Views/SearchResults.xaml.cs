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
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    public sealed partial class SearchResults : Page
    {
        public SearchResults()
        {
            InitializeComponent();
        }
        Filters SearchFilterMain = Filters.All;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is string text)
            {
                txtQuery.Text = "Query: " + text;
                var searchresults = MasterSearchIndex.GetSearchResults(text, SearchFilterMain, 0, true);
            }
            base.OnNavigatedTo(e);
        }
    }
}
