// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 241126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels.Media;
using Windows.Storage.Streams;

namespace Vusic_Player.Configuration.Vusic_API_CORE
{
    public static partial class VusicAPIMediaProperties
    {
        static VusicAPIMediaProperties()
        {
            NativeLibrary.SetDllImportResolver(typeof(VusicAPIMediaProperties).Assembly, (libraryName, assembly, searchPath) =>
            {
                if (libraryName.EndsWith("VusicMediaProperties.dll", StringComparison.OrdinalIgnoreCase))
                {
                    string ffmpegDir = System.IO.Path.Combine(AppContext.BaseDirectory, "FFmpeg");

                    NativeLibrary.Load(System.IO.Path.Combine(ffmpegDir, "avutil-61.dll"));
                    NativeLibrary.Load(System.IO.Path.Combine(ffmpegDir, "swresample-7.dll")); // omit if avcodec-63 doesn't import it
                    NativeLibrary.Load(System.IO.Path.Combine(ffmpegDir, "avcodec-63.dll"));
                    NativeLibrary.Load(System.IO.Path.Combine(ffmpegDir, "avformat-63.dll"));

                    return NativeLibrary.Load(System.IO.Path.Combine(ffmpegDir, "VusicMediaProperties.dll"));
                }
                return IntPtr.Zero;
            });
        }
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct MediaProperties
        {
            public long duration_microseconds;//
            public int width;//
            public int height;//
            public int tracknumber;    //
            public int totaltracks;    //
            public long audiobitrate;   //
            public long videobitrate;   //
            public long channels;   //
            public long samplerate;     //
            public long filesize;   //
            public fixed byte title[512]; //
            public fixed byte artist[512]; //
            public fixed byte lyricist[512];//
            public fixed byte fileformat[512];//
            public fixed byte album[512];//
            public fixed byte contributingartists[512];//
            public fixed byte composer[512];//
            public fixed byte copyright[512];//
            public fixed byte publisher[512];//
            public fixed byte date[512];//
            public fixed byte creation_time[512];
            public fixed byte performer[512];//
            public fixed byte genre[512];//
            public fixed byte comment[512];//
            public fixed byte video_codec[64];//
            public fixed byte audio_codec[64];//
            public long totalframes;//
            public IntPtr cover_data;//
            public int cover_size;//
        }

        [LibraryImport(@"FFmpeg\VusicMediaProperties.dll", StringMarshalling = StringMarshalling.Utf8)]
        public static partial int GetMediaProperties(string filepath, out MediaProperties mediainfo);

        [LibraryImport(@"FFmpeg\VusicMediaProperties.dll")]
        public static partial void FreeMediaProperties(ref MediaProperties mediainfo);
        private static unsafe string UTF8ToString(byte* buffer, int maxlength)
        {
            int len = 0;
            while (len < maxlength && buffer[len] != 0) len++;
            return len == 0 ? string.Empty : Encoding.UTF8.GetString(buffer, len);
        }
        private static unsafe string UTF8ToStringOnlyFirst(byte* buffer, int maxlength)
        {
            int len = 0;
            while (len < maxlength && buffer[len] != 0) len++;
            string total = Encoding.UTF8.GetString(buffer, len);
            var split = total.Split(',');
            return len == 0 ? string.Empty : split[0];
        }
        public static async Task<BitmapImage> BytesToBitmap(IntPtr coverptr, int coversize, int decodepixelwidth = 300)
        {
            if (coverptr == IntPtr.Zero || coversize <= 0) return null;

            byte[] imageBytes = new byte[coversize];
            Marshal.Copy(coverptr, imageBytes, 0, coversize);
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(imageBytes.AsBuffer());

            stream.Seek(0);

            var bitmapimage = new BitmapImage();
            if (decodepixelwidth > 0)
            {
                bitmapimage.DecodePixelWidth = decodepixelwidth;
            }
            await bitmapimage.SetSourceAsync(stream);
            return bitmapimage;
        }
        private static unsafe string LongToDurationString(long microseconds)
        {
            var timespan = TimeSpan.FromMicroseconds((double)microseconds);
            return timespan.TotalHours >= 1 ? $"{(int)timespan.TotalHours:D2}:{timespan.Minutes:D2}:{timespan.Seconds:D2}"
                    : $"{timespan.Minutes:D2}:{timespan.Seconds:D2}";
        }
        private static unsafe string LongToBitrateString(long bitrate)
        {
            var kbps = bitrate / 1000;
            return $"{kbps} kbps";
        }
        private static unsafe string LongToSampleRateString(long samplerate)
        {
            return $"{samplerate} Hz";
        }

        private static unsafe TimeSpan DurationValue(long microseconds)
        {
            return TimeSpan.FromMicroseconds((double)microseconds);
        }
        private static unsafe string BytesToFileSize(long sizeinbytes)
        {
            double mb = sizeinbytes / 1_000_000.0;

            return mb < 1
                ? $"{sizeinbytes / 1_000.0:0.##} KB"
                : mb >= 1000
                    ? $"{sizeinbytes / 1_000_000_000.0:0.##} GB"
                    : $"{mb:0.##} MB";
        }
        private static unsafe string LongToChannels(long channelcount)
        {
            return $"{channelcount} {(channelcount == 1 ? "channel" : "channels")}";
        }
        private static unsafe int LongToInt(long value)
        {
            return (int)value;
        }
        private static unsafe string ResolutionString(int width, int height)
        {
            return $"{width} x {height}";
        }
        private static unsafe int CreationYear(string date)
        {
            if (DateTime.TryParse(date, out DateTime result))
            {
                return result.Year;
            }
            else
            {
                if (int.TryParse(date, out int resultint))
                    return resultint;
            }
            return 1998;
        }
        public static  async Task< MediaMetadata> GetFileInfo(string filepath)
        {
            int result = GetMediaProperties(filepath, out MediaProperties raw);
            if (result >= 0)
            {
                try
                {
                    var cover = await BytesToBitmap(raw.cover_data, raw.cover_size);
                    unsafe
                    {
                        return new MediaMetadata { Title = UTF8ToString(raw.title, 512), Album = (UTF8ToString(raw.album, 512)), Artist = (UTF8ToString(raw.artist, 512)), Duration = DurationValue(raw.duration_microseconds), DurationString = LongToDurationString(raw.duration_microseconds), AudioBitrate = LongToBitrateString(raw.audiobitrate), Comment = (UTF8ToString(raw.comment, 512)), Composer = (UTF8ToString(raw.composer, 512)), Copyright = UTF8ToString(raw.copyright, 512), Performer = UTF8ToString(raw.publisher, 512), ContributingArtists = UTF8ToString(raw.contributingartists, 512), Publisher = UTF8ToString(raw.publisher, 512), Lyricist = UTF8ToString(raw.lyricist, 512), Genre = UTF8ToString(raw.genre, 512), FileFormat = UTF8ToString(raw.fileformat, 512), SampleRate = LongToSampleRateString(raw.samplerate), AudioCodec = UTF8ToString(raw.audio_codec, 512), VideoCodec = UTF8ToString(raw.video_codec, 512), VideoBitrate = LongToBitrateString(raw.videobitrate), FileSize = BytesToFileSize(raw.filesize), Channels = LongToChannels(raw.channels), Width = raw.width, Height = raw.height, VideoResolution = ResolutionString(raw.width, raw.height), TotalFrames = raw.totalframes, TotalTracks = raw.totaltracks, TrackNumber = raw.tracknumber, Date = UTF8ToString(raw.date, 512), Year = CreationYear((UTF8ToString(raw.date, 512))), Cover = cover };
                    }
                }
                finally
                {
                    FreeMediaProperties(ref raw);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
