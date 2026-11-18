using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Pages;

namespace Vusic_Player.Configuration.Playback
{
    public class QueueService
    {
        private static Random rng = new Random();
        public static int videoindex = -1;
        public static ObservableCollection<SongModel> VusicQueue { get; } = new();
        public static ObservableCollection<SongModel> VusicQueueFull { get; } = new();
        public static ObservableCollection<SongModel> OriginalVusicQueue { get; } = new();
        public static ObservableCollection<SongModel> VusicQueueNext { get; } = new();
        public static ObservableCollection<SongModel> OriginalVusicQueueNext { get; } = new();
        public static AdvancedCollectionView VusicQueueView { get; set; } = new();


        public static bool IsLoopTrue = false;
        public static bool IsShuffleTrue = false;
        private static bool IsVusicQueueNextChanging = false;
        public static bool IsLooping = false;
        public static async Task ShuffleQueue()
        {
            var originalitems = QueueObjects.QueueInstance.QueueFull.ToList();
            foreach (var item in originalitems)
            {
                QueueObjects.QueueInstance.QueueTemporary.Add(item);
            }
            var incompleteitems = QueueObjects.QueueInstance.QueueFull.Where(p => p.IsCompleted == false && p.FilePath != PlayerService.CurrentPlayingPath).ToList();

            var completeitems = QueueObjects.QueueInstance.QueueFull.Where(p => p.IsCompleted == true);
            var currentmedia = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            var tempsongmodel = new SongModel();
            if (currentmedia != null)
            {
                tempsongmodel = currentmedia;   
            }
            var shuffled = ShuffleItems(incompleteitems);
            QueueObjects.QueueInstance.QueueFull.Clear();
            foreach (var item in completeitems)
            {
                var exist = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (exist == null)
                {
                    QueueObjects.QueueInstance.QueueFull.Add(item);
                }
            }
            var exist2 = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (exist2 == null)
            {
                QueueObjects.QueueInstance.QueueFull.Add(tempsongmodel);
            }
            foreach (var item in shuffled)
            {
                var exist = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (exist == null)
                {
                    QueueObjects.QueueInstance.QueueFull.Add(item);
                }
            }
        }
        public static async Task RestoreQueueItems()
        {
            var originalitems = QueueObjects.QueueInstance.QueueTemporary.ToList();
            QueueObjects.QueueInstance.QueueFull.Clear();
            foreach (var item in originalitems)
            {
                QueueObjects.QueueInstance.QueueFull.Add(item);
            }
        }

        public static void VusicQueueNext_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("DRAGGED AND DROPPED");
            if (IsLooping == true) return;
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Move)
                {
                    IsVusicQueueNextChanging = true;
                    for (int i = VusicQueue.Count - 1; i >= 0; i--)
                    {
                        var item = VusicQueue[i];
                        if (item.IsCompleted == false && item.FilePath != PlayerService.CurrentPlayingPath)
                        {
                            VusicQueue.RemoveAt(i);
                        }
                    }
                    for (int i = 0; i < VusicQueueNext.Count; i++)
                    {
                        var exist = VusicQueue.FirstOrDefault(p => p.FilePath == VusicQueueNext[i].FilePath);
                        if (exist == null)
                        {
                            VusicQueue.Add(VusicQueueNext[i]);
                        }
                    }
                }
            }
            finally
            {
                IsVusicQueueNextChanging = false;
            }
        }
        public static void AddToQueue(string FilePath)
        {

        }

        public static void AddToQueue(List<string> FilePaths)
        {

        }
        public static void AddToQueue(ObservableCollection<SongModel> SongModels)
        {

        }
        public static void VusicQueue_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 1. Guard clauses to completely ignore internal updates or resets
            //     if (IsVusicQueueNextChanging) return;
            return;
            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Move) return;

            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                try
                {
                    //        IsVusicQueueNextChanging = true;
                    var currentmedia = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
                    Debug.WriteLine("YES CHANGING for " + currentmedia.FilePath);

                    if (currentmedia != null)
                    {
                        var index = QueueObjects.QueueInstance.QueueFull.IndexOf(currentmedia);
                        Debug.WriteLine("INDEX " + index);
                        //Every media prior
                        for (int i = 0; i < index; i++)
                        {
                            var track = QueueObjects.QueueInstance.QueueFull[i];
                            if (track != null)
                            {
                                track.IsCompleted = true;
                            }
                        }
                        //Every media after
                        for (int i = index + 1; i < QueueObjects.QueueInstance.QueueFull.Count; i++)
                        {
                            var track = QueueObjects.QueueInstance.QueueFull[i];
                            if (track != null)
                            {
                                track.IsCompleted = false;
                            }
                        }
                    }
                    return;
                    // Step A: Categorize everything while maintaining their current order
                    var completedTracks = VusicQueue.Where(s => s.VisibilityOfStrikeThrough == Visibility.Visible).ToList();

                    var currentlyPlaying = VusicQueue.Where(s => s.FilePath == PlayerService.CurrentPlayingPath
                                                              && s.VisibilityOfStrikeThrough != Visibility.Visible).ToList();

                    var upcomingTracks = VusicQueue.Where(s => s.FilePath != PlayerService.CurrentPlayingPath
                                                            && s.VisibilityOfStrikeThrough != Visibility.Visible).ToList();

                    // Step B: Combine them into the definitive, golden-standard layout sequence
                    var sortedTargetList = completedTracks
                        .Concat(currentlyPlaying)
                        .Concat(upcomingTracks)
                        .ToList();

                    // Step C: Surgically re-order the live VusicQueue to match our target list
                    for (int i = 0; i < sortedTargetList.Count; i++)
                    {
                        var targetItem = sortedTargetList[i];
                        int currentLiveIndex = VusicQueue.IndexOf(targetItem);

                        if (currentLiveIndex >= 0 && currentLiveIndex != i)
                        {
                            // Move the item to its true locked group position smoothly!
                            VusicQueue.Move(currentLiveIndex, i);
                        }
                    }
                }
                finally
                {
                    IsVusicQueueNextChanging = false;
                }
            });
        }
        public static void ClearQueue()
        {
            QueueObjects.QueueInstance.QueueFull.Clear();
            QueueObjects.QueueInstance.QueueIndex = 0;
        }
        public static async void AddToQueueAndPlay(List<string> Paths, bool IsShuffleTrue, bool IsLoopTrue)
        {
            //OCEANCYAN REVISION
            if (Paths.Count == 0) return;
            ClearQueue();

            foreach (var path in Paths)
            {
                var songmodel = await MetadataFetcher.GetSongModel(path);
                Debug.WriteLine("SONG MODEL FETCHED FOR " + path + " ARTIST IS " + songmodel.Artist);
                QueueObjects.QueueInstance.QueueFull.Add(await MetadataFetcher.GetSongModel(path));
            }
            PlayerService.OpenPath(Paths[0]);
            QueueObjects.QueueInstance.IsLoop = IsLoopTrue;
        }
        public static void PlayMediaAtIndex(int index)
        {
            var media = QueueObjects.QueueInstance.QueueFull[index];
            if (File.Exists(media.FilePath))
            {
                PlayerService.OpenPath(media.FilePath);
                NextMediaInitiated?.Invoke();
            }
        }
        public static void PlayPrevious()
        {
            //OCEANCYAN REVISION PENDING
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
            return;
            var queue = VusicQueue;
            var currentPath = PlayerService.CurrentPlayingPath;

            if (queue == null || queue.Count == 0 || string.IsNullOrEmpty(currentPath)) return;

            // 1. Find where we currently are in the master queue
            int index = queue.ToList().FindIndex(p => p.FilePath == currentPath);

            // If we're at the very first song (index 0), there is no previous track!
            if (index <= 0) return;

            try
            {
                // 2. SILENCE HANDLERS: Lock down notifications while we manipulate the history state
                IsLooping = true;

                var currentItem = queue[index];
                var prevItem = queue[index - 1];

                if (prevItem == null || string.IsNullOrEmpty(prevItem.FilePath)) return;

                // 3. Reset states: Bring BOTH songs back to life from the history "strikethrough" zone
                currentItem.IsCompleted = false;

                prevItem.IsCompleted = false;

                // 4. Update Upcoming Views: Put the current item back at the top of the future queue
                var existInNext = VusicQueueNext.FirstOrDefault(p => p.FilePath == currentItem.FilePath);
                if (existInNext == null)
                {
                    VusicQueueNext.Insert(0, currentItem);
                }

                // 5. CRITICAL FIX: Keep the shuffle backup pristine if shuffle is currently active
                if (IsShuffleTrue)
                {
                    var existInBackup = OriginalVusicQueueNext.FirstOrDefault(p => p.FilePath == currentItem.FilePath);
                    if (existInBackup == null)
                    {
                        OriginalVusicQueueNext.Insert(0, currentItem);
                    }
                }

                // 6. Set the player path and fire off the audio stream initialization
                PlayerService.CurrentPlayingPath = prevItem.FilePath;
                PlayerService.OpenPath(prevItem.FilePath);
            }
            finally
            {
                IsLooping = false;
            }

            // 7. Force the master queue layouts to smoothly update and mirror these changes instantly
            SyncFullQueueFromNext();
        }
        public static async void ReloadQueue()
        {
            NextMediaInitiated?.Invoke();
        }
        public static event Action? NextMediaInitiated;
        public static async void PlayNext()
        {
            //OCEANCYAN REVISION PENDING

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
            return;
            MarkSongCompleted();
            if (IsLoopTrue)
            {

                if (VusicQueueNext.Count == 0)
                {
                    try
                    {
                        IsLooping = true;

                        foreach (var item in VusicQueue.ToList())
                        {
                            item.IsCompleted = false;
                            VusicQueueNext.Add(item);
                        }

                        // 1. Force the engine to start at the absolute beginning of the reset queue
                        if (VusicQueue.Count > 0)
                        {
                            var firstItemOfNewLoop = VusicQueue[0];
                            PlayerService.CurrentPlayingPath = firstItemOfNewLoop.FilePath;
                            if (File.Exists(firstItemOfNewLoop.FilePath))
                            {
                                string fileExtension = Path.GetExtension(firstItemOfNewLoop.FilePath).ToLowerInvariant();
                                bool isVideo = false;
                                if (Extensions.VideoExtensions.List.Contains(fileExtension))
                                {
                                    isVideo = true;
                                }
                                if (isVideo)
                                {
                                    Debug.WriteLine("True");
                                    if (App.NavigationFrame != null)
                                    {
                                        if (PlayerService.InVideoPage == false)
                                        {
                                            App.NavigationFrame.Navigate(typeof(VideoPlayer), firstItemOfNewLoop.FilePath);
                                        }
                                        else
                                        {
                                            PlayerService.OpenPath(firstItemOfNewLoop.FilePath);
                                        }

                                    }
                                }
                                else
                                {
                                    if (App.NavigationFrame != null)
                                    {
                                        if (PlayerService.InVideoPage == true)
                                        {
                                            App.NavigationFrame.GoBack();
                                            PlayerService.InVideoPage = false;
                                            PlayerService.OpenPath(firstItemOfNewLoop.FilePath);

                                        }

                                    }
                                }
                            }
                        }

                        // 2. CRITICAL: Sync and exit early! Do not let the code run down to the regular index math.
                        SyncQueueLists();
                        return;
                    }
                    finally
                    {
                        IsLooping = false;
                    }
                }
            }
            var currentpath = VusicQueue.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (currentpath != null && currentpath.FilePath != null)
            {
                var index = VusicQueue.IndexOf(currentpath);
                var nextindex = index + 1;
                if (nextindex < VusicQueue.Count)
                {
                    var item = VusicQueue[nextindex];
                    if (File.Exists(item.FilePath))
                    {
                        string fileExtension = Path.GetExtension(item.FilePath).ToLowerInvariant();
                        bool isVideo = false;
                        if (Extensions.VideoExtensions.List.Contains(fileExtension))
                        {
                            isVideo = true;
                        }
                        if (isVideo)
                        {
                            Debug.WriteLine("True");
                            if (App.NavigationFrame != null)
                            {
                                if (PlayerService.InVideoPage == false)
                                {
                                    App.NavigationFrame.Navigate(typeof(VideoPlayer), item.FilePath);
                                }
                                else
                                {
                                    PlayerService.OpenPath(item.FilePath);
                                }

                            }
                        }
                        else
                        {
                            if (App.NavigationFrame != null)
                            {
                                if (PlayerService.InVideoPage == true)
                                {
                                    App.NavigationFrame.GoBack();
                                    PlayerService.InVideoPage = false;
                                    PlayerService.OpenPath(item.FilePath);

                                }

                            }
                        }
                    }
                }
            }
            else
            {
                if (VusicQueue.Count != 0)
                {
                    var firstitem = VusicQueue[0];
                    if (firstitem != null)
                    {
                        if (File.Exists(firstitem.FilePath))
                        {
                            string fileExtension = Path.GetExtension(firstitem.FilePath).ToLowerInvariant();
                            bool isVideo = false;
                            if (Extensions.VideoExtensions.List.Contains(fileExtension))
                            {
                                isVideo = true;
                            }
                            if (isVideo)
                            {
                                Debug.WriteLine("True");
                                if (App.NavigationFrame != null)
                                {
                                    if (PlayerService.InVideoPage == false)
                                    {
                                        App.NavigationFrame.Navigate(typeof(VideoPlayer), firstitem.FilePath);
                                    }
                                    else
                                    {
                                        PlayerService.OpenPath(firstitem.FilePath);
                                    }

                                }
                            }
                            else
                            {
                                if (App.NavigationFrame != null)
                                {
                                    if (PlayerService.InVideoPage == true)
                                    {
                                        App.NavigationFrame.GoBack();
                                        PlayerService.InVideoPage = false;
                                        PlayerService.OpenPath(firstitem.FilePath);

                                    }

                                }
                            }
                        }
                    }
                }
            }
            SyncQueueLists();
        }

        public static void PlayMedia(ObservableCollection<SongModel> media, bool IsShuffleEnabled, bool IsLoopEnabled)
        {
            if (media == null || media.Count == 0) return;

            try
            {
                IsVusicQueueNextChanging = true;
                IsShuffleTrue = IsShuffleEnabled;
                PlayerService.CurrentPlayingPath = "";
                VusicQueue.Clear();
                VusicQueueNext.Clear();
                OriginalVusicQueueNext.Clear();
                foreach (var item in media)
                {
                    item.IsCompleted = false;
                    item.QueueControls = Visibility.Collapsed;
                    VusicQueue.Add(item);
                }

                if (IsShuffleTrue)
                {
                    ShuffleList();
                }

                var firstTrack = VusicQueue[0];
                PlayerService.CurrentPlayingPath = firstTrack.FilePath;
                for (int i = 1; i < VusicQueue.Count; i++)
                {
                    VusicQueueNext.Add(VusicQueue[i]);
                    OriginalVusicQueueNext.Add(VusicQueue[i]);
                }
                PlayerService.OpenPath(firstTrack.FilePath);
            }
            finally
            {
                IsVusicQueueNextChanging = false;
            }

            SyncFullQueueFromNext();
        }
        public static void ShuffleAll()
        {
            OriginalVusicQueue.Clear();

            var itemsToAdd = VusicQueue.ToList();

            foreach (var item in itemsToAdd)
            {
                OriginalVusicQueue.Add(item);
            }
            foreach (var item in VusicQueue)
            {
                item.IsCompleted = false;
            }
            var shuffledlist = ShuffleItems(VusicQueue.ToList());
            foreach (var item in shuffledlist)
            {
                VusicQueue.Add(item);
            }

        }
        public static void ShuffleNext()
        {
            if (VusicQueueNext.Count <= 1) return;

            try
            {
                IsLooping = true;

                // 2. Safely capture the original sequence baseline before shuffling
                OriginalVusicQueueNext.Clear();
                foreach (var item in VusicQueueNext)
                {
                    OriginalVusicQueueNext.Add(item);
                }

                // 3. Shuffle our localized memory list copy
                var shuffledList = ShuffleItems(VusicQueueNext.ToList());

                // 4. IN-PLACE MOVE: Reorder elements cleanly without clearing the collection
                for (int i = 0; i < shuffledList.Count; i++)
                {
                    var targetItem = shuffledList[i];
                    targetItem.QueueControls = Visibility.Collapsed;

                    int currentLiveIndex = VusicQueueNext.IndexOf(targetItem);
                    if (currentLiveIndex >= 0 && currentLiveIndex != i)
                    {
                        VusicQueueNext.Move(currentLiveIndex, i);
                    }
                }
            }
            finally
            {
                IsLooping = false;
            }
            SyncFullQueueFromNext();
        }

        public static void RestoreNext()
        {
            if (OriginalVusicQueueNext.Count == 0) return;

            try
            {
                // 1. SILENCE HANDLERS: Freeze layout syncing while we restore order
                IsLooping = true;

                // 2. IN-PLACE RESTORE: Match the original backup index order exactly
                for (int i = 0; i < OriginalVusicQueueNext.Count; i++)
                {
                    var targetItem = OriginalVusicQueueNext[i];
                    targetItem.QueueControls = Visibility.Collapsed;

                    int currentLiveIndex = VusicQueueNext.IndexOf(targetItem);
                    if (currentLiveIndex >= 0 && currentLiveIndex != i)
                    {
                        VusicQueueNext.Move(currentLiveIndex, i);
                    }
                }
            }
            finally
            {
                IsLooping = false;
            }
            SyncFullQueueFromNext();
        }
        public static void RestoreAll()
        {
            VusicQueue.Clear();
            foreach (var item in OriginalVusicQueue)
            {
                VusicQueue.Add(item);
            }
        }
        public static List<SongModel> ShuffleItems(List<SongModel> list)
        {
            var items = list;
            int n = items.Count;


            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = items[k];
                items[k] = items[n];
                items[n] = value;
            }
            return items;
        }
        public static void ShuffleList()
        {
            try
            {
                var items = VusicQueue.ToList();
                int n = items.Count;


                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    var value = items[k];
                    items[k] = items[n];
                    items[n] = value;
                }
                VusicQueue.Clear();
                foreach (var item in items)
                {
                    VusicQueue.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error occured: " + ex.Message, "Shuffle Queue", Logger.LogLevelType.Error);
            }
        }
        private static void RemoveSong()
        {
            var song = VusicQueue.FirstOrDefault(x => x.FilePath == PlayerService.CurrentPlayingPath);
            if (song != null)
            {
                VusicQueueNext.Remove(song);
            }
        }
        public static void MarkSongCompleted()
        {
            var song2 = VusicQueue.FirstOrDefault(x => x.FilePath == PlayerService.CurrentPlayingPath);

            if (song2 != null)
            {
                song2.IsCompleted = true; Debug.WriteLine(song2.FilePath + " is to be removed now");

            }
        }


        public static void AddToQueue()
        {

        }
        public static void SyncFullQueueFromNext()
        {
            try
            {
                IsLooping = true; // Lock to protect VusicQueue modifications

                // 1. Separate the full queue into its macro groups
                var completedTracks = VusicQueue.Where(s => s.VisibilityOfStrikeThrough == Visibility.Visible).ToList();
                var currentlyPlaying = VusicQueue.Where(s => s.FilePath == PlayerService.CurrentPlayingPath
                                                          && s.VisibilityOfStrikeThrough != Visibility.Visible).ToList();

                // 2. Take the unplayed upcoming tracks EXACTLY as they were just shuffled
                var upcomingTracks = VusicQueueNext.ToList();

                // 3. Combine them into the target master blueprint sequence
                var sortedTargetList = completedTracks
                    .Concat(currentlyPlaying)
                    .Concat(upcomingTracks)
                    .ToList();

                // 4. Smoothly mirror the layout onto VusicQueue using Move
                for (int i = 0; i < sortedTargetList.Count; i++)
                {
                    var targetItem = sortedTargetList[i];
                    int currentLiveIndex = VusicQueue.IndexOf(targetItem);

                    if (currentLiveIndex >= 0 && currentLiveIndex != i)
                    {
                        VusicQueue.Move(currentLiveIndex, i);
                    }
                }
            }
            finally
            {
                IsLooping = false;
            }
        }
        public static void SyncQueueLists()
        {
            //Update VusicQueueNext:
            var completedItems = VusicQueue.Where(i => i.IsCompleted == true && i.VisibilityOfStrikeThrough == Visibility.Visible).ToList();
            foreach (var item in completedItems)
            {
                var exist = VusicQueueNext.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (exist != null)
                {
                    VusicQueueNext.Remove(exist);
                }
                var backupExist = OriginalVusicQueueNext.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (backupExist != null)
                {
                    OriginalVusicQueueNext.Remove(backupExist);
                }
            }
            var currentpath = VusicQueueNext.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (currentpath != null)
            {
                VusicQueueNext.Remove(currentpath);
            }
            var backupCurrent = OriginalVusicQueueNext.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (backupCurrent != null)
            {
                OriginalVusicQueueNext.Remove(backupCurrent);
            }
        }
    }
}
