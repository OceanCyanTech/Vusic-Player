using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Vusic_Player;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Windows.UI;
using Windows.UI.Text;

namespace Vusic_Player.Configuration.Playback
{
    public static class WatermarkSerializationHelpers
    {
        public static string ToHex(this Brush? brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return $"#{scb.Color.A:X2}{scb.Color.R:X2}{scb.Color.G:X2}{scb.Color.B:X2}";
            }
            return "#FFFFFFFF";
        }

        public static SolidColorBrush FromHex(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return new SolidColorBrush(Colors.White);

            hex = hex.Trim().TrimStart('#');

            try
            {
                byte a = 255;
                byte r, g, b;

                if (hex.Length == 8) // AARRGGBB
                {
                    a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                else if (hex.Length == 6) // RRGGBB
                {
                    r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }

                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
            catch
            {
                return new SolidColorBrush(Colors.White);
            }
        }
    }
    public enum WatermarkMode
    {
        ElapsedPlaybackTime,        // Elapsed Time of Video
        RemainingPlaybackTime,      // Remaining Time of Video
        CurrentVsTotalDuration,     // Current Time vs Total Duration
        CurrentSystemTime,          // Current System Time
        CustomInitialOffset,        // Custom Initial Offset
        RunningFrames,              // Running Frames (Frame Rate to be Selected)
        MusicalTimecode,// Musical Timecode (Bars and Beats)
        CustomStaticWatermark
    }
    public class WatermarkFontValues : INotifyPropertyChanged
    {
        public static WatermarkFontValues Instance { get; } = new WatermarkFontValues();

        #region Backing Fields
        // Label 1
        private Visibility _label1IsVisible = Visibility.Collapsed;

        // Label 2
        private Visibility _label2IsVisible = Visibility.Collapsed;

        // Label 3
        private Visibility _label3IsVisible = Visibility.Collapsed;

        // Label 4
        private Visibility _label4IsVisible = Visibility.Collapsed;

        // Label 5
        private Visibility _label5IsVisible = Visibility.Collapsed;

        // Label 6
        private Visibility _label6IsVisible = Visibility.Collapsed;

        // Label 7
        private Visibility _label7IsVisible = Visibility.Collapsed;

        // Label 8
        private Visibility _label8IsVisible = Visibility.Collapsed;

        // Label 9
        private Visibility _label9IsVisible = Visibility.Collapsed;
        private Visibility _overallVisible = Visibility.Collapsed;
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
        private ObservableCollection<TimestampWatermark> _timestamplabels = new ObservableCollection<TimestampWatermark>();
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
        public Visibility Label1IsVisible { get => _label1IsVisible; set => SetProperty(ref _label1IsVisible, value); }
        public Visibility Label2IsVisible { get => _label2IsVisible; set => SetProperty(ref _label2IsVisible, value); }
        public Visibility Label3IsVisible { get => _label3IsVisible; set => SetProperty(ref _label3IsVisible, value); }
        public Visibility Label4IsVisible { get => _label4IsVisible; set => SetProperty(ref _label4IsVisible, value); }
        public Visibility Label5IsVisible { get => _label5IsVisible; set => SetProperty(ref _label5IsVisible, value); }
        public Visibility Label6IsVisible { get => _label6IsVisible; set => SetProperty(ref _label6IsVisible, value); }
        public Visibility Label7IsVisible { get => _label7IsVisible; set => SetProperty(ref _label7IsVisible, value); }
        public Visibility Label8IsVisible { get => _label8IsVisible; set => SetProperty(ref _label8IsVisible, value); }
        public Visibility Label9IsVisible { get => _label9IsVisible; set => SetProperty(ref _label9IsVisible, value); }
        public Visibility OverallVisibility { get => _overallVisible; set => SetProperty(ref _overallVisible, value); }
        // --- Label 9 ---
        public double Label9FontSize { get => _label9FontSize; set => SetProperty(ref _label9FontSize, value); }
        public Brush Label9FontColor { get => _label9FontColor; set => SetProperty(ref _label9FontColor, value); }
        public string Label9FontFamily { get => _label9FontFamily; set => SetProperty(ref _label9FontFamily, value); }
        public FontWeight Label9FontWeight { get => _label9FontWeight; set => SetProperty(ref _label9FontWeight, value); }
        public FontStyle Label9FontStyle { get => _label9FontStyle; set => SetProperty(ref _label9FontStyle, value); }
        public TimeSpan Label9Offset { get => _label9Offset; set => SetProperty(ref _label9Offset, value); }
        public HorizontalAlignment Label9HorizontalAlignment { get => _label9HorizontalAlignment; set => SetProperty(ref _label9HorizontalAlignment, value); }
        public VerticalAlignment Label9VerticalAlignment { get => _label9VerticalAlignment; set => SetProperty(ref _label9VerticalAlignment, value); }
        public ObservableCollection<TimestampWatermark> TimestampWatermarksLabels { get => _timestamplabels; set => SetProperty(ref _timestamplabels, value); }
        public WatermarkMode Label9Mode { get => _label9Mode; set => SetProperty(ref _label9Mode, value); }

        #endregion
        public WatermarkSettingsModel ToSettingsModel()
        {
            return new WatermarkSettingsModel
            {
                Label1 = new()
                {
                    IsVisible = Label1IsVisible,
                    FontSize = Label1FontSize,
                    HexColor = Label1FontColor.ToHex(),
                    FontFamily = Label1FontFamily,
                    FontWeight = Label1FontWeight.Weight,
                    FontStyle = Label1FontStyle,
                    Offset = Label1Offset,
                    HorizontalAlignment = Label1HorizontalAlignment,
                    VerticalAlignment = Label1VerticalAlignment,
                    Mode = Label1Mode
                },
                Label2 = new()
                {
                    IsVisible = Label2IsVisible,
                    FontSize = Label2FontSize,
                    HexColor = Label2FontColor.ToHex(),
                    FontFamily = Label2FontFamily,
                    FontWeight = Label2FontWeight.Weight,
                    FontStyle = Label2FontStyle,
                    Offset = Label2Offset,
                    HorizontalAlignment = Label2HorizontalAlignment,
                    VerticalAlignment = Label2VerticalAlignment,
                    Mode = Label2Mode
                },
                Label3 = new()
                {
                    IsVisible = Label3IsVisible,
                    FontSize = Label3FontSize,
                    HexColor = Label3FontColor.ToHex(),
                    FontFamily = Label3FontFamily,
                    FontWeight = Label3FontWeight.Weight,
                    FontStyle = Label3FontStyle,
                    Offset = Label3Offset,
                    HorizontalAlignment = Label3HorizontalAlignment,
                    VerticalAlignment = Label3VerticalAlignment,
                    Mode = Label3Mode
                },
                Label4 = new()
                {
                    IsVisible = Label4IsVisible,
                    FontSize = Label4FontSize,
                    HexColor = Label4FontColor.ToHex(),
                    FontFamily = Label4FontFamily,
                    FontWeight = Label4FontWeight.Weight,
                    FontStyle = Label4FontStyle,
                    Offset = Label4Offset,
                    HorizontalAlignment = Label4HorizontalAlignment,
                    VerticalAlignment = Label4VerticalAlignment,
                    Mode = Label4Mode
                },
                Label5 = new()
                {
                    IsVisible = Label5IsVisible,
                    FontSize = Label5FontSize,
                    HexColor = Label5FontColor.ToHex(),
                    FontFamily = Label5FontFamily,
                    FontWeight = Label5FontWeight.Weight,
                    FontStyle = Label5FontStyle,
                    Offset = Label5Offset,
                    HorizontalAlignment = Label5HorizontalAlignment,
                    VerticalAlignment = Label5VerticalAlignment,
                    Mode = Label5Mode
                },
                Label6 = new()
                {
                    IsVisible = Label6IsVisible,
                    FontSize = Label6FontSize,
                    HexColor = Label6FontColor.ToHex(),
                    FontFamily = Label6FontFamily,
                    FontWeight = Label6FontWeight.Weight,
                    FontStyle = Label6FontStyle,
                    Offset = Label6Offset,
                    HorizontalAlignment = Label6HorizontalAlignment,
                    VerticalAlignment = Label6VerticalAlignment,
                    Mode = Label6Mode
                },
                Label7 = new()
                {
                    IsVisible = Label7IsVisible,
                    FontSize = Label7FontSize,
                    HexColor = Label7FontColor.ToHex(),
                    FontFamily = Label7FontFamily,
                    FontWeight = Label7FontWeight.Weight,
                    FontStyle = Label7FontStyle,
                    Offset = Label7Offset,
                    HorizontalAlignment = Label7HorizontalAlignment,
                    VerticalAlignment = Label7VerticalAlignment,
                    Mode = Label7Mode
                },
                Label8 = new()
                {
                    IsVisible = Label8IsVisible,
                    FontSize = Label8FontSize,
                    HexColor = Label8FontColor.ToHex(),
                    FontFamily = Label8FontFamily,
                    FontWeight = Label8FontWeight.Weight,
                    FontStyle = Label8FontStyle,
                    Offset = Label8Offset,
                    HorizontalAlignment = Label8HorizontalAlignment,
                    VerticalAlignment = Label8VerticalAlignment,
                    Mode = Label8Mode
                },
                Label9 = new()
                {
                    IsVisible = Label9IsVisible,
                    FontSize = Label9FontSize,
                    HexColor = Label9FontColor.ToHex(),
                    FontFamily = Label9FontFamily,
                    FontWeight = Label9FontWeight.Weight,
                    FontStyle = Label9FontStyle,
                    Offset = Label9Offset,
                    HorizontalAlignment = Label9HorizontalAlignment,
                    VerticalAlignment = Label9VerticalAlignment,
                    Mode = Label9Mode
                }
            };
        }
        public void LoadFromSettingsModel(WatermarkSettingsModel model)
        {
            if (model == null) return;

            void Apply(WatermarkLabelSettingsModel source,
                       Action<Visibility> setVisible,
                       Action<double> setSize, Action<Brush> setColor,
                       Action<string> setFont, Action<FontWeight> setWeight, Action<FontStyle> setStyle,
                       Action<TimeSpan> setOffset, Action<HorizontalAlignment> setHAlign,
                       Action<VerticalAlignment> setVAlign, Action<WatermarkMode> setMode)
            {
                if (source == null) return;
                setVisible(source.IsVisible);
                setSize(source.FontSize);
                setColor(WatermarkSerializationHelpers.FromHex(source.HexColor));
                setFont(source.FontFamily);
                setWeight(new FontWeight { Weight = source.FontWeight });
                setStyle(source.FontStyle);
                setOffset(source.Offset);
                setHAlign(source.HorizontalAlignment);
                setVAlign(source.VerticalAlignment);
                setMode(source.Mode);
            }

            Apply(model.Label1, v => Label1IsVisible = v, v => Label1FontSize = v, v => Label1FontColor = v, v => Label1FontFamily = v, v => Label1FontWeight = v, v => Label1FontStyle = v, v => Label1Offset = v, v => Label1HorizontalAlignment = v, v => Label1VerticalAlignment = v, v => Label1Mode = v);
            Apply(model.Label2, v => Label2IsVisible = v, v => Label2FontSize = v, v => Label2FontColor = v, v => Label2FontFamily = v, v => Label2FontWeight = v, v => Label2FontStyle = v, v => Label2Offset = v, v => Label2HorizontalAlignment = v, v => Label2VerticalAlignment = v, v => Label2Mode = v);
            Apply(model.Label3, v => Label3IsVisible = v, v => Label3FontSize = v, v => Label3FontColor = v, v => Label3FontFamily = v, v => Label3FontWeight = v, v => Label3FontStyle = v, v => Label3Offset = v, v => Label3HorizontalAlignment = v, v => Label3VerticalAlignment = v, v => Label3Mode = v);
            Apply(model.Label4, v => Label4IsVisible = v, v => Label4FontSize = v, v => Label4FontColor = v, v => Label4FontFamily = v, v => Label4FontWeight = v, v => Label4FontStyle = v, v => Label4Offset = v, v => Label4HorizontalAlignment = v, v => Label4VerticalAlignment = v, v => Label4Mode = v);
            Apply(model.Label5, v => Label5IsVisible = v, v => Label5FontSize = v, v => Label5FontColor = v, v => Label5FontFamily = v, v => Label5FontWeight = v, v => Label5FontStyle = v, v => Label5Offset = v, v => Label5HorizontalAlignment = v, v => Label5VerticalAlignment = v, v => Label5Mode = v);
            Apply(model.Label6, v => Label6IsVisible = v, v => Label6FontSize = v, v => Label6FontColor = v, v => Label6FontFamily = v, v => Label6FontWeight = v, v => Label6FontStyle = v, v => Label6Offset = v, v => Label6HorizontalAlignment = v, v => Label6VerticalAlignment = v, v => Label6Mode = v);
            Apply(model.Label7, v => Label7IsVisible = v, v => Label7FontSize = v, v => Label7FontColor = v, v => Label7FontFamily = v, v => Label7FontWeight = v, v => Label7FontStyle = v, v => Label7Offset = v, v => Label7HorizontalAlignment = v, v => Label7VerticalAlignment = v, v => Label7Mode = v);
            Apply(model.Label8, v => Label8IsVisible = v, v => Label8FontSize = v, v => Label8FontColor = v, v => Label8FontFamily = v, v => Label8FontWeight = v, v => Label8FontStyle = v, v => Label8Offset = v, v => Label8HorizontalAlignment = v, v => Label8VerticalAlignment = v, v => Label8Mode = v);
            Apply(model.Label9, v => Label9IsVisible = v, v => Label9FontSize = v, v => Label9FontColor = v, v => Label9FontFamily = v, v => Label9FontWeight = v, v => Label9FontStyle = v, v => Label9Offset = v, v => Label9HorizontalAlignment = v, v => Label9VerticalAlignment = v, v => Label9Mode = v);
        }
        private static System.Timers.Timer? _debounceTimer;

        public WatermarkFontValues()
        {
            PropertyChanged += OnWatermarkPropertyChanged;
        }

        private void OnWatermarkPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Ignore internal or non-saving notifications if any
            TriggerDebouncedSave();
        }

        private void TriggerDebouncedSave()
        {
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            // Wait 500ms after the last property change before writing to disk
            _debounceTimer = new System.Timers.Timer(500) { AutoReset = false };
            _debounceTimer.Elapsed += async (_, _) =>
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                if (currentSettings.IsSavingWatermarkValues)
                {
                    currentSettings.Watermark = Instance.ToSettingsModel();
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                }
            };
            _debounceTimer.Start();
        }
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