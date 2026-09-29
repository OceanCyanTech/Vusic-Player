using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Text;
using Vusic_Player;
using Vusic_Player.Configuration.ClassModels;
using Microsoft.UI.Xaml;
using System;

namespace Vusic_Player.Configuration.Playback
{
    public enum WatermarkMode
    {
        ElapsedPlaybackTime,        // Elapsed Time of Video
        RemainingPlaybackTime,      // Remaining Time of Video
        CurrentVsTotalDuration,     // Current Time vs Total Duration
        CurrentSystemTime,          // Current System Time
        CustomInitialOffset,        // Custom Initial Offset
        RunningFrames,              // Running Frames (Frame Rate to be Selected)
        MusicalTimecode             // Musical Timecode (Bars and Beats)
    }
    public class WatermarkFontValues : INotifyPropertyChanged
    {
        public static WatermarkFontValues Instance { get; } = new WatermarkFontValues();

        #region Backing Fields

        // Label 1
        private double _label1FontSize = 36;
        private Brush _label1FontColor = new SolidColorBrush(Colors.White);
        private string _label1FontFamily = "Segoe UI";
        private FontWeight _label1FontWeight = FontWeights.Normal;
        private FontStyle _label1FontStyle = FontStyle.Normal;
        private TimeSpan _label1Offset = TimeSpan.Zero;
        private HorizontalAlignment _label1HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label1VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label1Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 2
        private double _label2FontSize = 36;
        private Brush _label2FontColor = new SolidColorBrush(Colors.White);
        private string _label2FontFamily = "Segoe UI";
        private FontWeight _label2FontWeight = FontWeights.Normal;
        private FontStyle _label2FontStyle = FontStyle.Normal;
        private TimeSpan _label2Offset = TimeSpan.Zero;
        private HorizontalAlignment _label2HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label2VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label2Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 3
        private double _label3FontSize = 36;
        private Brush _label3FontColor = new SolidColorBrush(Colors.White);
        private string _label3FontFamily = "Segoe UI";
        private FontWeight _label3FontWeight = FontWeights.Normal;
        private FontStyle _label3FontStyle = FontStyle.Normal;
        private TimeSpan _label3Offset = TimeSpan.Zero;
        private HorizontalAlignment _label3HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label3VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label3Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 4
        private double _label4FontSize = 36;
        private Brush _label4FontColor = new SolidColorBrush(Colors.White);
        private string _label4FontFamily = "Segoe UI";
        private FontWeight _label4FontWeight = FontWeights.Normal;
        private FontStyle _label4FontStyle = FontStyle.Normal;
        private TimeSpan _label4Offset = TimeSpan.Zero;
        private HorizontalAlignment _label4HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label4VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label4Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 5
        private double _label5FontSize = 36;
        private Brush _label5FontColor = new SolidColorBrush(Colors.White);
        private string _label5FontFamily = "Segoe UI";
        private FontWeight _label5FontWeight = FontWeights.Normal;
        private FontStyle _label5FontStyle = FontStyle.Normal;
        private TimeSpan _label5Offset = TimeSpan.Zero;
        private HorizontalAlignment _label5HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label5VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label5Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 6
        private double _label6FontSize = 36;
        private Brush _label6FontColor = new SolidColorBrush(Colors.White);
        private string _label6FontFamily = "Segoe UI";
        private FontWeight _label6FontWeight = FontWeights.Normal;
        private FontStyle _label6FontStyle = FontStyle.Normal;
        private TimeSpan _label6Offset = TimeSpan.Zero;
        private HorizontalAlignment _label6HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label6VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label6Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 7
        private double _label7FontSize = 36;
        private Brush _label7FontColor = new SolidColorBrush(Colors.White);
        private string _label7FontFamily = "Segoe UI";
        private FontWeight _label7FontWeight = FontWeights.Normal;
        private FontStyle _label7FontStyle = FontStyle.Normal;
        private TimeSpan _label7Offset = TimeSpan.Zero;
        private HorizontalAlignment _label7HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label7VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label7Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 8
        private double _label8FontSize = 36;
        private Brush _label8FontColor = new SolidColorBrush(Colors.White);
        private string _label8FontFamily = "Segoe UI";
        private FontWeight _label8FontWeight = FontWeights.Normal;
        private FontStyle _label8FontStyle = FontStyle.Normal;
        private TimeSpan _label8Offset = TimeSpan.Zero;
        private HorizontalAlignment _label8HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label8VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label8Mode = WatermarkMode.ElapsedPlaybackTime;

        // Label 9
        private double _label9FontSize = 36;
        private Brush _label9FontColor = new SolidColorBrush(Colors.White);
        private string _label9FontFamily = "Segoe UI";
        private FontWeight _label9FontWeight = FontWeights.Normal;
        private FontStyle _label9FontStyle = FontStyle.Normal;
        private TimeSpan _label9Offset = TimeSpan.Zero;
        private HorizontalAlignment _label9HorizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _label9VerticalAlignment = VerticalAlignment.Top;
        private WatermarkMode _label9Mode = WatermarkMode.ElapsedPlaybackTime;

        #endregion

        #region Properties

        // --- Label 1 ---
        public double Label1FontSize { get => _label1FontSize; set => SetProperty(ref _label1FontSize, value); }
        public Brush Label1FontColor { get => _label1FontColor; set => SetProperty(ref _label1FontColor, value); }
        public string Label1FontFamily { get => _label1FontFamily; set => SetProperty(ref _label1FontFamily, value); }
        public FontWeight Label1FontWeight { get => _label1FontWeight; set => SetProperty(ref _label1FontWeight, value); }
        public FontStyle Label1FontStyle { get => _label1FontStyle; set => SetProperty(ref _label1FontStyle, value); }
        public TimeSpan Label1Offset { get => _label1Offset; set => SetProperty(ref _label1Offset, value); }
        public HorizontalAlignment Label1HorizontalAlignment { get => _label1HorizontalAlignment; set => SetProperty(ref _label1HorizontalAlignment, value); }
        public VerticalAlignment Label1VerticalAlignment { get => _label1VerticalAlignment; set => SetProperty(ref _label1VerticalAlignment, value); }
        public WatermarkMode Label1Mode { get => _label1Mode; set => SetProperty(ref _label1Mode, value); }

        // --- Label 2 ---
        public double Label2FontSize { get => _label2FontSize; set => SetProperty(ref _label2FontSize, value); }
        public Brush Label2FontColor { get => _label2FontColor; set => SetProperty(ref _label2FontColor, value); }
        public string Label2FontFamily { get => _label2FontFamily; set => SetProperty(ref _label2FontFamily, value); }
        public FontWeight Label2FontWeight { get => _label2FontWeight; set => SetProperty(ref _label2FontWeight, value); }
        public FontStyle Label2FontStyle { get => _label2FontStyle; set => SetProperty(ref _label2FontStyle, value); }
        public TimeSpan Label2Offset { get => _label2Offset; set => SetProperty(ref _label2Offset, value); }
        public HorizontalAlignment Label2HorizontalAlignment { get => _label2HorizontalAlignment; set => SetProperty(ref _label2HorizontalAlignment, value); }
        public VerticalAlignment Label2VerticalAlignment { get => _label2VerticalAlignment; set => SetProperty(ref _label2VerticalAlignment, value); }
        public WatermarkMode Label2Mode { get => _label2Mode; set => SetProperty(ref _label2Mode, value); }

        // --- Label 3 ---
        public double Label3FontSize { get => _label3FontSize; set => SetProperty(ref _label3FontSize, value); }
        public Brush Label3FontColor { get => _label3FontColor; set => SetProperty(ref _label3FontColor, value); }
        public string Label3FontFamily { get => _label3FontFamily; set => SetProperty(ref _label3FontFamily, value); }
        public FontWeight Label3FontWeight { get => _label3FontWeight; set => SetProperty(ref _label3FontWeight, value); }
        public FontStyle Label3FontStyle { get => _label3FontStyle; set => SetProperty(ref _label3FontStyle, value); }
        public TimeSpan Label3Offset { get => _label3Offset; set => SetProperty(ref _label3Offset, value); }
        public HorizontalAlignment Label3HorizontalAlignment { get => _label3HorizontalAlignment; set => SetProperty(ref _label3HorizontalAlignment, value); }
        public VerticalAlignment Label3VerticalAlignment { get => _label3VerticalAlignment; set => SetProperty(ref _label3VerticalAlignment, value); }
        public WatermarkMode Label3Mode { get => _label3Mode; set => SetProperty(ref _label3Mode, value); }

        // --- Label 4 ---
        public double Label4FontSize { get => _label4FontSize; set => SetProperty(ref _label4FontSize, value); }
        public Brush Label4FontColor { get => _label4FontColor; set => SetProperty(ref _label4FontColor, value); }
        public string Label4FontFamily { get => _label4FontFamily; set => SetProperty(ref _label4FontFamily, value); }
        public FontWeight Label4FontWeight { get => _label4FontWeight; set => SetProperty(ref _label4FontWeight, value); }
        public FontStyle Label4FontStyle { get => _label4FontStyle; set => SetProperty(ref _label4FontStyle, value); }
        public TimeSpan Label4Offset { get => _label4Offset; set => SetProperty(ref _label4Offset, value); }
        public HorizontalAlignment Label4HorizontalAlignment { get => _label4HorizontalAlignment; set => SetProperty(ref _label4HorizontalAlignment, value); }
        public VerticalAlignment Label4VerticalAlignment { get => _label4VerticalAlignment; set => SetProperty(ref _label4VerticalAlignment, value); }
        public WatermarkMode Label4Mode { get => _label4Mode; set => SetProperty(ref _label4Mode, value); }

        // --- Label 5 ---
        public double Label5FontSize { get => _label5FontSize; set => SetProperty(ref _label5FontSize, value); }
        public Brush Label5FontColor { get => _label5FontColor; set => SetProperty(ref _label5FontColor, value); }
        public string Label5FontFamily { get => _label5FontFamily; set => SetProperty(ref _label5FontFamily, value); }
        public FontWeight Label5FontWeight { get => _label5FontWeight; set => SetProperty(ref _label5FontWeight, value); }
        public FontStyle Label5FontStyle { get => _label5FontStyle; set => SetProperty(ref _label5FontStyle, value); }
        public TimeSpan Label5Offset { get => _label5Offset; set => SetProperty(ref _label5Offset, value); }
        public HorizontalAlignment Label5HorizontalAlignment { get => _label5HorizontalAlignment; set => SetProperty(ref _label5HorizontalAlignment, value); }
        public VerticalAlignment Label5VerticalAlignment { get => _label5VerticalAlignment; set => SetProperty(ref _label5VerticalAlignment, value); }
        public WatermarkMode Label5Mode { get => _label5Mode; set => SetProperty(ref _label5Mode, value); }

        // --- Label 6 ---
        public double Label6FontSize { get => _label6FontSize; set => SetProperty(ref _label6FontSize, value); }
        public Brush Label6FontColor { get => _label6FontColor; set => SetProperty(ref _label6FontColor, value); }
        public string Label6FontFamily { get => _label6FontFamily; set => SetProperty(ref _label6FontFamily, value); }
        public FontWeight Label6FontWeight { get => _label6FontWeight; set => SetProperty(ref _label6FontWeight, value); }
        public FontStyle Label6FontStyle { get => _label6FontStyle; set => SetProperty(ref _label6FontStyle, value); }
        public TimeSpan Label6Offset { get => _label6Offset; set => SetProperty(ref _label6Offset, value); }
        public HorizontalAlignment Label6HorizontalAlignment { get => _label6HorizontalAlignment; set => SetProperty(ref _label6HorizontalAlignment, value); }
        public VerticalAlignment Label6VerticalAlignment { get => _label6VerticalAlignment; set => SetProperty(ref _label6VerticalAlignment, value); }
        public WatermarkMode Label6Mode { get => _label6Mode; set => SetProperty(ref _label6Mode, value); }

        // --- Label 7 ---
        public double Label7FontSize { get => _label7FontSize; set => SetProperty(ref _label7FontSize, value); }
        public Brush Label7FontColor { get => _label7FontColor; set => SetProperty(ref _label7FontColor, value); }
        public string Label7FontFamily { get => _label7FontFamily; set => SetProperty(ref _label7FontFamily, value); }
        public FontWeight Label7FontWeight { get => _label7FontWeight; set => SetProperty(ref _label7FontWeight, value); }
        public FontStyle Label7FontStyle { get => _label7FontStyle; set => SetProperty(ref _label7FontStyle, value); }
        public TimeSpan Label7Offset { get => _label7Offset; set => SetProperty(ref _label7Offset, value); }
        public HorizontalAlignment Label7HorizontalAlignment { get => _label7HorizontalAlignment; set => SetProperty(ref _label7HorizontalAlignment, value); }
        public VerticalAlignment Label7VerticalAlignment { get => _label7VerticalAlignment; set => SetProperty(ref _label7VerticalAlignment, value); }
        public WatermarkMode Label7Mode { get => _label7Mode; set => SetProperty(ref _label7Mode, value); }

        // --- Label 8 ---
        public double Label8FontSize { get => _label8FontSize; set => SetProperty(ref _label8FontSize, value); }
        public Brush Label8FontColor { get => _label8FontColor; set => SetProperty(ref _label8FontColor, value); }
        public string Label8FontFamily { get => _label8FontFamily; set => SetProperty(ref _label8FontFamily, value); }
        public FontWeight Label8FontWeight { get => _label8FontWeight; set => SetProperty(ref _label8FontWeight, value); }
        public FontStyle Label8FontStyle { get => _label8FontStyle; set => SetProperty(ref _label8FontStyle, value); }
        public TimeSpan Label8Offset { get => _label8Offset; set => SetProperty(ref _label8Offset, value); }
        public HorizontalAlignment Label8HorizontalAlignment { get => _label8HorizontalAlignment; set => SetProperty(ref _label8HorizontalAlignment, value); }
        public VerticalAlignment Label8VerticalAlignment { get => _label8VerticalAlignment; set => SetProperty(ref _label8VerticalAlignment, value); }
        public WatermarkMode Label8Mode { get => _label8Mode; set => SetProperty(ref _label8Mode, value); }

        // --- Label 9 ---
        public double Label9FontSize { get => _label9FontSize; set => SetProperty(ref _label9FontSize, value); }
        public Brush Label9FontColor { get => _label9FontColor; set => SetProperty(ref _label9FontColor, value); }
        public string Label9FontFamily { get => _label9FontFamily; set => SetProperty(ref _label9FontFamily, value); }
        public FontWeight Label9FontWeight { get => _label9FontWeight; set => SetProperty(ref _label9FontWeight, value); }
        public FontStyle Label9FontStyle { get => _label9FontStyle; set => SetProperty(ref _label9FontStyle, value); }
        public TimeSpan Label9Offset { get => _label9Offset; set => SetProperty(ref _label9Offset, value); }
        public HorizontalAlignment Label9HorizontalAlignment { get => _label9HorizontalAlignment; set => SetProperty(ref _label9HorizontalAlignment, value); }
        public VerticalAlignment Label9VerticalAlignment { get => _label9VerticalAlignment; set => SetProperty(ref _label9VerticalAlignment, value); }
        public WatermarkMode Label9Mode { get => _label9Mode; set => SetProperty(ref _label9Mode, value); }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                             ?? App.MainWindowInstance?.DispatcherQueue;

            if (dispatcher != null && !dispatcher.HasThreadAccess)
            {
                dispatcher.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}