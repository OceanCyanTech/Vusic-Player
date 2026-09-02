using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLib.Matroska;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;



namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class OceanSlider : UserControl
    {
        public OceanSlider()
        {
            this.InitializeComponent();
            //chapt.Add(new ChapterModel { ChapterTitle = "Intro", Starttime = 0 });
            //chapt.Add(new ChapterModel { ChapterTitle = "Exposition", Starttime = 100000000 });
            //chapt.Add(new ChapterModel { ChapterTitle = "Rising Action", Starttime = 800000000 });
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(OceanSlider),
                new PropertyMetadata(Orientation.Horizontal, (d, e) => ((OceanSlider)d).UpdateVisuals()));
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public static readonly DependencyProperty IsSnapToTickEnabledProperty =
    DependencyProperty.Register(nameof(IsSnapToTickEnabled), typeof(bool), typeof(OceanSlider),
        new PropertyMetadata(false));

        public bool IsSnapToTickEnabled
        {
            get => (bool)GetValue(IsSnapToTickEnabledProperty);
            set => SetValue(IsSnapToTickEnabledProperty, value);
        }
        public static readonly DependencyProperty ThumbVisibility =
          DependencyProperty.Register(nameof(IsThumbVisible), typeof(Visibility), typeof(OceanSlider),
              new PropertyMetadata(Visibility.Visible, (d, e) => ((OceanSlider)d).UpdateVisuals()));

        public static readonly DependencyProperty TicksEnabledProperty =
    DependencyProperty.Register(nameof(TicksEnabled), typeof(bool), typeof(OceanSlider),
        new PropertyMetadata(false, (d, e) => ((OceanSlider)d).UpdateTickVisibility()));

        public static readonly DependencyProperty ChaptersEnabledProperty =
  DependencyProperty.Register(nameof(ChaptersEnabledProperty), typeof(bool), typeof(OceanSlider),
      new PropertyMetadata(false, (d, e) => ((OceanSlider)d).UpdateChapterVisibility()));
        private void UpdateTickVisibility()
        {
            TickBar.Visibility = TicksEnabled ? Visibility.Visible : Visibility.Collapsed;
            if (TicksEnabled) UpdateVisuals();
        }
        private void UpdateChapterVisibility()
        {
            ChapterBar.Visibility = ChaptersEnabled ? Visibility.Visible : Visibility.Collapsed;
            if (ChaptersEnabled) UpdateVisuals();
        }
        public bool TicksEnabled
        {
            get => (bool)GetValue(TicksEnabledProperty);
            set => SetValue(TicksEnabledProperty, value);
        }
        public bool ChaptersEnabled
        {
            get => (bool)GetValue(ChaptersEnabledProperty);
            set => SetValue(ChaptersEnabledProperty, value);
        }

        public int TickFrequency { get; set; } = 10; // Number of ticks to show
        public Visibility IsThumbVisible
        {
            get => (Visibility)GetValue(ThumbVisibility);
            set => SetValue(ThumbVisibility, value);
        }
        public double Minimum { get; set; } = 0;
        public static readonly DependencyProperty MaximumProperty =
    DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(OceanSlider),
        new PropertyMetadata(100.0, OnMaximumChanged));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(OceanSlider),
        new PropertyMetadata(0.0, OnValueChanged));

        // 2. The Wrapper (Keep this simple)
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // 3. The Callback (This replaces your 'set' logic)
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OceanSlider control)
            {
                double newValue = (double)e.NewValue;

                // Clamp the value if necessary
                double clamped = Math.Clamp(newValue, control.Minimum, control.Maximum);

                // Avoid infinite loops: only update if clamped value differs from what was set
                if (newValue != clamped)
                {
                    control.Value = clamped;
                    return;
                }

                control.UpdateVisuals();
            }
        }

        public event Action? DragStarted;
        public event Action? DragCompleted;
        public event Action<double>? ValueChanged;
        public event Action<object, double>? ValueChangedWithSender;

        private bool isDragging = false;

        #region Interaction Logic

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isDragging = true;
            VisualStateManager.GoToState(this, "Pressed", true);
            InputLayer.CapturePointer(e.Pointer);
            DragStarted?.Invoke();
            UpdateFromPointer(e);
        }

        private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isDragging) UpdateFromPointer(e);
        }

        private void Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!isDragging) return;
            isDragging = false;
            TimePopup.IsOpen = false; // Hide tooltip
            VisualStateManager.GoToState(this, "Normal", true);
            InputLayer.ReleasePointerCapture(e.Pointer);
            DragCompleted?.Invoke();
        }

        public bool IsTooltipEnabled { get; set; } = true; // Default to true for the time slider

        private void UpdateFromPointer(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(InputLayer);
            double percent;
            double tracklength;

            if (Orientation == Orientation.Horizontal)
            {
                if (ControlRoot.ActualWidth <= 0) return;
                tracklength = ControlRoot.ActualWidth;
                percent = Math.Clamp(point.Position.X / ControlRoot.ActualWidth, 0, 1);
            }
            else
            {
                if (ControlRoot.ActualHeight <= 0) return;
                tracklength = ControlRoot.ActualHeight;
                percent = Math.Clamp(1 - (point.Position.Y / ControlRoot.ActualHeight), 0, 1);
            }
            double totalRange = Maximum - Minimum;
            if (totalRange <= 0) return;
            double calculatedValue = Minimum + (percent * (Maximum - Minimum));
            // Magnetic snap radius in value units (corresponds to ~8 pixels on screen)
            const double snapRadiusPixels = 8.0;
            double snapThresholdValue = (snapRadiusPixels / tracklength) * totalRange;

            double closestSnapTarget = calculatedValue;
            double minDistance = double.MaxValue;
        
            if(chapt != null)
            {
                foreach(var chapter in chapt)
                {
                    double chapterSeconds = TimeSpan.FromTicks(chapter.StartTime).TotalSeconds;
                    double distance = Math.Abs(calculatedValue - chapterSeconds);

                    if (distance < snapThresholdValue && distance < minDistance)
                    {
                        minDistance = distance;
                        closestSnapTarget = chapterSeconds;
                    }
                }
            }
            // MAGNETIC SNAPPING LOGIC
            if (IsSnapToTickEnabled && TickFrequency > 0)
            {
                // In WinUI, TickFrequency is the step interval (e.g., 5s, 10s)
                double nearestTick = Minimum + Math.Round((calculatedValue - Minimum) / TickFrequency) * TickFrequency;
                double distance = Math.Abs(calculatedValue - nearestTick);

                if (distance < snapThresholdValue && distance < minDistance)
                {
                    minDistance = distance;
                    closestSnapTarget = nearestTick;
                }
            }

            // Apply the value once
            Value = Math.Clamp(closestSnapTarget, Minimum, Maximum);

            if (IsTooltipEnabled)
            {
                TimePopup.IsOpen = true;
                if (Orientation == Orientation.Horizontal)
                {
                    TimePopup.HorizontalOffset = point.Position.X - (TimeLabel.ActualWidth / 2);
                    TimePopup.VerticalOffset = -30;
                }
                else
                {
                    TimePopup.HorizontalOffset = 30;
                    TimePopup.VerticalOffset = point.Position.Y - (TimeLabel.ActualHeight / 2);
                }
                TimeLabel.Text = FormatTime(Value);
            }
            else
            {
                TimePopup.IsOpen = false;
            }

            ValueChanged?.Invoke(Value);
            ValueChangedWithSender?.Invoke(this, Value);
        }

        private void InputLayer_PointerEntered(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "PointerOver", true);
        private void InputLayer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!isDragging)
            {
                TimePopup.IsOpen = false; // Hide tooltip
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }
        #endregion

        #region Visual Engine

        private void ControlRoot_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateVisuals();

        private void UpdateVisuals()
        {
            if (ControlRoot.ActualWidth <= 0 || ControlRoot.ActualHeight <= 0) return;

            double percent = (Maximum > Minimum) ? (Value - Minimum) / (Maximum - Minimum) : 0;
            Thumb.Visibility = IsThumbVisible;
            if (Orientation == Orientation.Horizontal)
            {
                // Horizontal Layout Logic
                Track.Width = double.NaN;
                Track.Height = 6;
                Track.HorizontalAlignment = HorizontalAlignment.Stretch;
                Track.VerticalAlignment = VerticalAlignment.Center;

                Progress.Height = 6;
                Progress.VerticalAlignment = VerticalAlignment.Center;
                Progress.HorizontalAlignment = HorizontalAlignment.Left;

                double xPos = percent * ControlRoot.ActualWidth;
                Progress.Width = xPos;
                if (IsThumbVisible == Visibility.Visible)
                {
                    Canvas.SetLeft(Thumb, xPos - (Thumb.ActualWidth / 2));
                    Canvas.SetTop(Thumb, (ControlRoot.ActualHeight - Thumb.ActualHeight) / 2);
                }
            }
            else
            {
                // Vertical Layout Logic
                Track.Height = double.NaN;
                Track.Width = 6;
                Track.VerticalAlignment = VerticalAlignment.Stretch;
                Track.HorizontalAlignment = HorizontalAlignment.Center;

                Progress.Width = 6;
                Progress.HorizontalAlignment = HorizontalAlignment.Center;
                Progress.VerticalAlignment = VerticalAlignment.Bottom;

                double yPos = (1 - percent) * ControlRoot.ActualHeight;
                Progress.Height = Math.Max(0, ControlRoot.ActualHeight - yPos);

                if (IsThumbVisible == Visibility.Visible)
                {
                    Canvas.SetTop(Thumb, yPos - (Thumb.ActualHeight / 2));
                    Canvas.SetLeft(Thumb, (ControlRoot.ActualWidth - Thumb.ActualWidth) / 2);
                }
            }
            if (ChaptersEnabled)
            {
                DrawChapters();
            }
            if (TicksEnabled) DrawTicks();
        }
        ObservableCollection<ChapterModel> chapt = new ObservableCollection<ChapterModel>();
        private void DrawChapters()
        {
            Debug.WriteLine("Draw Chapters Called");
            ChapterBar.Items.Clear();

            // Guard against running before the layout pass finishes
            if (ControlRoot.ActualWidth <= 0 || Maximum <= 0)
                return;

            const double markerWidth = 3.5;
            const double markerHeight = 17.0;

            if (PlayerService.Masterplayer == null) return;
            foreach (var chapter in PlayerService.Masterplayer.Chapters)
            {
                var title = chapter.Title;
                var starttime = chapter.StartTime;

                double totalSeconds = TimeSpan.FromTicks(starttime).TotalSeconds;

                // Ratio between 0.0 and 1.0
                double ratio = Math.Clamp(totalSeconds / Maximum, 0.0, 1.0);
                double xPos = ratio * ControlRoot.ActualWidth;

                var chapteritem = new Microsoft.UI.Xaml.Shapes.Rectangle()
                {
                    Width = markerWidth,
                    Height = markerHeight,
                    Fill = new SolidColorBrush(Microsoft.UI.Colors.Cyan),
                  
                   
                };
                chapteritem.Tapped += ((object sender, TappedRoutedEventArgs e) =>
                {
                    if (PlayerService.Masterplayer != null)
                    {
                        var timespanstart = TimeSpan.FromTicks(starttime);
                        var targetTime = TimeSpan.FromTicks(starttime);
                        string start = timespanstart.TotalHours >= 1
                    ? $"{(int)timespanstart.TotalHours:D2}:{timespanstart.Minutes:D2}:{timespanstart.Seconds:D2}"
                    : $"{timespanstart.Minutes:D2}:{timespanstart.Seconds:D2}";
                        PlayerService.Masterplayer.SeekAccurate((int)targetTime.TotalMilliseconds);
                        var curTime = TimeSpan.FromTicks(starttime);
                        GeneralInfoService.ShowInfo($"Jumped to {chapter.Title} at {start}");
                        mediacontroller.CurrentPosition = curTime.TotalSeconds;
                    }
                });
                ToolTipService.SetToolTip(chapteritem, title);

                // Center the marker on the chapter position
                Canvas.SetLeft(chapteritem, xPos - (markerWidth / 2));
                Canvas.SetTop(chapteritem, (ControlRoot.ActualHeight - markerHeight) / 2);

                ChapterBar.Items.Add(chapteritem);
            }
        }
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        ObservableCollection<ChapterModel> Chapters = new ObservableCollection<ChapterModel>();
        private void DrawTicks()
        {
            TickBar.Items.Clear();
            if (TickFrequency <= 1) return;

            // Use a small margin so the ticks don't touch the absolute edge
            double edgeMargin = 2;
            double availableWidth = ControlRoot.ActualWidth - (edgeMargin * 2);
            double availableHeight = ControlRoot.ActualHeight - (edgeMargin * 2);

            for (int i = 0; i <= TickFrequency; i++)
            {
                var tick = new Microsoft.UI.Xaml.Shapes.Rectangle()
                {
                    Fill = new SolidColorBrush(Microsoft.UI.Colors.Gray)
                };

                double relativePos = (double)i / TickFrequency;

                if (Orientation == Orientation.Horizontal)
                {
                    tick.Width = 1;
                    tick.Height = 8;
                    // Calculate X and subtract half the tick width (0.5) to truly center it
                    double xPos = edgeMargin + (relativePos * availableWidth) - 0.5;

                    Canvas.SetLeft(tick, xPos);
                    Canvas.SetTop(tick, (ControlRoot.ActualHeight / 2) + 6);
                }
                else
                {
                    tick.Width = 8;
                    tick.Height = 1;
                    // Invert for vertical: 0 is bottom, adjust for margin
                    double yPos = edgeMargin + ((1 - relativePos) * availableHeight) - 0.5;

                    Canvas.SetTop(tick, yPos);
                    Canvas.SetLeft(tick, (ControlRoot.ActualWidth / 2) - 12);
                }

                TickBar.Items.Add(tick);
            }
        }
        private string FormatTime(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return t.TotalHours >= 1
                ? t.ToString(@"hh\:mm\:ss")
                : t.ToString(@"mm\:ss");
        }
        public void SetValueFromPlayer(double value)
        {
            if (!isDragging) Value = value;
        }

        #endregion
    }
}
