using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Vusic_Player.Configuration.AppConfig;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoggerPage : Page
    {
        public LoggerPage()
        {
            InitializeComponent();
            Logger.Log("Log Entries requested by user", "LoggerPage.LoadPlayerLogs", Logger.LogLevelType.Information);

        }
        private void LoadPlayerLogs()
        {
            logEntries.Clear();
            Logger.GetLogDetailsList().ForEach(log =>
            {
                var entry = new LogEntry
                {
                    Timestamp = log.Timestamp,
                    Source = log.Source,
                    Message = log.Message,
                    Level = log.Level,
                    Icon = log.Level switch
                    {
                        Logger.LogLevelType.Information => "ms-appx:///Assets/infoicon.png",
                        Logger.LogLevelType.Warning => "ms-appx:///Assets/warning.png",
                        Logger.LogLevelType.Error => "ms-appx:///Assets/error.png",
                        Logger.LogLevelType.Success => "ms-appx:///Assets/success.png",
                        _ => null
                    }
                };
                logEntries.Insert(0, entry);
                logEntriesOriginal.Insert(0, entry);
            });
            lstViewPlayerLogs.ItemsSource = logEntries;
            if (logEntries.Count == 0)
            {
                txtNoLogs.Visibility = Visibility.Visible;
                lstViewPlayerLogs.Visibility = Visibility.Collapsed;
                //    stackHeader.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtNoLogs.Visibility = Visibility.Collapsed;
                //          stackHeader.Visibility = Visibility.Visible;
                lstViewPlayerLogs.Visibility = Visibility.Visible;
            }
        }

        ObservableCollection<LogEntry> logEntries = new ObservableCollection<LogEntry>();
        ObservableCollection<LogEntry> logEntriesOriginal = new ObservableCollection<LogEntry>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadPlayerLogs();
            Logger.LogAdded += Logger_LogAdded; ;
            // Updater.Logger.LogAdded += Logger_LogAdded1; ;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            base.OnNavigatedFrom(e);

            Logger.LogAdded -= Logger_LogAdded;


        }



        private void Logger_LogAdded(LogEntry obj)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                // LoadPlayerLogs();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Copy log entry
            if (sender is not Button button)
                return;

            if (button.DataContext is not LogEntry logEntry)
                return;

            var logText =
                $"[{logEntry.Timestamp:O}] " +
                $"[{logEntry.Level}] " +
                $"{logEntry.Source}: {logEntry.Message}";

            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(logText);

            Clipboard.SetContent(dataPackage);
            Clipboard.Flush(); // ensures persistence after app closes
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
             

                var picker = new FileSavePicker();
                var hwnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
                InitializeWithWindow.Initialize(picker, hwnd);

                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.SuggestedFileName = "VusicPlayer" + "_Log";

                picker.FileTypeChoices.Add("Log File", new List<string> { ".log" });
                picker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });

                StorageFile file = await picker.PickSaveFileAsync();
                if (file == null)
                    return;

                var sb = new StringBuilder();
                if (logEntries.Count == 0)
                    return;

                foreach (var log in logEntries)
                {
                    sb.AppendLine(
                        $"{log.Timestamp:O} | {log.Level} | {log.Source} | {log.Message}");
                }

                await FileIO.WriteTextAsync(file, sb.ToString());

                Logger.Log($"VusicPlayer log exported successfully.",
                    "LogPage",
                    Logger.LogLevelType.Information);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to export log: " + ex.Message,
                    "LogPage",
                    Logger.LogLevelType.Error);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            LoadPlayerLogs();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Level
            fntIconDirectionMessage.Glyph = "";
            fntIconDirectionSource.Glyph = "";
            fntIconDirectionTimeStamp.Glyph = "";
            var sorted = logEntries.OrderByDescending(p => p.Level).ToList();
            if (fntIconDirectionLevel.Glyph == "\uE70E")
            {
                fntIconDirectionLevel.Glyph = "\uE70D";
            }
            else
            {
                fntIconDirectionLevel.Glyph = "\uE70E";
                sorted = logEntries.OrderBy(p => p.Level).ToList();
            }
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = logEntries.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    logEntries.Move(oldIndex, newIndex);
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Timestamp
            fntIconDirectionMessage.Glyph = "";
            fntIconDirectionLevel.Glyph = "";
            fntIconDirectionSource.Glyph = "";
            var sorted = logEntries.OrderByDescending(p => p.Timestamp).ToList();
            if (fntIconDirectionTimeStamp.Glyph == "\uE70E")
            {
                Debug.WriteLine("IT IS CHEVRON UP");
                fntIconDirectionTimeStamp.Glyph = "\uE70D";
            }
            else
            {
                Debug.WriteLine("IT IS CHEVRON DOWN");

                fntIconDirectionTimeStamp.Glyph = "\uE70E";
                sorted = logEntries.OrderBy(p => p.Timestamp).ToList();
            }
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = logEntries.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    logEntries.Move(oldIndex, newIndex);
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            // Source
            fntIconDirectionMessage.Glyph = "";
            fntIconDirectionLevel.Glyph = "";
            fntIconDirectionTimeStamp.Glyph = "";
            var sorted = logEntries.OrderByDescending(p => p.Source).ToList();
            if (fntIconDirectionSource.Glyph == "\uE70E")
            {
                fntIconDirectionSource.Glyph = "\uE70D";
            }
            else
            {
                fntIconDirectionSource.Glyph = "\uE70E";
                sorted = logEntries.OrderBy(p => p.Source).ToList();
            }
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = logEntries.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    logEntries.Move(oldIndex, newIndex);
                }
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            // Message
            fntIconDirectionLevel.Glyph = "";
            fntIconDirectionSource.Glyph = "";
            fntIconDirectionTimeStamp.Glyph = "";
            var sorted = logEntries.OrderByDescending(p => p.Message).ToList();
            if (fntIconDirectionMessage.Glyph == "\uE70E")
            {
                fntIconDirectionMessage.Glyph = "\uE70D";
            }
            else
            {
                fntIconDirectionMessage.Glyph = "\uE70E";
                sorted = logEntries.OrderBy(p => p.Message).ToList();
            }
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = logEntries.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    logEntries.Move(oldIndex, newIndex);
                }
            }
        }

        private void chckShowErrors_Checked(object sender, RoutedEventArgs e)
        {
            FilterLogs();
        }

        private void FilterLogs()
        {
            if (!this.IsLoaded) return;
            bool showErrors = chckShowErrors.IsChecked == true;
            bool showInfos = chckShowInformation.IsChecked == true;
            bool showWarnings = chckShowWarning.IsChecked == true;
            bool showSuccess = chckShowSuccess.IsChecked == true;
            // 1. Filter and sort directly from the original list in one efficient LINQ query
            var targetItems = logEntriesOriginal
                .Where(p => (p.Level == Logger.LogLevelType.Error && showErrors) ||
             (p.Level == Logger.LogLevelType.Information && showInfos) ||
             (p.Level == Logger.LogLevelType.Warning && showWarnings) ||
             (p.Level == Logger.LogLevelType.Success && showSuccess))
                .OrderByDescending(p => p.Timestamp)
                .ToList();

            // 2. Clear and repopulate the ObservableCollection
            // This is vastly faster than doing IndexOf and Move operations
            logEntries.Clear();
            foreach (var item in targetItems)
            {
                logEntries.Add(item);
            }

            // 3. Reset UI icons
            fntIconDirectionMessage.Glyph = "";
            fntIconDirectionLevel.Glyph = "";
            fntIconDirectionSource.Glyph = "";
            fntIconDirectionTimeStamp.Glyph = "";
        }
        private void chckShowSuccess_Checked(object sender, RoutedEventArgs e)
        {
            FilterLogs();

        }
        private void chckShowInformation_Checked(object sender, RoutedEventArgs e)
        {
            FilterLogs();
        }

        private void chckShowWarning_Checked(object sender, RoutedEventArgs e)
        {
            FilterLogs();

        }
        private IEnumerable<LogEntry> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<LogEntry>();

            var rawQuery = query.Trim();


            return logEntriesOriginal.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(rawQuery) && (
                    (s.Message?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Source?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Level.ToString()?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true) 
                );

             

                return textMatch ;
            })
            .OrderByDescending(s => s.Message?.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Message);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                searchresults.Clear();
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                lstViewPlayerLogs.ItemsSource = logEntries;
                lstViewPlayerLogs.Visibility = Visibility.Visible;
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResults(sender.Text);

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };
                lstViewPlayerLogs.ItemsSource = searchresults;
            }
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var results = GetFilteredResults(sender.Text);

            if (results.Any())
            {
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                lstViewPlayerLogs.Visibility = Visibility.Visible;

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);
            }
            else if (logEntriesOriginal.Count > 0)
            {
                lstViewPlayerLogs.Visibility = Visibility.Collapsed;
                grdNoSearchResults.Visibility = Visibility.Visible;
                frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        ObservableCollection<LogEntry> searchresults = new();

        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
