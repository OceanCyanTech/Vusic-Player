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
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;


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
                await Task.Delay(4000);

                //        UpdateStatus("Indexing your media files");
                txtMain.Text = "Indexing your media files...";

                await Task.Delay(4000);
                txtMain.Text = "Organizing your media library...";

                //      UpdateStatus("Organizing your media library");
                await Task.Delay(4000);

                // Stop animation when done
                _loadingCts?.Cancel();
                txtMain.Text = "Ready!";
                prgMain.Visibility = Visibility.Collapsed;
                btnReady.Visibility = Visibility.Visible;
            }
            base.OnNavigatedTo(e);
        }
    }
}
