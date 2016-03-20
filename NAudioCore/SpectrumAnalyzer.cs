using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Kova.NAudioCore
{
    /// <summary>
    /// A spectrum analyzer control for visualizing audio data.
    /// </summary>
    [TemplatePart(Name = "PART_SpectrumCanvas", Type = typeof(Canvas))]
    public class SpectrumAnalyzer : Control
    {
        private readonly DispatcherTimer _animationTimer;
        private Canvas _spectrumCanvas;
        private ISpectrumPlayer _soundPlayer;
        private readonly List<Shape> _barShapes = new List<Shape>();
        private readonly List<Shape> _peakShapes = new List<Shape>();
        private double[] _barHeights;
        private double[] _peakHeights;
        private float[] _channelData = new float[2048];
        private float[] _channelPeakData;
        private double _bandWidth = 1.0;
        private double _barWidth = 1;
        private int _maximumFrequencyIndex = 2047;
        private int _minimumFrequencyIndex;
        private int[] _barIndexMax;
        private int[] _barLogScaleIndexMax;

        private const int _scaleFactorLinear = 9;
        private const int _scaleFactorSqr = 2;
        private const double _minDBValue = -90;
        private const double _maxDBValue = 0;
        private const double _dbScale = (_maxDBValue - _minDBValue);

        public static readonly DependencyProperty MaximumFrequencyProperty = DependencyProperty.Register("MaximumFrequency", typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(20000, OnMaximumFrequencyChanged, OnCoerceMaximumFrequency));

        private static object OnCoerceMaximumFrequency(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceMaximumFrequency((int)value);
            else
                return value;
        }

        private static void OnMaximumFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnMaximumFrequencyChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual int OnCoerceMaximumFrequency(int value)
        {
            if ((int)value < MinimumFrequency)
                return MinimumFrequency + 1;
            return value;
        }

        protected virtual void OnMaximumFrequencyChanged(int oldValue, int newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// The maximum display frequency (right side) for the spectrum analyzer.
        /// </summary>
        /// <remarks>In usual practice, this value should be somewhere between 0 and half of the maximum sample rate. If using
        /// the maximum sample rate, this would be roughly 22000.</remarks>
        [Category("Common")]
        public int MaximumFrequency
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (int)GetValue(MaximumFrequencyProperty);
            }
            set
            {
                SetValue(MaximumFrequencyProperty, value);
            }
        }

        public static readonly DependencyProperty MinimumFrequencyProperty = DependencyProperty.Register("MinimumFrequency", typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(20, OnMinimumFrequencyChanged, OnCoerceMinimumFrequency));

        private static object OnCoerceMinimumFrequency(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceMinimumFrequency((int)value);
            else
                return value;
        }

        private static void OnMinimumFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnMinimumFrequencyChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual int OnCoerceMinimumFrequency(int value)
        {
            if (value < 0)
                return value = 0;
            CoerceValue(MaximumFrequencyProperty);
            return value;
        }

        protected virtual void OnMinimumFrequencyChanged(int oldValue, int newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// The minimum display frequency (left side) for the spectrum analyzer.
        /// </summary>
        [Category("Common")]
        public int MinimumFrequency
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (int)GetValue(MinimumFrequencyProperty);
            }
            set
            {
                SetValue(MinimumFrequencyProperty, value);
            }
        }

        public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register("BarCount", typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(32, OnBarCountChanged, OnCoerceBarCount));

        private static object OnCoerceBarCount(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceBarCount((int)value);
            else
                return value;
        }

        private static void OnBarCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnBarCountChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual int OnCoerceBarCount(int value)
        {
            value = Math.Max(value, 1);
            return value;
        }

        protected virtual void OnBarCountChanged(int oldValue, int newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// The number of bars to show on the sprectrum analyzer.
        /// </summary>
        /// <remarks>A bar's width can be a minimum of 1 pixel. If the BarSpacing and BarCount property result
        /// in the bars being wider than the chart itself, the BarCount will automatically scale down.</remarks>
        [Category("Common")]
        public int BarCount
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (int)GetValue(BarCountProperty);
            }
            set
            {
                SetValue(BarCountProperty, value);
            }
        }

        public static readonly DependencyProperty BarSpacingProperty = DependencyProperty.Register("BarSpacing", typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(5.0d, OnBarSpacingChanged, OnCoerceBarSpacing));

        private static object OnCoerceBarSpacing(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceBarSpacing((double)value);
            else
                return value;
        }

        private static void OnBarSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnBarSpacingChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceBarSpacing(double value)
        {
            value = Math.Max(value, 0);
            return value;
        }

        protected virtual void OnBarSpacingChanged(double oldValue, double newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// The spacing, in pixels, between the bars.
        /// </summary>
        [Category("Common")]
        public double BarSpacing
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (double)GetValue(BarSpacingProperty);
            }
            set
            {
                SetValue(BarSpacingProperty, value);
            }
        }

        public static readonly DependencyProperty PeakFallDelayProperty = DependencyProperty.Register("PeakFallDelay", typeof(int), typeof(SpectrumAnalyzer), new UIPropertyMetadata(10, OnPeakFallDelayChanged, OnCoercePeakFallDelay));

        private static object OnCoercePeakFallDelay(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoercePeakFallDelay((int)value);
            else
                return value;
        }

        private static void OnPeakFallDelayChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnPeakFallDelayChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual int OnCoercePeakFallDelay(int value)
        {
            value = Math.Max(value, 0);
            return value;
        }

        protected virtual void OnPeakFallDelayChanged(int oldValue, int newValue)
        {

        }

        /// <summary>
        /// The delay factor for the peaks falling. This is relative to the
        /// refresh rate of the chart.
        /// </summary>
        [Category("Common")]
        public int PeakFallDelay
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (int)GetValue(PeakFallDelayProperty);
            }
            set
            {
                SetValue(PeakFallDelayProperty, value);
            }
        }

        public static readonly DependencyProperty IsFrequencyScaleLinearProperty = DependencyProperty.Register("IsFrequencyScaleLinear", typeof(bool), typeof(SpectrumAnalyzer), new UIPropertyMetadata(false, OnIsFrequencyScaleLinearChanged, OnCoerceIsFrequencyScaleLinear));

        private static object OnCoerceIsFrequencyScaleLinear(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceIsFrequencyScaleLinear((bool)value);
            else
                return value;
        }

        private static void OnIsFrequencyScaleLinearChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnIsFrequencyScaleLinearChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual bool OnCoerceIsFrequencyScaleLinear(bool value)
        {
            return value;
        }

        protected virtual void OnIsFrequencyScaleLinearChanged(bool oldValue, bool newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// If true, the bars will represent frequency buckets on a linear scale (making them all
        /// have equal band widths on the frequency scale). Otherwise, the bars will be layed out
        /// on a logrithmic scale, with each bar having a larger bandwidth than the one previous.
        /// </summary>
        [Category("Common")]
        public bool IsFrequencyScaleLinear
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (bool)GetValue(IsFrequencyScaleLinearProperty);
            }
            set
            {
                SetValue(IsFrequencyScaleLinearProperty, value);
            }
        }

        public static readonly DependencyProperty BarHeightScalingProperty = DependencyProperty.Register("BarHeightScaling", typeof(BarHeightScalingStyles), typeof(SpectrumAnalyzer), new UIPropertyMetadata(BarHeightScalingStyles.Decibel, OnBarHeightScalingChanged, OnCoerceBarHeightScaling));

        private static object OnCoerceBarHeightScaling(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceBarHeightScaling((BarHeightScalingStyles)value);
            else
                return value;
        }

        private static void OnBarHeightScalingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnBarHeightScalingChanged((BarHeightScalingStyles)e.OldValue, (BarHeightScalingStyles)e.NewValue);
        }

        protected virtual BarHeightScalingStyles OnCoerceBarHeightScaling(BarHeightScalingStyles value)
        {
            return value;
        }

        protected virtual void OnBarHeightScalingChanged(BarHeightScalingStyles oldValue, BarHeightScalingStyles newValue)
        {

        }

        /// <summary>
        /// If true, the bar height will be displayed linearly with the intensity value.
        /// Otherwise, the bars will be scaled with a square root function.
        /// </summary>
        [Category("Common")]
        public BarHeightScalingStyles BarHeightScaling
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (BarHeightScalingStyles)GetValue(BarHeightScalingProperty);
            }
            set
            {
                SetValue(BarHeightScalingProperty, value);
            }
        }

        public static readonly DependencyProperty AveragePeaksProperty = DependencyProperty.Register("AveragePeaks", typeof(bool), typeof(SpectrumAnalyzer), new UIPropertyMetadata(false, OnAveragePeaksChanged, OnCoerceAveragePeaks));

        private static object OnCoerceAveragePeaks(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceAveragePeaks((bool)value);
            else
                return value;
        }

        private static void OnAveragePeaksChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnAveragePeaksChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual bool OnCoerceAveragePeaks(bool value)
        {
            return value;
        }

        protected virtual void OnAveragePeaksChanged(bool oldValue, bool newValue)
        {

        }

        /// <summary>
        /// If true, each bar's peak value will be averaged with the previous
        /// bar's peak. This creates a smoothing effect on the bars.
        /// </summary>
        [Category("Common")]
        public bool AveragePeaks
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (bool)GetValue(AveragePeaksProperty);
            }
            set
            {
                SetValue(AveragePeaksProperty, value);
            }
        }

        public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register("BarStyle", typeof(Style), typeof(SpectrumAnalyzer), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBarStyleChanged), new CoerceValueCallback(OnCoerceBarStyle)));

        private static object OnCoerceBarStyle(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceBarStyle((Style)value);
            else
                return value;
        }

        private static void OnBarStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnBarStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        protected virtual Style OnCoerceBarStyle(Style value)
        {
            return value;
        }

        protected virtual void OnBarStyleChanged(Style oldValue, Style newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// A style with which to draw the bars on the spectrum analyzer.
        /// </summary>
        public Style BarStyle
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (Style)GetValue(BarStyleProperty);
            }
            set
            {
                SetValue(BarStyleProperty, value);
            }
        }

        public static readonly DependencyProperty PeakStyleProperty = DependencyProperty.Register("PeakStyle", typeof(Style), typeof(SpectrumAnalyzer), new UIPropertyMetadata(null, new PropertyChangedCallback(OnPeakStyleChanged), new CoerceValueCallback(OnCoercePeakStyle)));

        private static object OnCoercePeakStyle(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoercePeakStyle((Style)value);
            else
                return value;
        }

        private static void OnPeakStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnPeakStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        protected virtual Style OnCoercePeakStyle(Style value)
        {

            return value;
        }

        protected virtual void OnPeakStyleChanged(Style oldValue, Style newValue)
        {
            UpdateBarLayout();
        }

        /// <summary>
        /// A style with which to draw the falling peaks on the spectrum analyzer.
        /// </summary>
        public Style PeakStyle
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (Style)GetValue(PeakStyleProperty);
            }
            set
            {
                SetValue(PeakStyleProperty, value);
            }
        }

        public static readonly DependencyProperty ActualBarWidthProperty = DependencyProperty.Register("ActualBarWidth", typeof(double), typeof(SpectrumAnalyzer), new UIPropertyMetadata(0.0d, new PropertyChangedCallback(OnActualBarWidthChanged), new CoerceValueCallback(OnCoerceActualBarWidth)));

        private static object OnCoerceActualBarWidth(DependencyObject o, object value)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                return spectrumAnalyzer.OnCoerceActualBarWidth((double)value);
            else
                return value;
        }

        private static void OnActualBarWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SpectrumAnalyzer spectrumAnalyzer = o as SpectrumAnalyzer;
            if (spectrumAnalyzer != null)
                spectrumAnalyzer.OnActualBarWidthChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceActualBarWidth(double value)
        {
            return value;
        }

        protected virtual void OnActualBarWidthChanged(double oldValue, double newValue)
        {

        }

        /// <summary>
        /// The actual width that the bars will be drawn at.
        /// </summary>
        public double ActualBarWidth
        {
            // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
            get
            {
                return (double)GetValue(ActualBarWidthProperty);
            }
            protected set
            {
                SetValue(ActualBarWidthProperty, value);
            }
        }

        /// <summary>
        /// The different ways that the bar height can be scaled by the spectrum analyzer.
        /// </summary>
        public enum BarHeightScalingStyles
        {
            Decibel,

            Sqrt,

            Linear
        }

        /// <summary>
        /// The styles that the spectrum analyzer can draw the bars.
        /// </summary>
        public enum BarDrawingStyles
        {
            Square,

            Rounded
        }

        public override void OnApplyTemplate()
        {
            _spectrumCanvas = GetTemplateChild("PART_SpectrumCanvas") as Canvas;
            UpdateBarLayout();
        }

        static SpectrumAnalyzer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumAnalyzer), new FrameworkPropertyMetadata(typeof(SpectrumAnalyzer)));
        }

        public SpectrumAnalyzer()
        {
            _animationTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = TimeSpan.FromMilliseconds(25),
            };
            _animationTimer.Tick += animationTimer_Tick;
        }

        /// <summary>
        /// Register a sound player from which the spectrum analyzer
        /// can get the necessary playback data.
        /// </summary>
        /// <param name="soundPlayer">A sound player that provides spectrum data through the ISpectrumPlayer interface methods.</param>
        public void RegisterSoundPlayer(ISpectrumPlayer soundPlayer)
        {
            this._soundPlayer = soundPlayer;
            soundPlayer.PropertyChanged += soundPlayer_PropertyChanged;
            UpdateBarLayout();
            _animationTimer.Start();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            UpdateBarLayout();
            UpdateSpectrum();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateBarLayout();
            UpdateSpectrum();
        }

        private void UpdateSpectrum()
        {
            if (_soundPlayer == null || _spectrumCanvas == null || _spectrumCanvas.RenderSize.Width < 1 || _spectrumCanvas.RenderSize.Height < 1)
                return;

            if (_soundPlayer.IsPlaying && (_soundPlayer.GetFFTData1(_channelData)))
                return;

            UpdateSpectrumShapes();
        }


        private void UpdateSpectrumShapes()
        {
            bool allZero = true;
            double fftBucketHeight = 0f;
            double barHeight = 0f;
            double lastPeakHeight = 0f;
            double peakYPos = 0f;
            double height = _spectrumCanvas.RenderSize.Height;
            int barIndex = 0;
            double peakDotHeight = Math.Max(_barWidth / 2.0f, 1);
            double barHeightScale = (height - peakDotHeight);

            for (int i = _minimumFrequencyIndex; i <= _maximumFrequencyIndex; i++)
            {
                // If we're paused, keep drawing, but set the current height to 0 so the peaks fall.
                if (!_soundPlayer.IsPlaying)
                {
                    barHeight = 0f;
                }
                else // Draw the maximum value for the bar's band
                {
                    switch (BarHeightScaling)
                    {
                        case BarHeightScalingStyles.Decibel:
                            double dbValue = 20 * Math.Log10((double)_channelData[i]);
                            fftBucketHeight = ((dbValue - _minDBValue) / _dbScale) * barHeightScale;
                            break;
                        case BarHeightScalingStyles.Linear:
                            fftBucketHeight = (_channelData[i] * _scaleFactorLinear) * barHeightScale;
                            break;
                        case BarHeightScalingStyles.Sqrt:
                            fftBucketHeight = (((Math.Sqrt((double)_channelData[i])) * _scaleFactorSqr) * barHeightScale);
                            break;
                    }

                    if (barHeight < fftBucketHeight)
                        barHeight = fftBucketHeight;
                    if (barHeight < 0f)
                        barHeight = 0f;
                }

                // If this is the last FFT bucket in the bar's group, draw the bar.
                int currentIndexMax = IsFrequencyScaleLinear ? _barIndexMax[barIndex] : _barLogScaleIndexMax[barIndex];
                if (i == currentIndexMax)
                {
                    // Peaks can't surpass the height of the control.
                    if (barHeight > height)
                        barHeight = height;

                    if (AveragePeaks && barIndex > 0)
                        barHeight = (lastPeakHeight + barHeight) / 2;

                    peakYPos = barHeight;

                    if (_channelPeakData[barIndex] < peakYPos)
                        _channelPeakData[barIndex] = (float)peakYPos;
                    else
                        _channelPeakData[barIndex] = (float)(peakYPos + (PeakFallDelay * _channelPeakData[barIndex])) / ((float)(PeakFallDelay + 1));

                    double xCoord = BarSpacing + (_barWidth * barIndex) + (BarSpacing * barIndex) + 1;

                    _barShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - barHeight, 0, 0);
                    _barShapes[barIndex].Height = barHeight;
                    _peakShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - _channelPeakData[barIndex] - peakDotHeight, 0, 0);
                    _peakShapes[barIndex].Height = peakDotHeight;

                    if (_channelPeakData[barIndex] > 0.05)
                        allZero = false;

                    lastPeakHeight = barHeight;
                    barHeight = 0f;
                    barIndex++;
                }
            }

            if (allZero && !_soundPlayer.IsPlaying)
                _animationTimer.Stop();
        }

        private void UpdateBarLayout()
        {
            if (_soundPlayer == null || _spectrumCanvas == null)
                return;

            _barWidth = Math.Max(((double)(_spectrumCanvas.RenderSize.Width - (BarSpacing * (BarCount + 1))) / (double)BarCount), 1);
            _maximumFrequencyIndex = Math.Min(_soundPlayer.GetFFTFrequencyIndex(MaximumFrequency) + 1, 2047);
            _minimumFrequencyIndex = Math.Min(_soundPlayer.GetFFTFrequencyIndex(MinimumFrequency), 2047);
            _bandWidth = Math.Max(((double)(_maximumFrequencyIndex - _minimumFrequencyIndex)) / _spectrumCanvas.RenderSize.Width, 1.0);

            int actualBarCount;
            if (_barWidth >= 1.0d)
                actualBarCount = BarCount;
            else
                actualBarCount = Math.Max((int)((_spectrumCanvas.RenderSize.Width - BarSpacing) / (_barWidth + BarSpacing)), 1);
            _channelPeakData = new float[actualBarCount];

            int indexCount = _maximumFrequencyIndex - _minimumFrequencyIndex;
            int linearIndexBucketSize = (int)Math.Round((double)indexCount / (double)actualBarCount, 0);
            List<int> maxIndexList = new List<int>();
            List<int> maxLogScaleIndexList = new List<int>();
            double maxLog = Math.Log(actualBarCount, actualBarCount);
            for (int i = 1; i < actualBarCount; i++)
            {
                maxIndexList.Add(_minimumFrequencyIndex + (i * linearIndexBucketSize));
                int logIndex = (int)((maxLog - Math.Log((actualBarCount + 1) - i, (actualBarCount + 1))) * indexCount) + _minimumFrequencyIndex;
                maxLogScaleIndexList.Add(logIndex);
            }
            maxIndexList.Add(_maximumFrequencyIndex);
            maxLogScaleIndexList.Add(_maximumFrequencyIndex);
            _barIndexMax = maxIndexList.ToArray();
            _barLogScaleIndexMax = maxLogScaleIndexList.ToArray();

            _barHeights = new double[actualBarCount];
            _peakHeights = new double[actualBarCount];

            _spectrumCanvas.Children.Clear();
            _barShapes.Clear();
            _peakShapes.Clear();

            double height = _spectrumCanvas.RenderSize.Height;
            double peakDotHeight = Math.Max(_barWidth / 2.0f, 1);
            for (int i = 0; i < actualBarCount; i++)
            {
                double xCoord = BarSpacing + (_barWidth * i) + (BarSpacing * i) + 1;
                Rectangle barRectangle = new Rectangle()
                {
                    Margin = new Thickness(xCoord, height, 0, 0),
                    Width = _barWidth,
                    Height = 0,
                    Style = BarStyle
                };
                _barShapes.Add(barRectangle);
                Rectangle peakRectangle = new Rectangle()
                {
                    Margin = new Thickness(xCoord, height - peakDotHeight, 0, 0),
                    Width = _barWidth,
                    Height = peakDotHeight,
                    Style = PeakStyle
                };
                _peakShapes.Add(peakRectangle);
            }

            foreach (Shape shape in _barShapes)
                _spectrumCanvas.Children.Add(shape);
            foreach (Shape shape in _peakShapes)
                _spectrumCanvas.Children.Add(shape);

            ActualBarWidth = _barWidth;
        }

        private void soundPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPlaying":
                    if (_soundPlayer.IsPlaying && !_animationTimer.IsEnabled)
                        _animationTimer.Start();
                    break;
            }
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            UpdateSpectrum();
        }
    }
}

