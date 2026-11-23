// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;


namespace Vusic_Player.Pages.Views
{
    public sealed partial class LyricViewer : Page
    {
        public LyricViewer()
        {
            InitializeComponent();
        }
        private async void btnOpenMedia_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var media = await MediaPicker.PickSingle(App.MainWindowInstance, "Open Media");
            if (media != null)
            {
                bool isAudio = AudioExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                bool isVideo = VideoExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                if (isAudio)
                {

                    if (File.Exists(media.Path))
                    {
                        QueueService.AddToQueueAndPlay(media.Path);
                    }
                }
                else if (isVideo)
                {
                    if (PlayerService.InVideoPage == false)
                    {
                        if (File.Exists(media.Path))
                            Frame.Navigate(typeof(VideoPlayer), media.Path);
                    }
                    else
                    {
                        PlayerService.OpenPath(media.Path);
                    }

                }
                else
                {
                    //Handle other cases
                }
            }
        }
    }
}
