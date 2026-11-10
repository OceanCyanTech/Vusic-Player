// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 091126) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;
using System.Windows.Input;
using Windows.Storage;

namespace Vusic_Player.UI.Experience.Pages
{
    public sealed partial class WelcomePage : Page
    {
        private readonly Player _localplayer = new Player();
        private bool _navigated = false;
        private DispatcherTimer? _safetyTimer;

        public WelcomePage()
        {
            InitializeComponent();
            this.Loaded += WelcomePage_Loaded;
            this.Unloaded += WelcomePage_Unloaded;

            try
            {
                var keyConfig = _localplayer.Config.Player.KeyBindings;
                keyConfig.RemoveAll();
            }
            catch { }
        }

        private async void WelcomePage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _localplayer.Config.Video.AspectRatio = AspectRatio.Fill;
                hostMedia.Player = _localplayer;
                _localplayer.PlaybackStopped += Localplayer_PlaybackStopped;

                // Safety timeout: auto-advance if playback freezes or hangs in VM environments
                _safetyTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(18) };
                _safetyTimer.Tick += (s, args) =>
                {
                    _safetyTimer?.Stop();
                    NavigateNext();
                };
                _safetyTimer.Start();

                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Vusic Player Intro.mp4"));

                if (file != null && File.Exists(file.Path))
                {
                    _localplayer.Open(file.Path);
                }
                else
                {
                    NavigateNext();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WelcomePage] Video load failure: {ex.Message}");
                NavigateNext();
            }
        }

        private void Localplayer_PlaybackStopped(object? sender, PlaybackStoppedArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                NavigateNext();
            });
        }

        private void NavigateNext()
        {
            if (_navigated) return;
            _navigated = true;

            try
            {
                _safetyTimer?.Stop();
                _localplayer.PlaybackStopped -= Localplayer_PlaybackStopped;
                _localplayer.Stop();
                hostMedia.Player = null; // Unbind DirectX host surface before frame change
            }
            catch { }

            DispatcherQueue.TryEnqueue(() =>
            {
                Frame?.Navigate(typeof(WhatIsVusicPlayerPage), null, new DrillInNavigationTransitionInfo());
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _localplayer.Pause();
            }
            catch { }
        }

        private void WelcomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _safetyTimer?.Stop();
                _localplayer.PlaybackStopped -= Localplayer_PlaybackStopped;
                _localplayer.Stop();
                hostMedia.Player = null;
            }
            catch { }
        }
    }
}