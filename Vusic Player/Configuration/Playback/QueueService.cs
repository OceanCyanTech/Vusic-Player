using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Pages;
using Windows.Storage;

namespace Vusic_Player.Configuration.Playback
{
    public class QueueService
    {

        private static Random rng = new Random();
        public static int videoindex = -1;
        public static ObservableCollection<SongModel> VusicQueue { get; } = new();
        public static ObservableCollection<SongModel> OriginalVusicQueue { get; } = new();
        public static ObservableCollection<SongModel> VusicQueueNext { get; } = new();
        public static ObservableCollection<SongModel> OriginalVusicQueueNext { get; } = new();
        public static AdvancedCollectionView VusicQueueView { get; set; } = new();


        public static bool IsLoopTrue = false;
        public static bool IsShuffleTrue = false;

        public static void PlayMedia(ObservableCollection<SongModel> media, bool IsShuffleEnabled, bool IsLoopEnabled)
        {
            if (media != null)
            {
                PlayerService.CurrentPlayingPath = "";
                VusicQueue.Clear();

                foreach (var item in media)
                {

                    VusicQueue.Add(item);
                }

                if (IsShuffleEnabled)
                {
                    IsShuffleTrue = IsShuffleEnabled;
                    ShuffleList();
                }
                VusicQueueNext.Clear();
                foreach (var item in media)
                {

                    VusicQueueNext.Add(item);
                }



                PlayNext();

            }
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
                item.VisibilityOfStrikeThrough = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            var shuffledlist = ShuffleItems(VusicQueue.ToList());
            foreach (var item in shuffledlist)
            {
                VusicQueue.Add(item);
            }

        }
        public static void ShuffleNext()
        {
            OriginalVusicQueueNext.Clear();

            var itemsToAdd = VusicQueueNext.ToList();

            foreach (var item in itemsToAdd)
            {
                OriginalVusicQueueNext.Add(item);
            }
            var shuffledlist = ShuffleItems(VusicQueueNext.ToList());
            VusicQueueNext.Clear();
            foreach (var item in shuffledlist)
            {
                item.QueueControls = Visibility.Collapsed;
                VusicQueueNext.Add(item);
            }
        }
        public static void RestoreNext()
        {
            VusicQueueNext.Clear();
            foreach (var item in OriginalVusicQueueNext)
            {
                item.QueueControls = Visibility.Collapsed;
                VusicQueueNext.Add(item);
            }
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
        private static void RemoveSong()
        {
            var song = VusicQueue.FirstOrDefault(x => x.FilePath == PlayerService.CurrentPlayingPath);
            //int index = VusicQueueNext.IndexOf(song);
            //VusicQueueNext.RemoveAt(index +1);
            if (song != null)
            {
                VusicQueueNext.Remove(song);
            }
            //VusicQueueView.Filter = x => VusicQueue.IndexOf((SongModel)x) > videoindex;
        }
        //public static void MarkSongCompleted()
        //{
        //    var song2 = VusicQueue.FirstOrDefault(x => x.FilePath == PlayerService.CurrentPlayingPath);

        //    if (song2 != null)
        //    {
        //        song2.IsCompleted = true;
        //        song2.VisibilityOfStrikeThrough = Microsoft.UI.Xaml.Visibility.Visible;
        //        Debug.WriteLine(song2.FilePath + " is to be removed now");

        //    }


        //}
        //private static async Task PlayMediaAtIndex()
        //{
        //    var queue = VusicQueueNext;
        //    if (queue == null || queue.Count == 0) return;


        //    var item = queue[0];
        //    Debug.WriteLine(item.FilePath + " is to be played now");
        //    if (string.IsNullOrEmpty(item.FilePath)) return;

        //    // 3. Update state and play
        //    //    videoindex = targetIndex;
        //    var file = await StorageFile.GetFileFromPathAsync(item.FilePath);

        //    string fileExtension = file.FileType.ToLowerInvariant();
        //    bool isVideo = false;
        //    if (Extensions.VideoExtensions.List.Contains(fileExtension))
        //    {
        //        isVideo = true;
        //    }
        //    if (isVideo)
        //    {
        //        Debug.WriteLine("True");
        //        if(App.NavigationFrame != null)
        //        {
        //            if (PlayerService.InVideoPage == false)
        //            {
        //                App.NavigationFrame.Navigate(typeof(VideoPlayer), item.FilePath);
        //            }

        //        }


        //    }
        //    PlayerService.OpenPath(item.FilePath);
        //}
        //private static async Task PlayandRemoveAsync()
        //{
        //    if (VusicQueueNext == null || VusicQueueNext.Count == 0) return;

        //    // Capture the target item BEFORE we do anything asynchronous
        //    var firstitem2 = VusicQueueNext[0];

        //    // Await the media player initialization completely
        //    await PlayMediaAtIndex();

        //    Debug.WriteLine("Supposed to get removed is " + firstitem2.FilePath);
        //    if (firstitem2 != null)
        //    {
        //        Debug.WriteLine("Supposed to get removed is2 " + firstitem2.FilePath);
        //        VusicQueueNext.Remove(firstitem2);
        //    }

        //    foreach (var item in VusicQueueNext)
        //    {
        //        Debug.WriteLine(item.FilePath + " is new list");
        //    }

        //    if (IsShuffleTrue && firstitem2 != null)
        //    {
        //        var itemtoremove = OriginalVusicQueueNext.FirstOrDefault(p => p.FilePath == firstitem2.FilePath);
        //        if (itemtoremove != null)
        //        {
        //            OriginalVusicQueueNext.Remove(itemtoremove);
        //        }
        //    }
        //}
        //      REWRITE WHOLE CODE AGAIN FOR NEXT, MEDIA AT INDEX AND REMOVAL
        public static async void PlayNext()
        {
            if (VusicQueueNext.Count != 0)
            {
                var firstitem = VusicQueueNext[0];
                var FileToBePlayed = firstitem.FilePath;
                if (FileToBePlayed == null) return;
                VusicQueueNext.Remove(firstitem);
                var file = await StorageFile.GetFileFromPathAsync(FileToBePlayed);

                string fileExtension = file.FileType.ToLowerInvariant();
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
                            App.NavigationFrame.Navigate(typeof(VideoPlayer), FileToBePlayed);
                        }

                    }
                }

                PlayerService.OpenPath(FileToBePlayed);
            }
        }
        public static void PlayMediaAtIndex()
        {

        }
        //public static async void PlayNext()
        //{
        //    if (VusicQueueNext == null) return;

        //    if (VusicQueueNext.Count == 0)
        //    {
        //        if (IsLoopTrue)
        //        {
        //            var refreshedobservablecoll = new ObservableCollection<SongModel>();
        //            foreach (var item in VusicQueue.ToList())
        //            {
        //                item.IsCompleted = false;
        //                item.VisibilityOfStrikeThrough = Visibility.Collapsed;
        //                refreshedobservablecoll.Add(item);
        //            }

        //            VusicQueue.Clear();
        //            foreach (var items in refreshedobservablecoll)
        //            {
        //                VusicQueue.Add(items);
        //            }
        //            foreach (var items in VusicQueue)
        //            {
        //                items.QueueControls = Visibility.Collapsed;
        //                VusicQueueNext.Add(items);
        //            }

        //            await PlayandRemoveAsync();
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        Debug.WriteLine("Is true");
        //    }
        //    MarkSongCompleted();
        //    await PlayandRemoveAsync();
        //}
        public static void PlayPrevious()
        {
            var queue = VusicQueue;
            var currentPath = PlayerService.CurrentPlayingPath;
            if (queue == null || queue.Count == 0 || !File.Exists(currentPath)) return;
            int index = queue.ToList().FindIndex(p => p.FilePath == currentPath);
            if (index <= 0) return;

            var currentItem = queue[index];
            currentItem.IsCompleted = false;
            currentItem.VisibilityOfStrikeThrough = Visibility.Collapsed;
            var prevItem = queue[index - 1];

            if (prevItem == null || string.IsNullOrEmpty(prevItem.FilePath)) return;
            prevItem.VisibilityOfStrikeThrough = Visibility.Collapsed;
            prevItem.IsCompleted = false;

            VusicQueueNext.Insert(0, currentItem);
            PlayerService.OpenPath(prevItem.FilePath);
        }
    }
}
