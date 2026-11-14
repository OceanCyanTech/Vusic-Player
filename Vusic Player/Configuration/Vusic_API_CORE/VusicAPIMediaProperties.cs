// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels.Media;

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
            public long duration_microseconds;
            public int width;
            public int height;
            public fixed byte title[512];
            public fixed byte artist[512];
            public fixed byte album[512];
            public fixed byte creation_timer[512];
            public fixed byte genre[512];

            public fixed byte video_codec[64];
            public IntPtr cover_data;
            public int cover_size;
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
        private static unsafe string LongToDurationString(long microseconds)
        {
            var timespan = TimeSpan.FromMicroseconds((double)microseconds);
            return timespan.TotalHours >= 1 ? $"{(int)timespan.TotalHours:D2}:{timespan.Minutes:D2}:{timespan.Seconds:D2}"
                    : $"{timespan.Minutes:D2}:{timespan.Seconds:D2}";
        }
        private static unsafe TimeSpan DurationValue(long microseconds)
        {
            return TimeSpan.FromMicroseconds((double)microseconds);
        }
        public static unsafe MediaMetadata? GetFileInfo(string filepath)
        {

            int result = GetMediaProperties(filepath, out MediaProperties raw);
            if (result >= 0)
            {
                try
                {

                    return new MediaMetadata { Title = UTF8ToString(raw.title, 512), Album = (UTF8ToString(raw.album, 512)), Artist = (UTF8ToString(raw.artist, 512)), Duration = DurationValue(raw.duration_microseconds), DurationString = LongToDurationString(raw.duration_microseconds) };
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
