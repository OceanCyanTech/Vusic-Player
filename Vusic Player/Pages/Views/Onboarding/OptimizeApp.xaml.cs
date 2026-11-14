using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.UserSettings;
using Windows.ApplicationModel.Core;


namespace Vusic_Player.Pages.Views.Onboarding
{
    public sealed partial class OptimizeApp : Page
    {
        public OptimizeApp()
        {
            InitializeComponent();
        }
        private CancellationTokenSource? _loadingCts;
        bool isPermissionAllowed = true;
        private async Task AnimateStatusAsync(string baseText, CancellationToken token)
        {
            int dots = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    dots = (dots % 3) + 1;
                    txtMain.Text = baseText + new string('.', dots);

                    await Task.Delay(400, token);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when switching status messages
            }
        }
        private void UpdateStatus(string message)
        {
            // Cancel any ongoing dot-animation loop
            _loadingCts?.Cancel();
            _loadingCts = new CancellationTokenSource();

            // Start the new animation loop on the UI thread without blocking execution
            _ = AnimateStatusAsync(message, _loadingCts.Token);
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (isPermissionAllowed == true)
            {
                txtMain.Text = "Optimizing Vusic Player...";
                //UpdateStatus("Optimizing Vusic Player");
                await FilesInDatabase.InitializeDatabaseFunctionsAsync();

                await FilesInDatabase.RunBackgroundServiceAsync();
                await FilesInDatabase.LoadAllFiles();
                //        UpdateStatus("Indexing your media files");
                txtMain.Text = "Indexing your media files...";


                txtMain.Text = "Organizing your media library...";


                // Stop animation when done
                _loadingCts?.Cancel();
                txtMain.Text = "Vusic Player is all set to use!";
                prgMain.Visibility = Visibility.Collapsed;
                btnReady.Visibility = Visibility.Visible;
            }
            base.OnNavigatedTo(e);
        }
        private void RestartApp()
        {
            AppRestartFailureReason result = AppInstance.Restart(string.Empty);

            // If restart succeeds, the current process terminates automatically.
            // If it fails, inspect the failure reason:
            switch (result)
            {
                case AppRestartFailureReason.RestartPending:
                    // A restart request is already processing
                    break;
                case AppRestartFailureReason.InvalidUser:
                    // The user token is invalid
                    break;
                case AppRestartFailureReason.Other:
                    // General failure, handle fallback
                    break;
            }
        }

        private async void btnReady_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.isFirstTimeLaunch = false;
            currentSettings.LastSeenVersion = "1.0.7.0";
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            RestartApp();
        }
    }
}
