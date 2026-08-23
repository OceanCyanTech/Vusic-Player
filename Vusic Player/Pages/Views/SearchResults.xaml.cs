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
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Navig;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.Pages.Views
{
    public class CountToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 1 ? "1 search result" : $"{count} search results";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
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
        public ObservableCollection<MasterSearchModel> SearchResultsMain = new ObservableCollection<MasterSearchModel>();
        public void ModifySearchQuery(string text)
        {
            SearchResultsMain.Clear();
            txtQuery.Text = "Query: " + text;
            var searchresults = MasterSearchIndex.GetSearchResults(text, SearchFilterMain, 0, true);
            var filteredgroup = searchresults.GroupBy(p => p.SubInformation).Select(g => new SearchResultGroupHeader(g.Key, g)).ToList();
            AllSearchResults.ItemsSource = SearchResultsMain;
            foreach (var Searchresult in searchresults)
            {
                Debug.WriteLine(Searchresult.ResultMain);
                SearchResultsMain.Add(new MasterSearchModel { Album = Searchresult.Album, Artist = Searchresult.Artist, ImageThumbnail = Searchresult.ImageThumbnail, ResultMain = Searchresult.ResultMain, SubInformation = Searchresult.SubInformation, SearchFilter = Searchresult.SearchFilter, FilePath = Searchresult.FilePath });
            }
            AllSearchResults.ItemsSource = SearchResultsMain;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string text)
            {
                ModifySearchQuery(text);

            }
            base.OnNavigatedTo(e);
        }
    }
}
