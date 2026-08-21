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
using Vusic_Player.Configuration.Helper.UI.Navig;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.Pages.Views
{
    public sealed partial class SearchResults : Page
    {
        public SearchResults()
        {
            InitializeComponent();
        }
        MasterSearchIndex.Filters SearchFilterMain = MasterSearchIndex.Filters.All;
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SearchResultsPageState.alreadyNavigatedtoSearchResultsPage = false;
            base.OnNavigatedFrom(e);
        }
        public CollectionViewSource SearchCVS { get; set; } = new CollectionViewSource();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string text)
            {
                txtQuery.Text = "Query: " + text;
                var searchresults = MasterSearchIndex.GetSearchResults(text, SearchFilterMain, 0, true);
                var filteredgroup = searchresults.GroupBy(p => p.SubInformation).Select(g => new SearchResultGroupHeader(g.Key, g)).ToList();
                SearchCVS.IsSourceGrouped = true;
                SearchCVS.Source = filteredgroup;
            }
            base.OnNavigatedTo(e);
        }
    }
}
