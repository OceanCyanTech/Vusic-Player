using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.UI;

namespace Vusic_Player.Configuration.Helper.VideoProperties
{
    public class Screen
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isRecording = (bool)value;
            string type = parameter as string ?? "";

            if (type == "Icon")
                return isRecording ? "\uE7C8" : "\uE714"; // Stop icon vs Record icon

            return isRecording ? "Stop Recording" : "Start Recording";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
        public static event Action? OnRecordRequest;
        public static event Action? OnRecordStopRequest;
        public static string currentRecordPath = string.Empty;
        public static void TakeSnapshot(string SnapshotPath, bool timeStampIncluded, bool positionIncluded)
        {
            if (PlayerService.Masterplayer != null)
            {
                string rootPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string targetFolder = Path.Combine(rootPictures, "VusicPlayer Snapshots");
                if (SnapshotPath != "")
                {
                    targetFolder = SnapshotPath;
                }
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fullPath = Path.Combine(targetFolder, $"Snapshot_{timestamp}.jpg");

                PlayerService.Masterplayer.TakeSnapshotToFile(fullPath, 0, 0, timeStampIncluded, positionIncluded);
                GeneralInfoService.ShowInfo($"Snapshot taken and saved to {fullPath}");

            }
        }
        public static void Record(string RecordPath)
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.Masterplayer.IsPlaying == false)
            {
                PlayerService.Play();
            }
            string videosFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            if (RecordPath != "")
            {
                videosFolder = RecordPath;
            }
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Vusic Player Recording - {Path.GetFileName(PlayerService.CurrentPlayingPath)} ({timestamp}).mp4";
            string fullPath = Path.Combine(videosFolder, fileName);
            currentRecordPath = fullPath;
            PlayerService.Masterplayer.StartRecording(ref fullPath, true);
            OnRecordRequest?.Invoke();

        }
        public static void StopRecordRequest()
        {
            OnRecordStopRequest?.Invoke();
        }
        public static bool IsTimeStampIncluded = false;
        public static bool IsPositionIncluded = false;
        public static string SnapshotDirect = "";
        public static string RecordDirect = "";
        public static void StopRecord()
        {
            PlayerService.Masterplayer?.StopRecording();
            GeneralInfoService.ShowInfo($"Recording stopped and saved to {currentRecordPath}.");
        }
    }

}
