// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
#endregion

namespace Vusic_Player.Configuration.Playback
{
    /*  OCEANCYAN TECH HEADER
VUSIC PLAYER - VERSION v1.0.7.0
© OCEANCYAN TECH 2026

SERVICE - QUEUESERVICE
BUILD 131126
*/
    public class QueueService
    {

        #region Fields
        public static event Action? NextMediaInitiated;

        #endregion

        #region Playback
        public static void PlayMediaAtIndex(int index)
        {
            var media = QueueObjects.QueueInstance.QueueFull[index];
            if (File.Exists(media.FilePath))
            {
                PlayerService.OpenPath(media.FilePath);
                NextMediaInitiated?.Invoke();
                foreach (var item in QueueObjects.QueueInstance.QueueFull)
                {
                    item.IsCurrentQueueItem = false;
                }
                var currenttrack = QueueObjects.QueueInstance.QueueFull[index];
                currenttrack.IsCurrentQueueItem = true;
            }
        }
        public static void PlayPrevious()
        {
            if (QueueObjects.QueueInstance.QueueIndex != 0)
            {
                var prevmedia = QueueObjects.QueueInstance.QueueFull[QueueObjects.QueueInstance.QueueIndex - 1];
                if (prevmedia != null)
                {
                    prevmedia.IsCompleted = false;
                    QueueObjects.QueueInstance.QueueIndex--;
                    PlayMediaAtIndex(QueueObjects.QueueInstance.QueueIndex);
                }
            }
            else
            {
                if (PlayerService.Masterplayer != null)
                {
                    PlayerService.Masterplayer.SeekAccurate(0);
                }
            }
        }
        public static async void PlayNext()
        {
            if (QueueObjects.QueueInstance.QueueIndex < 0) return;
            if (QueueObjects.QueueInstance.QueueIndex > QueueObjects.QueueInstance.QueueFull.Count) return;
            var currentindexmedia = QueueObjects.QueueInstance.QueueFull[QueueObjects.QueueInstance.QueueIndex];
            if (currentindexmedia != null)
            {
                currentindexmedia.IsCompleted = true;
                if (QueueObjects.QueueInstance.QueueIndex == QueueObjects.QueueInstance.QueueFull.Count - 1)
                {
                    if (QueueObjects.QueueInstance.IsLoop)
                    {
                        QueueObjects.QueueInstance.QueueIndex = 0;
                        PlayMediaAtIndex(0);
                    }
                    return;
                }
                else
                {
                    QueueObjects.QueueInstance.QueueIndex++;
                    PlayMediaAtIndex(QueueObjects.QueueInstance.QueueIndex);
                }
            }
        }
        #endregion

        #region Queue Manager
        public static async Task ClearQueueFull()
        {
            QueueObjects.QueueInstance.QueueFull.Clear();
        }


        public static void ClearQueue()
        {
            QueueObjects.QueueInstance.QueueFull.Clear();
            QueueObjects.QueueInstance.QueueTemporary.Clear();
            QueueObjects.QueueInstance.QueueIndex = 0;
        }


        public static async void ReloadQueue()
        {
            NextMediaInitiated?.Invoke();
        }
        public static async void AddToQueue(string FilePath)
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                QueueObjects.QueueInstance.QueueFull.Add(await MetadataFetcher.GetSongModel(FilePath, true));
            }
            catch (Exception ex)
            {
                Logger.Log("Error: " + ex.Message, "QueueService.AddToQueue(SingleFilePath)", Logger.LogLevelType.Error);
            }
        }

        public static async void AddToQueue(List<string> FilePaths)
        {
            try
            {
                foreach (var path in FilePaths)
                {
                    if (!File.Exists(path)) continue;
                    QueueObjects.QueueInstance.QueueFull.Add(await MetadataFetcher.GetSongModel(path, true));
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error: " + ex.Message, "QueueService.AddToQueue(ListFilePaths)", Logger.LogLevelType.Error);
            }
        }

        public static void AddToQueue(ObservableCollection<SongModel> SongModels)
        {
            try
            {
                foreach (var song in SongModels)
                {
                    if (!File.Exists(song.FilePath)) continue;
                    QueueObjects.QueueInstance.QueueFull.Add(song);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error: " + ex.Message, "QueueService.AddToQueue(ListSongModels)", Logger.LogLevelType.Error);
            }
        }

        public static async void AddToQueueAndPlay(List<string> Paths, bool IsShuffleTrue, bool IsLoopTrue)
        {
            if (Paths.Count == 0) return;
            ClearQueue();

            foreach (var path in Paths)
            {
                if (!File.Exists(path)) continue;

                var songmodel = await MetadataFetcher.GetSongModel(path, true);
                Debug.WriteLine("SONG MODEL FETCHED FOR " + path + " ARTIST IS " + songmodel.Artist);
                QueueObjects.QueueInstance.QueueFull.Add(await MetadataFetcher.GetSongModel(path, true));
            }
            PlayerService.OpenPath(Paths[0]);
            foreach (var item in QueueObjects.QueueInstance.QueueFull)
            {
                item.IsCurrentQueueItem = false;
            }
            if (IsShuffleTrue)
            {
                ShuffleQueue();
                QueueObjects.QueueInstance.IsQueuePageShuffleEnabled = true;
            }
            else
            {
                QueueObjects.QueueInstance.IsQueuePageShuffleEnabled = false;
            }
            var currenttrack = QueueObjects.QueueInstance.QueueFull[0];
            currenttrack.IsCurrentQueueItem = true;
            QueueObjects.QueueInstance.IsLoop = IsLoopTrue;
        }
        public static async void AddToQueueAndPlay(string Path, bool IsShuffleTrue = false, bool IsLoopTrue = false)
        {
            if (!File.Exists(Path)) return;
            ClearQueue();
            QueueObjects.QueueInstance.QueueFull.Add(await MetadataFetcher.GetSongModel(Path, true));
            PlayerService.OpenPath(Path);
            var currenttrack = QueueObjects.QueueInstance.QueueFull[0];
            currenttrack.IsCurrentQueueItem = true;
            QueueObjects.QueueInstance.IsLoop = IsLoopTrue;
        }

        #endregion

        #region Additional
        public static void ToggleLoop(bool loop)
        {
            QueueObjects.QueueInstance.IsLoop = loop;
        }

        public static void ToggleShuffle(bool shuffle)
        {
            if (shuffle == true)
            {
                ShuffleQueue();
            }
            else
            {
                RestoreQueueItems();
            }
        }
        public static async void ShuffleQueue()
        {
            QueueObjects.QueueInstance.QueueTemporary.Clear();
            var queue = QueueObjects.QueueInstance.QueueFull;
            foreach (var item in queue.ToList())
            {
                QueueObjects.QueueInstance.QueueTemporary.Add(item);
            }
            int startIndex = QueueObjects.QueueInstance.QueueIndex + 1;

            if (queue is not null && startIndex < queue.Count - 1)
            {
                for (int i = queue.Count - 1; i > startIndex; i--)
                {
                    // Inclusive range [startIndex, i]
                    int randomIndex = Random.Shared.Next(startIndex, i + 1);
                    if (i != randomIndex)
                    {
                        queue.Move(i, randomIndex);
                    }
                }
            }
        }
        public static void RestoreQueueItems()
        {
            QueueObjects.QueueInstance.QueueFull.Clear();
            var originalitems = QueueObjects.QueueInstance.QueueTemporary.ToList();

            foreach (var item in originalitems)
            {
                QueueObjects.QueueInstance.QueueFull.Add(item);
            }
            var currentmediaplaying = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.IsCurrentQueueItem == true);
            if (currentmediaplaying != null)
            {
                QueueObjects.QueueInstance.QueueIndex = QueueObjects.QueueInstance.QueueFull.IndexOf(currentmediaplaying);
            }
        }

        #endregion

    }
}
