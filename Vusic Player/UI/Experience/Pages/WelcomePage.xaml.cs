// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using FlyleafLib;
using FlyleafLib.MediaPlayer;
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

    public sealed partial class WelcomePage : Page
    {
        Player localplayer = new Player();
        public WelcomePage()
        {
            InitializeComponent();
            this.Loaded += OceanCyanLogoIntro_Loaded;
            var keyConfig = localplayer.Config.Player.KeyBindings;

            keyConfig.Remove(Key.Left);
            keyConfig.Remove(Key.Space);
            keyConfig.Remove(Key.Right);
            keyConfig.Remove(Key.M);

            keyConfig.Remove(Key.Left, ctrl: true);
            keyConfig.Remove(Key.Right, ctrl: true);
            keyConfig.Remove(Key.Left, shift: true);
            keyConfig.Remove(Key.Right, shift: true); keyConfig.RemoveAll();
        }
        private async void OceanCyanLogoIntro_Loaded(object sender, RoutedEventArgs e)
        {
            localplayer.Config.Video.AspectRatio = AspectRatio.Fill;
            hostMedia.Player = localplayer;
            localplayer.PlaybackStopped += Localplayer_PlaybackStopped;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Vusic Player Intro.mov"));
            localplayer.Open(file.Path);
        }

        private void Localplayer_PlaybackStopped(object? sender, PlaybackStoppedArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                Frame.Navigate(typeof(WhatIsVusicPlayerPage), null, new DrillInNavigationTransitionInfo());
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            localplayer.Pause();
        }
    }
}
