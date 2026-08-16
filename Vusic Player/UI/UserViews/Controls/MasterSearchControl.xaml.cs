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
using Vusic_Player.Extensions;
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class MasterSearchControl : UserControl
    {
        List<AudioTrackLite> rawSongs = new List<AudioTrackLite>();
        public MasterSearchControl()
        {
            InitializeComponent();
            rawSongs = FilesInDatabase.rawSongs;
            lstViewSearchOptions.ItemsSource = searchResultsMaster;
            lstViewSearchOptions.AddHandler(
      UIElement.PreviewKeyDownEvent,
      new KeyEventHandler(lstViewSearchOptions_AlwaysPreviewKeyDown),
      true

 );
        }
        private void lstViewSearchOptions_AlwaysPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            int currentIndex = lstViewSearchOptions.SelectedIndex;
            int totalItems = lstViewSearchOptions.Items.Count;

            // 1. Handle Enter Key
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                if (currentIndex >= 0 && currentIndex < totalItems)
                {
                    if (lstViewSearchOptions.Items[currentIndex] is MasterSearchModel selected)
                    {
                        CommitSelection(selected);
                    }
                }
                return;
            }

            // 2. Handle Up Arrow at the very top
            if (e.Key == Windows.System.VirtualKey.Up && currentIndex == 0)
            {
                e.Handled = true; // Stop WinUI from changing selection
                lstViewSearchOptions.SelectedIndex = -1;
                asbSearchOptions.Focus(FocusState.Programmatic);
                return;
            }

            // 3. Handle Down Arrow at the very bottom
            if (e.Key == Windows.System.VirtualKey.Down && currentIndex == totalItems - 1)
            {
                e.Handled = true; // Stop WinUI from dead-ending
                lstViewSearchOptions.SelectedIndex = 0; // Explicitly loop to top
                return;
            }
            if ((e.Key >= Windows.System.VirtualKey.A && e.Key <= Windows.System.VirtualKey.Z) ||
        (e.Key >= Windows.System.VirtualKey.Number0 && e.Key <= Windows.System.VirtualKey.Number9) ||
        (e.Key >= Windows.System.VirtualKey.NumberPad0 && e.Key <= Windows.System.VirtualKey.NumberPad9) ||
        e.Key == Windows.System.VirtualKey.Space ||
        e.Key == Windows.System.VirtualKey.Back)
            {
                // Clear the list selection highlight
                lstViewSearchOptions.SelectedIndex = -1;

                // Shift focus back to the TextBox
                asbSearchOptions.Focus(FocusState.Programmatic);

                // Move the cursor to the very end of the text box string so typing appends cleanly
                asbSearchOptions.SelectionStart = asbSearchOptions.Text.Length;
                asbSearchOptions.SelectionLength = 0;

                // Do NOT set e.Handled = true here! 
                // Leaving it false allows WinUI to pass the letter directly into the newly-focused TextBox.
            }
        }
        private void CommitSelection(MasterSearchModel selected)
        {
            popupSearch.IsOpen = false;
            //        ListViewSelected(selected);
        }



        ObservableCollection<MasterSearchModel> searchResultsMaster = new ObservableCollection<MasterSearchModel>();

        private ObservableCollection<MasterSearchModel> GetFilteredResults(string query, int count = 5)
        {
            if (string.IsNullOrWhiteSpace(query)) return new ObservableCollection<MasterSearchModel>();

            var rawQuery = query.Trim();

            var returnableobservable = new ObservableCollection<MasterSearchModel>();

            var rawMedia = FilesInDatabase.rawSongs;
            var rawMediaTitleBoolCheck = rawMedia.All(p => p.Artist?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            Debug.WriteLine("raw media count: " + rawMedia.Count);
            Debug.WriteLine("raw query: " + rawQuery);
            var rawMediaArtist = rawMedia.Where(p => p.Title?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true).Take(count).ToList();
            foreach (var item in rawMediaArtist)
            {
                var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
                var imgicon = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "musicnoteicon" : "default";
                returnableobservable.Add(new MasterSearchModel { ImageThumbnail = $"ms-appx:///Assets/{imgicon}.png", ResultMain = item.Title, SubInformation = subinfo, FilePath = item.FilePath });
            }

            //if (rawMediaTitleBoolCheck)
            //{
            //    Debug.WriteLine("TITLE CHECK");
            //    var rawMediaTitle = rawMedia.Where(p => p.Title?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            //    foreach (var item in rawMediaTitle)
            //    {
            //        var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
            //        returnableobservable.Add(new MasterSearchModel { ImageThumbnail = "ms-appx:///Assets/artistdefault.png", ResultMain = item.Title, SubInformation = subinfo });
            //    }
            //}
            return returnableobservable;
        }

        private void asbSearchOptions_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = asbSearchOptions.Text.ToLower();

            searchResultsMaster.Clear();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var suggestions = GetFilteredResults(query);


                foreach (var item in suggestions)
                {
                    searchResultsMaster.Add(item);

                }
            }

            if (searchResultsMaster.Count == 0 || string.IsNullOrWhiteSpace(query))
            {
                popupSearch.IsOpen = false;
                lstViewSearchOptions.Visibility = Visibility.Collapsed;
            }
            else
            {
                popupSearch.IsOpen = true;
                lstViewSearchOptions.Visibility = Visibility.Visible;

                if (query.Length == 1)
                {
                    asbSearchOptions.Focus(FocusState.Programmatic);

                    // Keep the cursor flashing at the end of the single letter
                    asbSearchOptions.SelectionStart = asbSearchOptions.Text.Length;
                    asbSearchOptions.SelectionLength = 0;
                }
            }
        }


        private void btnSearchQuery_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lstViewSearchOptions_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void asbSearchOptions_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (lstViewSearchOptions.Items.Count == 0 || lstViewSearchOptions.Visibility != Visibility.Visible) return;

            if (e.Key == Windows.System.VirtualKey.Down)
            {
                e.Handled = true; // Prevent cursor from moving in TextBox

                popupSearch.IsOpen = true;
                lstViewSearchOptions.Focus(FocusState.Programmatic);

                // Only force index 0 if nothing is selected yet
                if (lstViewSearchOptions.SelectedIndex == -1)
                {
                    lstViewSearchOptions.SelectedIndex = 0;
                }
            }
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;

                if (lstViewSearchOptions.Items.Count > 0)
                {
                    // 1. Force the selection index so SelectedItem updates
                    lstViewSearchOptions.SelectedIndex = 0;

                    // 2. Grab the item and immediately execute it!
                    if (lstViewSearchOptions.Items[0] is MasterSearchModel selected)
                    {
                        CommitSelection(selected); // This closes the popup and navigates
                    }
                }
                else
                {
                    popupSearch.IsOpen = false;
                    //         spSearchResults.Visibility = Visibility.Visible;
                    //   tbViewHolder.Visibility = Visibility.Collapsed;
                }
            }
        }
        private DependencyObject? FindChildElementByName(DependencyObject parent, string name)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element && element.Name == name)
                {
                    return child;
                }

                var result = FindChildElementByName(child, name);
                
                if (result != null) return result;
            }
            return null;
        }
        private void asbSearchOptions_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Find the DeleteButton by its template name
                var deleteButton = FindChildElementByName(textBox, "DeleteButton");

                if (deleteButton is Button button)
                {
                    // Collapse its visibility so it never takes up space or appears
                    button.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void asbSearchOptions_GotFocus(object sender, RoutedEventArgs e)
        {
            btnSearchQuery.Margin = new Thickness(0, 0, 30, 0);
        }

        private void asbSearchOptions_LostFocus(object sender, RoutedEventArgs e)
        {
            btnSearchQuery.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
