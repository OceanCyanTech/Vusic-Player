// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.Configuration.Helper.VideoProperties
{
    public class Screen : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // --- Observable Properties ---
        public static Screen Instance = new Screen();
        private bool _isTimeStampIncluded;
        public bool IsTimeStampIncluded
        {
            get => _isTimeStampIncluded;
            set => SetField(ref _isTimeStampIncluded, value);
        }

        private bool _isPositionIncluded;
        public bool IsPositionIncluded
        {
            get => _isPositionIncluded;
            set => SetField(ref _isPositionIncluded, value);
        }

        private string _snapshotDirect = string.Empty;
        public string SnapshotDirect
        {
            get => _snapshotDirect;
            set => SetField(ref _snapshotDirect, value);
        }

        private string _recordDirect = string.Empty;
        public string RecordDirect
        {
            get => _recordDirect;
            set => SetField(ref _recordDirect, value);
        }

        private string _currentRecordPath = string.Empty;
        public string CurrentRecordPath
        {
            get => _currentRecordPath;
            set => SetField(ref _currentRecordPath, value);
        }

        // --- Value Converter Logic ---

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isRecording = value is bool b && b;
            string type = parameter as string ?? string.Empty;

            if (type == "Icon")
                return isRecording ? "\uE7C8" : "\uE714"; // Stop icon vs Record icon

            return isRecording ? "Stop Recording" : "Start Recording";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, string? language) => null;

        // --- Events & Service Actions ---

        public static event Action? OnRecordRequest;
        public static event Action? OnRecordStopRequest;
        public static event Action<int, bool>? WatermarkVisibilityChanged;
        public static event Action<int, string, string, TimeSpan, WatermarkMode, DateTime?, double, double, int, string>? WatermarkCalled;

        public static void SetWatermarkVisibility(int labelIndex, bool isVisible)
        {
            WatermarkVisibilityChanged?.Invoke(labelIndex, isVisible);
        }

        public static void CallWatermark(
            int index,
            string position,
            string format,
            TimeSpan offset,
            WatermarkMode mode,
            DateTime baseDateTime,
            double frameRate = 30.0,
            double bpm = 120.0,
            int beatsPerBar = 4,
            string customwatermarktext = "")
        {
            WatermarkCalled?.Invoke(index, position, format, offset, mode, baseDateTime, frameRate, bpm, beatsPerBar, customwatermarktext);
        }

        public void TakeSnapshot(string snapshotPath, bool timeStampIncluded, bool positionIncluded)
        {
            if (PlayerService.Masterplayer == null) return;

            string targetFolder = string.IsNullOrWhiteSpace(snapshotPath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VusicPlayer Snapshots")
                : snapshotPath;

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fullPath = Path.Combine(targetFolder, $"Snapshot_{timestamp}.jpg");

            PlayerService.Masterplayer.TakeSnapshotToFile(fullPath, 0, 0, timeStampIncluded, positionIncluded);
            GeneralInfoService.ShowInfo($"Snapshot taken and saved to {fullPath}");
        }

        public void Record(string recordPath)
        {
            if (PlayerService.Masterplayer == null) return;

            if (!PlayerService.Masterplayer.IsPlaying)
            {
                PlayerService.Play();
            }

            string videosFolder = string.IsNullOrWhiteSpace(recordPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
                : recordPath;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Vusic Player Recording - {Path.GetFileName(PlayerService.CurrentPlayingPath)} ({timestamp}).mp4";
            string fullPath = Path.Combine(videosFolder, fileName);

            CurrentRecordPath = fullPath;
            PlayerService.Masterplayer.StartRecording(ref fullPath, true);
            OnRecordRequest?.Invoke();
        }

        public void StopRecordRequest()
        {
            OnRecordStopRequest?.Invoke();
        }

        public void StopRecord()
        {
            PlayerService.Masterplayer?.StopRecording();
            GeneralInfoService.ShowInfo($"Recording stopped and saved to {CurrentRecordPath}.");
        }
    }
}