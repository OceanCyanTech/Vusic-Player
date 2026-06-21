using System;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace Vusic_Player.Configuration.ClassModels
{
    public class SearchVideoOptions
    {
        public static ObservableCollection<SettingSearchResult> searchIndex = new ObservableCollection<SettingSearchResult>();
        public static void IndexResults(
            // Video Targets
            FrameworkElement stkZoom, FrameworkElement stkPlaybackSpeed, FrameworkElement stkVideoStream,
           FrameworkElement stkSnapshot, FrameworkElement stkRecord,
            FrameworkElement stkFilters, FrameworkElement stkRotation, FrameworkElement stkFlip,
            FrameworkElement grdcustomAspectRatio,
            // Audio Targets
            FrameworkElement stkPitch, FrameworkElement stkAudioStream, FrameworkElement stkAudioDevice,
            FrameworkElement stkAudioDelay, FrameworkElement stkEqualizer,
            // Subtitle Targets
            FrameworkElement stkSubtitleStream, FrameworkElement stkSubtitleExternal, FrameworkElement grdSubtitlesCustomize, FrameworkElement stkSubDelay)
        {
            // --- VIDEO SECTION (TabIndex 0) ---

            string[] zoomkeywords = { "zoom", "glow", "scale", "view" };
            searchIndex.Add(new SettingSearchResult { Name = "View", Keywords = zoomkeywords, TabIndex = 0, SegmentIndex = 0, TargetGrid = stkZoom });

            string[] speedkeywords = { "speed", "reverse", "loop", "fast", "slow", "playback" };
            searchIndex.Add(new SettingSearchResult { Name = "Playback", Keywords = speedkeywords, TabIndex = 0, SegmentIndex = 0, TargetGrid = stkPlaybackSpeed });

            string[] videostream = { "stream", "track" };
            searchIndex.Add(new SettingSearchResult { Name = "Video Stream", Keywords = videostream, TabIndex = 0, SegmentIndex = 0, TargetGrid = stkVideoStream });



            string[] snapshot = { "snapshot", "screenshot", "photo", "capture", "frame" };
            searchIndex.Add(new SettingSearchResult { Name = "Frame Snapshot", Keywords = snapshot, TabIndex = 0, SegmentIndex = 0, TargetGrid = stkSnapshot });

            string[] videorecord = { "record", "screenrecord" };
            searchIndex.Add(new SettingSearchResult { Name = "Screen Record", Keywords = videorecord, TabIndex = 0, SegmentIndex = 0, TargetGrid = stkRecord });

            string[] videofilters = { "brightness", "hue", "contrast", "saturation", "color" };
            searchIndex.Add(new SettingSearchResult { Name = "Video Filters", Keywords = videofilters, TabIndex = 0, SegmentIndex = 1, TargetGrid = stkFilters });

            string[] rotation = { "rotate", "rotation", "orientation" };
            searchIndex.Add(new SettingSearchResult { Name = "Video Rotation", Keywords = rotation, TabIndex = 0, SegmentIndex = 2, TargetGrid = stkRotation });

            string[] mirro = { "flip", "mirror", "mirroring", "horizontal", "vertical" };
            searchIndex.Add(new SettingSearchResult { Name = "Video Flip", Keywords = mirro, TabIndex = 0, SegmentIndex = 2, TargetGrid = stkFlip });

            string[] aspectratiokeywords = { "width", "height", "16:9", "resolution", "aspect ratio" };
            searchIndex.Add(new SettingSearchResult { Name = "Aspect Ratio", Keywords = aspectratiokeywords, TabIndex = 0, SegmentIndex = 3, TargetGrid = grdcustomAspectRatio });


            // --- AUDIO SECTION (TabIndex 1) ---

            string[] pitchkeywords = { "pitch", "octaves" };
            searchIndex.Add(new SettingSearchResult { Name = "Pitch", Keywords = pitchkeywords, TabIndex = 1, SegmentIndex = 0, TargetGrid = stkPitch });

            searchIndex.Add(new SettingSearchResult { Name = "Audio Stream", Keywords = videostream, TabIndex = 1, SegmentIndex = 0, TargetGrid = stkAudioStream });

            string[] audiodevice = { "device", "headphones", "output", "audio", "speaker" };
            searchIndex.Add(new SettingSearchResult { Name = "Audio Device", Keywords = audiodevice, TabIndex = 1, SegmentIndex = 0, TargetGrid = stkAudioDevice });

            string[] audiodelay = { "delay", "latency", "sync", "audio", "lip sync", "lag" };
            searchIndex.Add(new SettingSearchResult { Name = "Audio Delay", Keywords = audiodelay, TabIndex = 1, SegmentIndex = 0, TargetGrid = stkAudioDelay });

            string[] subdelay = { "delay", "latency", "sync", "subtitle", "text", "captions" };
            searchIndex.Add(new SettingSearchResult { Name = "Subtitle Delay", Keywords = subdelay, TabIndex = 2, SegmentIndex = 0, TargetGrid = stkSubDelay });

            string[] equalizerKeywords = { "equalizer", "eq", "audio profile", "bass", "treble", "frequencies", "sound" };
            searchIndex.Add(new SettingSearchResult { Name = "Equalizer", Keywords = equalizerKeywords, TabIndex = 1, SegmentIndex = 1, TargetGrid = stkEqualizer });


            // --- SUBTITLE SECTION (TabIndex 2) ---

            string[] subtitleKeywords = { "subtitles", "subs", "captions", "cc", "text", "language", "stream", "track" };
            searchIndex.Add(new SettingSearchResult { Name = "Subtitle Tracks", Keywords = subtitleKeywords, TabIndex = 2, SegmentIndex = 0, TargetGrid = stkSubtitleStream });

            string[] subtitleExternalKeywords = { "external subtitles", "add subtitles", "browse", "file", "srt", "ass", "vtt", "import", "track", "stream" };
            searchIndex.Add(new SettingSearchResult { Name = "External Subtitles", Keywords = subtitleExternalKeywords, TabIndex = 2, SegmentIndex = 0, TargetGrid = stkSubtitleExternal });

            string[] subtitleCustomKeywords = { "customize", "appearance", "font", "color", "size", "style", "background", "position", "margin", "opacity" };
            searchIndex.Add(new SettingSearchResult { Name = "Subtitle Customization", Keywords = subtitleCustomKeywords, TabIndex = 2, SegmentIndex = 1, TargetGrid = grdSubtitlesCustomize });
        }
    }

}
