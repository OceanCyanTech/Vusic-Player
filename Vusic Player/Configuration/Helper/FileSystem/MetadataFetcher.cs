// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 241126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Vusic_API_CORE;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class MetadataFetcher
    {
        public static async Task<string> GetDurationString(string FilePath)
        {
            var mediametadata = await VusicAPIMediaProperties.GetFileInfo(FilePath);
            return string.IsNullOrEmpty(mediametadata.DurationString) ? "00:00" : mediametadata.DurationString;
        }
        public static async Task<TimeSpan> GetDuration(string FilePath)
        {
            var mediametadata = await VusicAPIMediaProperties.GetFileInfo(FilePath);
            return mediametadata.Duration;
        }
        public static async Task<string> GetMediaTitle(string FilePath)
        {
            var mediametadata = await VusicAPIMediaProperties.GetFileInfo(FilePath);
            return string.IsNullOrEmpty(mediametadata.Title) ? Path.GetFileNameWithoutExtension(FilePath) : mediametadata.Title;
        }
        public static async Task<int> GetMediaYear(string FilePath)
        {
            var mediametadata = await VusicAPIMediaProperties.GetFileInfo(FilePath);
            return mediametadata.Year;
        }
        public static async Task<string> GetAudioArtist(string FilePath)
        {
            Debug.WriteLine("Audio  ARTIST Called for : " + FilePath);
            var mediametadata = await VusicAPIMediaProperties.GetFileInfo(FilePath);
            Debug.WriteLine("ARTIST IS " + mediametadata.Artist);

            return string.IsNullOrEmpty(mediametadata.Artist) ? "Unknown Artist" : mediametadata.Artist;
        }
        public static async Task<SongModel> GetSongModel(string FilePathFetch, bool isQueueItem =false)
        {
            Debug.WriteLine("SONG MODEL CALLED FOR  " + FilePathFetch);

            string title = await GetMediaTitle(FilePathFetch);
            TimeSpan duration = await GetDuration(FilePathFetch);

            if (ExtensionVerifier.FileTypeExtension(FilePathFetch) == ExtensionVerifier.FileTypeEnum.Audio)
            {
                Debug.WriteLine("YES AUDIO FILE   ");

                string artist = await GetAudioArtist(FilePathFetch);
                string album = await GetAudioAlbum(FilePathFetch);

                Debug.WriteLine("ARTIST CAME OUT TO BE   " + artist);

                return new SongModel
                {
                    FilePath = FilePathFetch,
                    Title = title,
                    Artist = artist,
                    AlbumName = album,
                    SongDuration = duration,
                    IsQueueItem = isQueueItem
                };
            }

            return new SongModel
            {
                FilePath = FilePathFetch,
                Title = title,
                SongDuration = duration,
                IsQueueItem = isQueueItem
            };
        }
        public static async Task<SongModel>GetSongModelAudio(string FilePathFetch)
        {
            return new SongModel { FilePath = FilePathFetch, Title = await GetMediaTitle(FilePathFetch), Artist = await GetAudioArtist(FilePathFetch), AlbumName = await GetAudioAlbum(FilePathFetch), SongDuration = await GetDuration(FilePathFetch) };
        }
        public static async Task<SongModel> GetSongModelVideo(string FilePathFetch)
        {
            return new SongModel { FilePath = FilePathFetch, Title = await GetMediaTitle(FilePathFetch),  SongDuration = await GetDuration(FilePathFetch) };
        }
        public static async Task<string> GetAudioAlbum(string FilePath)
        {
            var mediametadata = await VusicAPIMediaProperties.GetFileInfo(FilePath);
            return string.IsNullOrEmpty(mediametadata.Album) ? "Unknown Album" : mediametadata.Album;
        }
    }
}
