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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class SearchControl : UserControl
    {
        public SearchControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty SearchPlaceholder =
          DependencyProperty.Register("Value", typeof(string), typeof(SearchControl), new PropertyMetadata("Search...."));

        public string SearchPlaceHolderText
        {
            get => (string)GetValue(SearchPlaceholder);
            set => SetValue(SearchPlaceholder, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register(
                 nameof(ItemsSource),
                 typeof(ObservableCollection<SearchModel>),
                 typeof(SearchControl),
                 new PropertyMetadata(null)); // 2. Default to null to avoid shared instances

        public ObservableCollection<SearchModel> ItemsSource
        {
            get => (ObservableCollection<SearchModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty NoSearchText =
       DependencyProperty.Register("Value", typeof(string), typeof(SearchControl), new PropertyMetadata("No Search Results found...."));

        public string NoSearchResultsDisplayString
        {
            get => (string)GetValue(NoSearchText);
            set => SetValue(NoSearchText, value);
        }
        private void btnCloseSubtitleSearch_Click(object sender, RoutedEventArgs e)
        {
            CloseSearch?.Invoke(this, EventArgs.Empty);
            //ClosePopupAnimation.Begin();
            //ClosePopupAnimation.Completed += (s, a) =>
            //{

            //};
        }

        private void asbSearch_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            int currentIndex = lstViewSearchResults.SelectedIndex;
            int maxIndex = lstViewSearchResults.Items.Count - 1;

            if (e.Key == Windows.System.VirtualKey.Down)
            {
                // Move selection down without leaving the search box
                if (currentIndex < maxIndex)
                {
                    lstViewSearchResults.SelectedIndex = currentIndex + 1;
                    lstViewSearchResults.ScrollIntoView(lstViewSearchResults.SelectedItem);
                }
                e.Handled = true; // Prevents the cursor from moving to the end of the text
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                asbSearch.Text = "";
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                // Move selection up
                if (currentIndex > 0)
                {
                    lstViewSearchResults.SelectedIndex = currentIndex - 1;
                    lstViewSearchResults.ScrollIntoView(lstViewSearchResults.SelectedItem);
                }
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // If they hit Enter, treat the selected item as "Clicked"
                if (lstViewSearchResults.SelectedItem is SearchModel selected)
                {
                    ItemSelected?.Invoke(this, selected);
                    e.Handled = true;
                }
            }
        }
        public event EventHandler<SearchModel>? ItemSelected;
        public event EventHandler? CloseSearch;

        private void OnInternalListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.AddedItems.FirstOrDefault() as SearchModel;

            if (selectedItem != null)
            {
                // Raise the event to notify the Page
                ItemSelected?.Invoke(this, selectedItem);
            }
        }
        private void asbSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (lstViewSearchResults.Items.Count > 0)
            {
                lstViewSearchResults.SelectedIndex = 0;
                var firstItem = lstViewSearchResults.Items[0] as SearchModel;

                if (firstItem != null)
                    ItemSelected?.Invoke(this, firstItem);
            }
        }
        ObservableCollection<SearchModel> SearchResults = new();

        private void asbSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {

                var searchTerm = sender.Text.ToLower().Trim();
                var items = ItemsSource;
                if (items == null) return;
                SearchResults.Clear();
                foreach (var item in items)
                {
                    SearchResults.Add(item);
                }

                var suggestions = SearchResults.Where(s =>
                    s.ResultString != null &&
                    s.ResultString.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                if (suggestions.Any())
                {
                    stkNoSearchResultsFound.Visibility = Visibility.Collapsed;
                    lstViewSearchResults.Visibility = Visibility.Visible;

                    lstViewSearchResults.ItemsSource = suggestions;
                }
                else
                {
                    stkNoSearchResultsFound.Visibility = Visibility.Visible;
                    lstViewSearchResults.Visibility = Visibility.Collapsed;
                    lstViewSearchResults.ItemsSource = null;
                }
            }

        }

        private void lstViewSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = lstViewSearchResults.SelectedItem as SearchModel;
            if (selected != null)
            {
                ItemSelected?.Invoke(this, selected);
            }
        }

        private void lstViewSearchResults_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selected = e.ClickedItem as SearchModel;
            if (selected != null)
            {
                ItemSelected?.Invoke(this, selected);
            }
        }
    }
}
