using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using NAudio.Wave;
using WPFSoundVisualizationLib;

namespace Kova.NAudioCore
{
    public class NAudioEngine : INotifyPropertyChanged, ISpectrumPlayer, IDisposable
    {
        private static NAudioEngine _instance;
        private readonly DispatcherTimer _positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly BackgroundWorker _waveformGenerateWorker = new BackgroundWorker();
        private readonly int _fftDataSize = (int)FFTDataSize.FFT2048;
        private bool _disposed;
        private bool _canPlay;
        private bool _canPause;
        private bool _canStop;
        private bool _isPlaying;
        private bool _inChannelTimerUpdate;
        private double _channelLength;
        private double _channelPosition;
        private IWavePlayer _waveOutDevice;
        private WaveStream _activeStream;
        private WaveChannel32 _inputStream;
        public AudioFileReader _audioFileReader;
        private Aggregator _aggregator;
        private Aggregator _waveformAggregator;
        private string _pendingWaveformPath;
        private float[] _fullLevelData;

        private const int waveformCompressedPointCount = 2000;

        public static NAudioEngine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NAudioEngine();
                return _instance;
            }
        }

        private NAudioEngine()
        {
            _positionTimer.Interval = TimeSpan.FromMilliseconds(25);
            _positionTimer.Tick += positionTimer_Tick;

            _waveformGenerateWorker.DoWork += waveformGenerateWorker_DoWork;
            _waveformGenerateWorker.RunWorkerCompleted += waveformGenerateWorker_RunWorkerCompleted;
            _waveformGenerateWorker.WorkerSupportsCancellation = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopAndCloseStream();
                }
                _disposed = true;
            }
        }

        //Spectrum analyzer
        public bool GetFFTData(float[] fftDataBuffer)
        {
            _aggregator.GetFFTResults(fftDataBuffer);
            return IsPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double maxFrequency;
            if (ActiveStream != null)
                maxFrequency = ActiveStream.WaveFormat.SampleRate / 2.0d;
            else
                maxFrequency = 22050; //44.1 kHz 
            return (int)((frequency / maxFrequency) * (_fftDataSize / 2));
        }

        public double ChannelLength
        {
            get { return _channelLength; }
            protected set
            {
                double oldValue = _channelLength;
                _channelLength = value;
                if (oldValue != _channelLength)
                    NotifyPropertyChanged("ChannelLength");
            }
        }

        public double ChannelPosition
        {
            get { return _channelPosition; }
            set
            {
                double oldValue = _channelPosition;
                double position = Math.Max(0, Math.Min(value, ChannelLength));
                if (!_inChannelTimerUpdate && ActiveStream != null)
                    ActiveStream.Position = (long)((position / 100) * ActiveStream.Length);
                _channelPosition = position;
                if (oldValue != _channelPosition)
                    NotifyPropertyChanged("ChannelPosition");
                if (ChannelPosition == ChannelLength)
                    _waveOutDevice.Stop();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private class WaveformGenerationParams
        {
            public WaveformGenerationParams(int points, string path)
            {
                Points = points;
                Path = path;
            }

            public int Points { get; protected set; }
            public string Path { get; protected set; }
        }

        private void GenerateWaveformData(string path)
        {
            if (_waveformGenerateWorker.IsBusy)
            {
                _pendingWaveformPath = path;
                _waveformGenerateWorker.CancelAsync();
                return;
            }

            if (!_waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
                _waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, path));
        }

        private void waveformGenerateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                if (!_waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
                    _waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, _pendingWaveformPath));
            }
        }

        private void waveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WaveformGenerationParams waveformParams = e.Argument as WaveformGenerationParams;
            Mp3FileReader waveformMp3Stream = new Mp3FileReader(waveformParams.Path);
            WaveChannel32 waveformInputStream = new WaveChannel32(waveformMp3Stream);
            waveformInputStream.Sample += waveStream_Sample;

            int frameLength = _fftDataSize;
            int frameCount = (int)((double)waveformInputStream.Length / (double)frameLength);
            int waveformLength = frameCount * 2;
            byte[] readBuffer = new byte[frameLength];
            _waveformAggregator = new Aggregator(frameLength);

            float maxLeftPointLevel = float.MinValue;
            float maxRightPointLevel = float.MinValue;
            int currentPointIndex = 0;
            float[] waveformCompressedPoints = new float[waveformParams.Points];
            List<float> waveformData = new List<float>();
            List<int> waveMaxPointIndexes = new List<int>();

            for (int i = 1; i <= waveformParams.Points; i++)
            {
                waveMaxPointIndexes.Add((int)Math.Round(waveformLength * ((double)i / (double)waveformParams.Points), 0));
            }

            //read FFT data 
            int readCount = 0;
            while (currentPointIndex * 2 < waveformParams.Points)
            {
                waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

                waveformData.Add(_waveformAggregator.LeftMaxVolume);
                waveformData.Add(_waveformAggregator.RightMaxVolume);

                if (_waveformAggregator.LeftMaxVolume > maxLeftPointLevel)
                    maxLeftPointLevel = _waveformAggregator.LeftMaxVolume;
                if (_waveformAggregator.RightMaxVolume > maxRightPointLevel)
                    maxRightPointLevel = _waveformAggregator.RightMaxVolume;

                if (readCount > waveMaxPointIndexes[currentPointIndex])
                {
                    waveformCompressedPoints[(currentPointIndex * 2)] = maxLeftPointLevel;
                    waveformCompressedPoints[(currentPointIndex * 2) + 1] = maxRightPointLevel;
                    maxLeftPointLevel = float.MinValue;
                    maxRightPointLevel = float.MinValue;
                    currentPointIndex++;
                }
                if (readCount % 3000 == 0)
                {
                    float[] clonedData = (float[])waveformCompressedPoints.Clone();
                }

                if (_waveformGenerateWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                readCount++;
            }

            float[] finalClonedData = (float[])waveformCompressedPoints.Clone();
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                _fullLevelData = waveformData.ToArray();
            }));
            waveformInputStream.Close();
        }

        // Utility methodes
        private void StopAndCloseStream()
        {
            if (_waveOutDevice != null)
            {
                _waveOutDevice.Stop();
            }
            if (_activeStream != null)
            {
                try
                {
                    _inputStream.Close();
                    _inputStream = null;
                }
                catch
                {
                    _inputStream = null;
                }
            }
            if (_waveOutDevice != null)
            {
                _waveOutDevice.Dispose();
                _waveOutDevice = null;
            }
        }

        public void Stop()
        {
            if (_waveOutDevice != null)
            {
                _waveOutDevice.Stop();
            }
            IsPlaying = false;
            CanStop = false;
            CanPlay = true;
            CanPause = false;
        }

        public void Pause()
        {
            if (IsPlaying && CanPause)
            {
                _waveOutDevice.Pause();
                IsPlaying = false;
                CanPlay = true;
                CanPause = false;
            }
        }

        public void Play()
        {
            if (CanPlay)
            {
                _waveOutDevice.Play();
                IsPlaying = true;
                CanPause = true;
                CanPlay = false;
                CanStop = true;
            }
        }

        public void OpenFile(string path)
        {
            if (ActiveStream != null)
            {
                ChannelPosition = 0;
            }

            StopAndCloseStream();

            if (System.IO.File.Exists(path))
            {
                try
                {
                    _waveOutDevice = new WaveOutEvent()
                    {
                        DesiredLatency = 65
                    };

                    ActiveStream = new AudioFileReader(path);
                    _inputStream = new WaveChannel32(ActiveStream);
                    _aggregator = new Aggregator(_fftDataSize);
                    _inputStream.Sample += inputStream_Sample;
                    _waveOutDevice.Init(_inputStream);
                    ChannelLength = 100;
                    GenerateWaveformData(path);
                    CanPlay = true;
                    _inputStream.PadWithZeroes = false;
                    _waveOutDevice.PlaybackStopped += new EventHandler<StoppedEventArgs>(OnPlaybackStopped);
                }
                catch
                {
                    ActiveStream = null;
                    CanPlay = false;
                }
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (ChannelPosition == ChannelLength) 
               NotifyPropertyChanged("PlaybackStopped");
        }

        public WaveStream ActiveStream
        {
            get { return _activeStream; }
            protected set
            {
                WaveStream oldValue = _activeStream;
                _activeStream = value;
                if (oldValue != _activeStream)
                    NotifyPropertyChanged("ActiveStream");
            }
        }

        //using volume of waveOutDevice for constantly size of channel position 
        //and respectively constant drawing size for spectrum analyzer
        public float Volume
        {
            get
            {
                if (_waveOutDevice == null)
                {
                    return 0;
                }
                return _waveOutDevice.Volume;
            }
            set
            {
                _waveOutDevice.Volume = value;
                NotifyPropertyChanged(nameof(Volume));
            }
        }

        public bool CanPlay
        {
            get { return _canPlay; }
            protected set
            {
                bool oldValue = _canPlay;
                _canPlay = value;
                if (oldValue != _canPlay)
                    NotifyPropertyChanged("CanPlay");
            }
        }

        public bool CanPause
        {
            get { return _canPause; }
            protected set
            {
                bool oldValue = _canPause;
                _canPause = value;
                if (oldValue != _canPause)
                    NotifyPropertyChanged("CanPause");
            }
        }

        public bool CanStop
        {
            get { return _canStop; }
            protected set
            {
                bool oldValue = _canStop;
                _canStop = value;
                if (oldValue != _canStop)
                    NotifyPropertyChanged("CanStop");
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            protected set
            {
                bool oldValue = _isPlaying;
                _isPlaying = value;
                if (oldValue != _isPlaying)
                    NotifyPropertyChanged("IsPlaying");
                _positionTimer.IsEnabled = value;
            }
        }

        private void inputStream_Sample(object sender, SampleEventArgs e)
        {
            _aggregator.Add(e.Left, e.Right);
        }

        void waveStream_Sample(object sender, SampleEventArgs e)
        {
            _waveformAggregator.Add(e.Left, e.Right);
        }

        void positionTimer_Tick(object sender, EventArgs e)
        {
            _inChannelTimerUpdate = true;
            ChannelPosition = ((double)ActiveStream.Position / (double)ActiveStream.Length) * 100;
            _inChannelTimerUpdate = false;
        }
    }
}