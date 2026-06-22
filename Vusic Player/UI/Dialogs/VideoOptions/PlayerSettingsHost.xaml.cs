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
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions
{
    public sealed partial class PlayerSettingsHost : UserControl
    {
        private FrameworkElement[] _panels;
        public PlayerSettingsHost()
        {
            InitializeComponent();
            _panels = new FrameworkElement[]
        {
            ctlVideoStream, ctlViewSettings, ctlSnapshotSettings, ctlPlaybackSpeed,
            ctlRecordSettings, ctlVideoFilters, ctlVideoRotation, ctlFlip,
            ctlCustomAspectRatio, ctlAudioPitch, ctlAudioGeneral, ctlAudioDevice,
            ctlAudioDelay, ctlEqualizer, ctlSubtitleGeneral, ctlSubtitleCustomize,
            ctlAudioVolume, ctlDelay
        };
            lstViewSearchOptions.ItemsSource = searchres;

            lstViewSearchOptions.AddHandler(
         UIElement.PreviewKeyDownEvent,
         new KeyEventHandler(lstViewSearchOptions_AlwaysPreviewKeyDown),
         true
     
    );
            //  this.PreviewKeyDown += PlayerSettingsHost_PreviewKeyDown; ;
            SearchVideoOptions.IndexResults(ctlViewSettings, ctlPlaybackSpeed, ctlVideoStream, ctlSnapshotSettings, ctlRecordSettings, ctlVideoFilters, ctlVideoRotation, ctlFlip, ctlCustomAspectRatio, ctlAudioPitch, ctlAudioGeneral, ctlAudioDevice, ctlAudioDelay, ctlEqualizer, ctlSubtitleGeneral, ctlSubtitleGeneral, ctlSubtitleCustomize, ctlDelay);
            ManualNavigationVideoSettings.NavigCalled += ManualNavigationVideoSettings_NavigCalled;
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
                    if (lstViewSearchOptions.Items[currentIndex] is SettingSearchResult selected)
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
        private void CommitSelection(SettingSearchResult selected)
        {
            popupSearch.IsOpen = false;
            ListViewSelected(selected);
        }
        private void PlayerSettingsHost_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (popupSearch.IsOpen == true)
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    Debug.WriteLine("Can you brave you most");
                    if (lstViewSearchOptions.SelectedIndex >= 0)
                    {
                        // Mark it as handled so nothing else processes it
                        e.Handled = true;

                        int index = lstViewSearchOptions.SelectedIndex;
                        if (lstViewSearchOptions.Items[index] is SettingSearchResult selected)
                        {
                            ListViewSelected(selected);
                        }
                    }
                }
            }
        }

        private void PlayerSettingsHost_KeyDown(object sender, KeyRoutedEventArgs e)
        {
         
        }

        public void ExecuteNavigation(int tabIdx, int subTabIdx, int panelIdx)
        {
            // 1. Set main tab
            if (tbViewOptions.SelectedIndex != tabIdx)
            {
                tbViewOptions.SelectedIndex = tabIdx;
            }

            // 2. Set sub-tab using a clean conditional block
            ApplySubTabSelection(tabIdx, subTabIdx);

            // 3. Scroll to panel safely after WinUI handles the layout pass
            if (panelIdx >= 0 && panelIdx < _panels.Length)
            {
                var targetPanel = _panels[panelIdx];

                // Using Normal priority ensures the UI has fully updated 
                // its tab visibility before calculating the scroll position.
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    try
                    {
                        var options = new BringIntoViewOptions
                        {
                            VerticalAlignmentRatio = 0.0, // Force to the top
                            AnimationDesired = true
                        };
                        targetPanel.StartBringIntoView(options);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, "PlayerSettingsNavigate", Logger.LogLevelType.Error);
                    }
                });
            }
        }

        private void ApplySubTabSelection(int tabIdx, int subTabIdx)
        {
            switch (tabIdx)
            {
                case 0:
                    sbiGeneral.IsSelected = (subTabIdx == 0);
                    sbiFilters.IsSelected = (subTabIdx == 1);
                    sbiOrientation.IsSelected = (subTabIdx == 2);
                    sbiAspectRatio.IsSelected = (subTabIdx == 3);
                    break;
                case 1:
                    GeneralTab.IsSelected = (subTabIdx == 0);
                    EqualizerTab.IsSelected = (subTabIdx != 0);
                    break;
                case 2:
                    selBarCustomizeSub.IsSelected = (subTabIdx == 1);
                    selBarTracksSub.IsSelected = (subTabIdx != 1);
                    break;
            }
        }
        private void ManualNavigationVideoSettings_NavigCalled()
        {
            tbViewOptions.SelectedIndex = ManualNavigationVideoSettings.TabIndex;
            Debug.WriteLine("IF YOU WONT END THINGS: " + ManualNavigationVideoSettings.TabIndex + " " + ManualNavigationVideoSettings.SubtabIndex + " " + ManualNavigationVideoSettings.PanelIndex);

            if (tbViewOptions.SelectedIndex == 0)
            {
                if (ManualNavigationVideoSettings.SubtabIndex == 0)
                {
                    sbiGeneral.IsSelected = true;
                }
                else if (ManualNavigationVideoSettings.SubtabIndex == 1)
                {
                    sbiFilters.IsSelected = true;
                }
                else if (ManualNavigationVideoSettings.SubtabIndex == 2)
                {
                    sbiOrientation.IsSelected = true;
                }
                else if (ManualNavigationVideoSettings.SubtabIndex == 3)
                {
                    sbiAspectRatio.IsSelected = true;
                }
            }
            else if (tbViewOptions.SelectedIndex == 1)
            {
                if (ManualNavigationVideoSettings.SubtabIndex == 0)
                {
                    GeneralTab.IsSelected = true;
                }
                else
                {
                    EqualizerTab.IsSelected = true;
                }

            }
            else if (tbViewOptions.SelectedIndex == 2)
            {
                if (ManualNavigationVideoSettings.SubtabIndex == 1)
                {
                    selBarCustomizeSub.IsSelected = true;
                }
                else
                {
                    selBarTracksSub.IsSelected = true;
                }
            }

            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                try
                {
                    // Define the order once (perhaps in a constructor or static field)
                    FrameworkElement[] panels = {
    ctlVideoStream,        // 0
    ctlViewSettings,       // 1
    ctlSnapshotSettings,   // 2
    ctlPlaybackSpeed,      // 3
    ctlRecordSettings,     // 4
    ctlVideoFilters,       // 5
    ctlVideoRotation,      // 6
    ctlFlip,               // 7
    ctlCustomAspectRatio,  // 8
    ctlAudioPitch,         // 9
    ctlAudioGeneral,       // 10
    ctlAudioDevice,        // 11
    ctlAudioDelay,         // 12
    ctlEqualizer,          // 13
    ctlSubtitleGeneral,    // 14
    ctlSubtitleCustomize,   // 15
    ctlAudioVolume, //16
    ctlDelay //17
};

                    // Simplified Assignment
                    if (ManualNavigationVideoSettings.PanelIndex >= 0 && ManualNavigationVideoSettings.PanelIndex < panels.Length)
                    {
                        framework = panels[ManualNavigationVideoSettings.PanelIndex];
                        Debug.WriteLine(framework.Name);
                    }
                    var options = new BringIntoViewOptions
                    {
                        VerticalAlignmentRatio = 0.0, // Force to the top
                        AnimationDesired = true
                    };

                    framework?.StartBringIntoView(options);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, "PlayerSettingsNavigate", Logger.LogLevelType.Error);
                }
            });
        }


        public static readonly DependencyProperty TabViewSelection =
    DependencyProperty.Register(
        nameof(TabViewSelection),
        typeof(int),
        typeof(PlayerSettingsHost),
        new PropertyMetadata(0, (d, e) => ((PlayerSettingsHost)d).OnIndexChanged()));
        public static readonly DependencyProperty SubTabViewSelection =
  DependencyProperty.Register(
      nameof(SubTabViewSelection),
      typeof(int),
      typeof(PlayerSettingsHost),
      new PropertyMetadata(0, (d, e) => ((PlayerSettingsHost)d).OnIndexChanged()));
        public static readonly DependencyProperty PanelId =
DependencyProperty.Register(
    nameof(PanelId),
    typeof(int),
    typeof(PlayerSettingsHost),
    new PropertyMetadata(0, (d, e) => ((PlayerSettingsHost)d).OnIndexChanged()));
        FrameworkElement? framework;

        private void OnIndexChanged()
        {
            Debug.WriteLine(TabViewSelectedIndex + " tabviewindex");
            tbViewOptions.SelectedIndex = TabViewSelectedIndex;
            if (tbViewOptions.SelectedIndex == 0)
            {
                if (SubTabViewSelectedIndex == 0)
                {
                    sbiGeneral.IsSelected = true;
                }
                else if (SubTabViewSelectedIndex == 1)
                {
                    sbiFilters.IsSelected = true;
                }
                else if (SubTabViewSelectedIndex == 2)
                {
                    sbiOrientation.IsSelected = true;
                }
                else if (SubTabViewSelectedIndex == 3)
                {
                    sbiAspectRatio.IsSelected = true;
                }
            }
            else if (tbViewOptions.SelectedIndex == 1)
            {
                if (SubTabViewSelectedIndex == 0)
                {
                    GeneralTab.IsSelected = true;
                }
                else
                {
                    EqualizerTab.IsSelected = true;
                }

            }
            else if (tbViewOptions.SelectedIndex == 2)
            {
                if (SubTabViewSelectedIndex == 1)
                {
                    selBarCustomizeSub.IsSelected = true;
                }
                else
                {
                    selBarTracksSub.IsSelected = true;
                }
            }

            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                // Define the order once (perhaps in a constructor or static field)
                FrameworkElement[] panels = {
    ctlVideoStream,        // 0
    ctlViewSettings,       // 1
    ctlSnapshotSettings,   // 2
    ctlPlaybackSpeed,      // 3
    ctlRecordSettings,     // 4
    ctlVideoFilters,       // 5
    ctlVideoRotation,      // 6
    ctlFlip,               // 7
    ctlCustomAspectRatio,  // 8
    ctlAudioPitch,         // 9
    ctlAudioGeneral,       // 10
    ctlAudioDevice,        // 11
    ctlAudioDelay,         // 12
    ctlEqualizer,          // 13
    ctlSubtitleGeneral,    // 14
    ctlSubtitleCustomize,   // 15
    ctlAudioVolume, //16
    ctlDelay //17
};

                // Simplified Assignment
                if (PanelID >= 0 && PanelID < panels.Length)
                {
                    framework = panels[PanelID];
                }
                var options = new BringIntoViewOptions
                {
                    VerticalAlignmentRatio = 0.0, // Force to the top
                    AnimationDesired = true
                };

                framework?.StartBringIntoView(options);
            });
        }

        // 2. The Wrapper (Keep this simple)
        public int TabViewSelectedIndex
        {
            get => (int)GetValue(TabViewSelection);
            set => SetValue(TabViewSelection, value);
        }
        public int SubTabViewSelectedIndex
        {
            get => (int)GetValue(SubTabViewSelection);
            set => SetValue(SubTabViewSelection, value);
        }
        public int PanelID
        {
            get => (int)GetValue(PanelId);
            set => SetValue(PanelId, value);
        }
        #region Search Box Events

        private void asbSearchOptions_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                tbViewHolder.Visibility = Visibility.Visible;
                spSearchResults.Visibility = Visibility.Collapsed;
                var query = sender.Text.ToLower();

                var searchres = new ObservableCollection<SettingSearchResult>();
                var suggestions = SearchVideoOptions.searchIndex.Where(s =>
                s.Keywords != null &&
               s.Keywords.Any(k => k.ToLower().Contains(query)));
                lstViewSearchOptions.ItemsSource = suggestions;

                if (lstViewSearchOptions.Items.Count == 0)
                {
                    popupSearch.IsOpen = false;
                    lstViewSearchOptions.Visibility = Visibility.Collapsed;
                }
                else
                {
                    popupSearch.IsOpen = true;

                    lstViewSearchOptions.Visibility = Visibility.Visible;
                }
                if (query == "")
                {
                    popupSearch.IsOpen = false;
                }
            }
        }



        private void btnSearchQuery_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSearchQuery();
        }

        // Consolidate your search trigger into one reusable method
        private void ExecuteSearchQuery()
        {
            if (lstViewSearchOptions.Items.Count > 0)
            {
                lstViewSearchOptions.SelectedIndex = 0;
                lstViewSearchOptions.Focus(FocusState.Programmatic);
            }
            else
            {
                popupSearch.IsOpen = false;
                spSearchResults.Visibility = Visibility.Visible;
                tbViewHolder.Visibility = Visibility.Collapsed;
            }
        }
        private void asbSearchOptions_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (lstViewSearchOptions.Items.Count > 0)
            {
                lstViewSearchOptions.SelectedIndex = 0;

            }
            else
            {
                popupSearch.IsOpen = false;
                spSearchResults.Visibility = Visibility.Visible;
                tbViewHolder.Visibility = Visibility.Collapsed;
            }
        }

        private void asbSearchOptions_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
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
                    if (lstViewSearchOptions.Items[0] is SettingSearchResult selected)
                    {
                        CommitSelection(selected); // This closes the popup and navigates
                    }
                }
                else
                {
                    popupSearch.IsOpen = false;
                    spSearchResults.Visibility = Visibility.Visible;
                    tbViewHolder.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        private void voSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            voGeneral.Visibility = Visibility.Collapsed;
            voFilters.Visibility = Visibility.Collapsed;
            voOrientation.Visibility = Visibility.Collapsed;

            voAspectRatio.Visibility = Visibility.Collapsed;
            switch (currentSelectedIndex)
            {
                case 0:
                    voGeneral.Visibility = Visibility.Visible;
                    break;

                case 1:
                    voFilters.Visibility = Visibility.Visible;
                    break;

                case 2:
                    voOrientation.Visibility = Visibility.Visible;
                    break;

                case 3:
                    voAspectRatio.Visibility = Visibility.Visible;
                    break;

            }
        }

        private void selBarAudioOptions_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            GeneralView.Visibility = Visibility.Collapsed;
            EqualizerView.Visibility = Visibility.Collapsed;
            switch (currentSelectedIndex)
            {
                case 0:
                    GeneralView.Visibility = Visibility.Visible;
                    break;
                case 1:
                    EqualizerView.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void voSelectorBarSubtitles_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            grdSubTracks.Visibility = Visibility.Collapsed;
            grdSubCustomize.Visibility = Visibility.Collapsed;
            //if (PlayerService.Masterplayer != null)
            //{
            //    cmbEmbeddedSubTracks.Items.Clear();
            //    foreach (var item in PlayerService.Masterplayer.Subtitles.Streams)
            //    {
            //        ComboBoxItem cmbitem = new ComboBoxItem();
            //        cmbitem.Content = $"{item.StreamIndex}. {item.Language}";
            //        cmbitem.Tag = item;
            //        cmbEmbeddedSubTracks.Items.Add(cmbitem);
            //        if (item.Enabled)
            //        {
            //            cmbEmbeddedSubTracks.SelectedItem = cmbitem;
            //        }
            //    }

            //}
            switch (currentSelectedIndex)
            {
                case 0:
                    grdSubTracks.Visibility = Visibility.Visible;
                    break;

                case 1:
                    grdSubCustomize.Visibility = Visibility.Visible;
                    break;



            }
        }
        private void ListViewSelected(SettingSearchResult selected)
        {
            if (selected != null)
            {
                tbViewOptions.SelectedIndex = selected.TabIndex;
                if (tbViewOptions.SelectedIndex == 0)
                {
                    if (selected.SegmentIndex == 0)
                    {
                        sbiGeneral.IsSelected = true;
                    }
                    else if (selected.SegmentIndex == 1)
                    {
                        sbiFilters.IsSelected = true;
                    }
                    else if (selected.SegmentIndex == 2)
                    {
                        sbiOrientation.IsSelected = true;
                    }
                    else if (selected.SegmentIndex == 3)
                    {
                        sbiAspectRatio.IsSelected = true;
                    }
                }

                else if (tbViewOptions.SelectedIndex == 1)
                {
                    if (selected.SegmentIndex == 0)
                    {
                        GeneralTab.IsSelected = true;
                    }
                    else
                    {
                        EqualizerTab.IsSelected = true;
                    }

                }
                else if (tbViewOptions.SelectedIndex == 2)
                {
                    if (selected.SegmentIndex == 1)
                    {
                        selBarCustomizeSub.IsSelected = true;
                    }
                    else
                    {
                        selBarTracksSub.IsSelected = true;
                    }
                }
                selected.TargetGrid?.UpdateLayout();

                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    var options = new BringIntoViewOptions
                    {
                        VerticalAlignmentRatio = 0.0, // Force to the top
                        AnimationDesired = true
                    };

                    selected.TargetGrid?.StartBringIntoView(options);
                });
            }

        }

        private void lstViewSearchOptions_ItemClick(object sender, ItemClickEventArgs e)
        {
            
            popupSearch.IsOpen = false;

            var selected = e.ClickedItem as SettingSearchResult;
            if (selected != null)
                ListViewSelected(selected);
        }

        private void lstViewSearchOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
            //popupSearch.IsOpen = false;

            //var selected = lstViewSearchOptions.SelectedItem as SettingSearchResult;
            //if (selected != null)
            //    ListViewSelected(selected);

        }
        /// <summary>
        /// NOTES: KEYBOARD SHORTCUT OR TYPING SHOULD FOCUS ON SEARCH
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstViewSearchOptions_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Up && lstViewSearchOptions.SelectedIndex == 0)
            {
                e.Handled = true;

                // Clear selection if you want a clean state when moving back up
                lstViewSearchOptions.SelectedIndex = -1;

                // Shift focus back to the textbox
                asbSearchOptions.Focus(FocusState.Programmatic);
            }
         
        }

        private void popupSearch_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {

        }
        // Place this at the top of your UserControl class with your other variables
        private ObservableCollection<SettingSearchResult> searchres = new ObservableCollection<SettingSearchResult>();
        private void asbSearchOptions_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            tbViewHolder.Visibility = Visibility.Visible;
            spSearchResults.Visibility = Visibility.Collapsed;

            var query = asbSearchOptions.Text.ToLower();

            searchres.Clear();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var suggestions = SearchVideoOptions.searchIndex.Where(s =>
                    s.Keywords != null &&
                    s.Keywords.Any(k => k.ToLower().Contains(query)));

                foreach (var item in suggestions)
                {
                    searchres.Add(item);
                }
            }

            if (searchres.Count == 0 || string.IsNullOrWhiteSpace(query))
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
    }
}
