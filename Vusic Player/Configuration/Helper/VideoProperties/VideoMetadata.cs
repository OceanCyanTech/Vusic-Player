using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.VideoProperties
{
    public class VideoMetadata
    {
        private static async Task<VideoInfo> GetVideoMetadataAsync(string ffprobePath, string videoFilePath)
        {
            Debug.WriteLine("CALLD");
            var startInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                // -v error tells ffprobe to cut down on extra log spam
                Arguments = $"-v error -select_streams v:0 -show_entries stream=codec_name,width,height,r_frame_rate -of json \"{videoFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true, // We keep this true, but we MUST drain it!
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            if (!process.Start()) return new VideoInfo();

            // DRAIN BOTH STREAMS SIMULTANEOUSLY TO PREVENT BUFFER DEADLOCKS
            Task<string> readOutputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> readErrorTask = process.StandardError.ReadToEndAsync();

            // Use a strict 4-second timeout so it can never hang up your system/files forever
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            try
            {
                await Task.WhenAll(readOutputTask, readErrorTask, process.WaitForExitAsync(cts.Token));
            }
            catch (OperationCanceledException)
            {
                // Force kill the process tree to instantly drop the file locks if it gets stuck
                process.Kill(entireProcessTree: true);
                Debug.WriteLine("ffprobe hung up on error buffers and was forcefully terminated.");
                return new VideoInfo();
            }

            string jsonOutput = readOutputTask.Result;

            if (string.IsNullOrWhiteSpace(jsonOutput)) return new VideoInfo();

            try
            {
                using var doc = JsonDocument.Parse(jsonOutput);

                // Ensure streams array actually has items
                if (!doc.RootElement.TryGetProperty("streams", out var streamsProp) || streamsProp.GetArrayLength() == 0)
                    return new VideoInfo();

                var stream = streamsProp[0];

                string codec = stream.TryGetProperty("codec_name", out var c) ? c.GetString() ?? "" : "";
                int width = stream.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
                int height = stream.TryGetProperty("height", out var h) ? h.GetInt32() : 0;

                string rawFrameRate = stream.TryGetProperty("r_frame_rate", out var r) ? r.GetString() ?? "" : "";
                double fps = CalculateFps(rawFrameRate);

                return new VideoInfo
                {
                    Codec = codec.ToUpper(),
                    Width = width,
                    Height = height,
                    FrameRate = Math.Round(fps, 2)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("JSON parsing error: " + ex.Message);
                return new VideoInfo();
            }
        }
        // Helper to safely convert "30000/1001" fractional strings into a double like 29.97
        private static double CalculateFps(string fraction)
        {
            if (string.IsNullOrEmpty(fraction) || !fraction.Contains("/")) return 0;
            var parts = fraction.Split('/');
            if (parts.Length == 2 && double.TryParse(parts[0], out double num) && double.TryParse(parts[1], out double den) && den != 0)
            {
                return num / den;
            }
            return 0;
        }
        public static async Task<VideoInfo> GetVideoMetadata(string videoFilePath)
        {
            string ffprobePath = Path.Combine(AppContext.BaseDirectory, "FFmpeg", "ffprobe.exe");
            var videoinfo = await GetVideoMetadataAsync(ffprobePath, videoFilePath);
            return videoinfo;
        }

        // Simple data carrier class
        public class VideoInfo
        {
            public string Codec { get; set; } = "";
            public int Width { get; set; }
            public int Height { get; set; }
            public double FrameRate { get; set; }
            public string DisplayResolution
            {
                get
                {
                    string friendlyName = Height switch
                    {
                        >= 2160 => "4K UHD",
                        >= 1440 => "1440p QHD",
                        >= 1080 => "1080p FHD",
                        >= 720 => "720p HD",
                        >= 480 => "480p SD",
                        _ => "SD"
                    };
                    return $"{Width} × {Height} ({friendlyName})";
                }
            }
        }
    }
}
