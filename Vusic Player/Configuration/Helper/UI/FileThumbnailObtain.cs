using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class FileThumbnailObtain
    {
        public static async Task<BitmapImage> GetVideoFrameAsync(string path, double percentage = 0.20)
        {
            // Define your fallback URI
            Uri fallbackUri = new Uri("ms-appx:///Assets/default.png");

            try
            {
                // 1. Get Duration using ffprobe
                string durationRaw = await RunProcessAsync(@"C:\Users\bnara\Videos\FFmpeg\ffprobe.exe",
                    $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"");

                if (!double.TryParse(durationRaw.Trim(), out double totalSeconds))
                    totalSeconds = 10;

                double seekSeconds = totalSeconds * percentage;

                // 2. Extract Frame
                string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
                string ffmpegArgs = $"-ss {seekSeconds} -i \"{path}\" -vf \"zscale=t=linear:npl=100,format=gbrp,zscale=p=bt709,tonemap=tonemap=mobius:peak=100,format=yuv420p,eq=brightness=0.18:contrast=1.1\" -frames:v 1 -q:v 2 -update 1 \"{tempFile}\" -y";                // CRITICAL: You must call RunProcessAsync BEFORE checking if the file exists
                await RunProcessAsync(@"C:\Users\bnara\Videos\FFmpeg\ffmpeg.exe", ffmpegArgs);

                if (File.Exists(tempFile))
                {
                    var bitmap = new BitmapImage();
                    using (var stream = File.OpenRead(tempFile))
                    {
                        await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                    }
                    File.Delete(tempFile);
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thumbnail Error: {ex.Message}");
            }

            // 3. Fallback: If everything above fails or file doesn't exist
            return new BitmapImage(fallbackUri);
        }
        public static async Task<string> ExtractVideoFrameToFileAsync(string path, double percentage = 0.20)
        {
            try
            {
                // 1. Get Duration using ffprobe
                string durationRaw = await RunProcessAsync(@"C:\Users\bnara\Videos\FFmpeg\ffprobe.exe",
                    $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"");

                if (!double.TryParse(durationRaw.Trim(), out double totalSeconds))
                    totalSeconds = 10;

                double seekSeconds = totalSeconds * percentage;

                // 2. Extract Frame to Temp Path
                string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
                //string ffmpegArgs = $"-ss {seekSeconds} -i \"{path}\" -vf \"zscale=t=linear:npl=100,format=gbrp,zscale=p=bt709,tonemap=tonemap=mobius:peak=100,format=yuv420p,eq=brightness=0.18:contrast=1.1\" -frames:v 1 -q:v 2 -update 1 \"{tempFile}\" -y";
                string ffmpegArgs = $"-ss {seekSeconds} -i \"{path}\" -vf \"zscale=p=bt2020:t=smpte2084:m=bt2020nc,zscale=p=bt709:t=bt709:m=bt709:r=tv,eq=brightness=0.22:contrast=1.15\" -frames:v 1 -q:v 2 -update 1 \"{tempFile}\" -y"; await RunProcessAsync(@"C:\Users\bnara\Videos\FFmpeg\ffmpeg.exe", ffmpegArgs);

                if (File.Exists(tempFile))
                {
                    return tempFile; // Return the path to the file to be handled by UI thread
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFmpeg Extraction Error: {ex.Message}");
            }

            return string.Empty;
        }
        private static async Task<string> RunProcessAsync(string fileName, string args)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output;
        }

        public static async Task<BitmapImage> GetFileThumbnailAsync(string path)
        {
            // Define your fallback asset
            Uri fallbackUri = new Uri("ms-appx:///Assets/default.png");

            try
            {
                if (string.IsNullOrEmpty(path))
                    return new BitmapImage(fallbackUri);
                if (!File.Exists(path)) return new BitmapImage(fallbackUri); ;
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                //    uint requestedSize = 2048;

                // 2. Use ThumbnailOptions.ResizeThumbnail to ensure it matches your size
                using var thumbnail = await file.GetScaledImageAsThumbnailAsync(
                    ThumbnailMode.SingleItem,
                    2048,
                    ThumbnailOptions.None);

                if (thumbnail != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    // This connects the stream to the UI object
                    await bitmapImage.SetSourceAsync(thumbnail);
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Thumbnail extraction failed: {ex.Message}", "HomePage", Logger.LogLevelType.Error);
            }

            // If everything fails, return the app icon
            return new BitmapImage(fallbackUri);
        }

    }

}
