// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Extensions;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class ExtensionVerifier
    {
        public enum FileTypeEnum
        {
            Video, Audio, Subtitle, Lyric, Other
        }
        public static FileTypeEnum FileTypeExtension(string filepath)
            {
            var extension = Path.GetExtension(filepath).ToLowerInvariant();
            Debug.WriteLine("EXTENSION ASKED FOR:  " + filepath + ". SO HERE U GO: " + extension);
            if (AudioExtensions.List.Contains(extension))
            {
                return FileTypeEnum.Audio;
            }
            else if (VideoExtensions.List.Contains(extension))
            {
                return  FileTypeEnum.Video;
            }
            else if (SubtitlesExtensions.List.Contains(extension))
            {
                return FileTypeEnum.Subtitle;
            }
            else if (LyricExtensions.List.Contains(extension))
            {
                return FileTypeEnum.Lyric;
            }
            return FileTypeEnum.Other;
        }
    }
}
