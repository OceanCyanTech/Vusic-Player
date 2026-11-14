// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using FlyleafLib.MediaPlayer;
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
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Experience.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OceanCyanLogoIntro : Page
    {
        Player localplayer = new Player();
        private bool _isCompleted = false;

        public event Action? Completed;

        public OceanCyanLogoIntro()
        {
            InitializeComponent();
            this.Loaded += OceanCyanLogoIntro_Loaded;
            this.Unloaded += OceanCyanLogoIntro_Unloaded;

            // Strip keybindings safely
            try
            {
                localplayer.Config.Player.KeyBindings.RemoveAll();
            }
            catch { }
        }

        private async void OceanCyanLogoIntro_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                hostMedia.Player = localplayer;
                localplayer.PlaybackStopped += Localplayer_PlaybackStopped;

                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Ocean Cyan Intro.mov"));
                if (file != null && System.IO.File.Exists(file.Path))
                {
                    localplayer.Open(file.Path);
                }
                else
                {
                    FinishIntro();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Intro video load error: {ex.Message}");
                FinishIntro();
            }
        }

        private void Localplayer_PlaybackStopped(object? sender, EventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                FinishIntro();
            });
        }

        private void FinishIntro()
        {
            if (_isCompleted) return;
            _isCompleted = true;

            try
            {
                localplayer.PlaybackStopped -= Localplayer_PlaybackStopped;
                localplayer.Stop();
                hostMedia.Player = null; // Unbind DirectX host surface cleanly
                DispatcherQueue.TryEnqueue(() =>
                {
                    Frame.Navigate(typeof(WelcomePage));
                });
            }
            catch { }

            // Trigger next wizard page on UI thread
            Completed?.Invoke();
        }

        private void OceanCyanLogoIntro_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                localplayer.PlaybackStopped -= Localplayer_PlaybackStopped;
                hostMedia.Player = null;
            }
            catch { }
        }
    }
}
