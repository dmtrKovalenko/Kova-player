using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Collections.ObjectModel;
using Kova.NAudioCore;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Kova.ViewModel
{
    public class AllCompositionsViewModel : ViewModelBase
    {
        private ObservableCollection<Song> _songs { get; set; }
        private Song _currentSong { get; set; }
        private bool _inTimerPorsitionUpdate { get; set; }
        private TimeSpan _currentTime { get; set; }
        private TimeSpan _totalTime { get; set; }
        private bool _isPlaying { get; set; }
        private bool _inRepeatSet { get; set; }
        private BitmapImage _albumArtWork { get; set; }
        private bool _isEqualizerShowing { get; set; }
        private bool _isVolumePopupOpened { get; set; }
        private bool _isMuted { get; set; }
        private float _lastVolume { get; set; }

        public RelayCommand VolumePopupOpenCommand { get; private set; }
        public RelayCommand AddMusicFolderCommand { get; private set; }
        public RelayCommand PlayNextCommand { get; private set; }
        public RelayCommand PlayPreviousCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand ShowPlaybackQueueCommand { get; private set; }
        public RelayCommand MuteCommand { get; private set; }

        public AllCompositionsViewModel()
        {
            _songs = new ObservableCollection<Song>();
            LoadMusicPath();

            CurrentSong = Songs[0];
            NAudioEngine.Instance.Stop();
            Volume = 1;

            AddMusicFolderCommand = new RelayCommand(AddMusicFolder);
            PlayNextCommand = new RelayCommand(PlayNext);
            PlayPreviousCommand = new RelayCommand(PlayPrevious);
            PlayCommand = new RelayCommand(Play);
            ShowPlaybackQueueCommand = new RelayCommand(ShowPlaybackQueue);
            VolumePopupOpenCommand = new RelayCommand(OpenVolumePopup);
            MuteCommand = new RelayCommand(Mute);

            NAudioEngine.Instance.PropertyChanged += NAudioEngine_PropertyChanged;
        }

        private void NAudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChannelPosition")
            {
                _inTimerPorsitionUpdate = true;
                CurrentPosition = NAudioEngine.Instance.ChannelPosition;
                RaisePropertyChanged(nameof(CurrentPosition));
                _inTimerPorsitionUpdate = false;

                CurrentTime = NAudioEngine.Instance.ActiveStream.CurrentTime;
            }

            if (e.PropertyName == "PlaybackStopped")
            {
                if (InRepeatSet)
                {
                    PlayRepeat();
                }
                else
                {
                    PlayNext();
                }
            }

            if (e.PropertyName == "ActiveStream")
            {
                if (NAudioEngine.Instance.ActiveStream != null)
                {
                    TotalTime = NAudioEngine.Instance.ActiveStream.TotalTime;
                    var file = TagLib.File.Create(CurrentSong.OriginalPath);
                    if (file.Tag.Pictures.Length > 0)
                    {
                        using (MemoryStream albumArtworkMemStream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                        {
                            try
                            {
                                BitmapImage albumImage = new BitmapImage();
                                albumImage.BeginInit();
                                albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                albumImage.StreamSource = albumArtworkMemStream;
                                albumImage.EndInit();
                                AlbumArtWork = albumImage;
                            }
                            catch (NotSupportedException)
                            {
                                AlbumArtWork = null;
                                // No image
                            }
                            albumArtworkMemStream.Close();
                        }
                    }
                    else
                    {
                        AlbumArtWork = null;
                    }
                }
            }

            if (e.PropertyName == "IsPlaying")
            {
                IsPlaying = NAudioEngine.Instance.IsPlaying;
            }
        }

        public BitmapImage AlbumArtWork
        {
            get
            {
                return _albumArtWork;
            }
            set
            {
                _albumArtWork = value;
                RaisePropertyChanged(nameof(AlbumArtWork));
            }
        }

        public ObservableCollection<Song> Songs
        {
            get
            {
                return _songs;
            }
            set
            {
                _songs = value;
                RaisePropertyChanged(nameof(Songs));
            }
        }

        public Song CurrentSong
        {
            get
            {
                return _currentSong;
            }
            set
            {
                _currentSong = value;
                NAudioEngine.Instance.OpenFile(value.OriginalPath);
                NAudioEngine.Instance.Play();
                RaisePropertyChanged(nameof(CurrentSong));
            }
        }

        public double CurrentPosition
        {
            get
            {
                return NAudioEngine.Instance.ChannelPosition;
            }
            set
            {
                if (!_inTimerPorsitionUpdate)
                {
                    NAudioEngine.Instance.ChannelPosition = value;
                    RaisePropertyChanged(nameof(CurrentPosition));
                }
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                return _currentTime;
            }
            private set
            {
                _currentTime = value;
                RaisePropertyChanged(nameof(CurrentTime));
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                return _totalTime;
            }
            private set
            {
                _totalTime = value;
                RaisePropertyChanged(nameof(TotalTime));
            }
        }

        public bool IsPlaying
        {
            get
            {
                return NAudioEngine.Instance.IsPlaying;
            }
            set
            {
                _isPlaying = value;
                RaisePropertyChanged(nameof(IsPlaying));
            }
        }

        public bool InRepeatSet
        {
            get
            {
                return _inRepeatSet;
            }
            set
            {
                _inRepeatSet = value;
                RaisePropertyChanged(nameof(InRepeatSet));
            }
        }

        public bool IsPlaybackQueueOpen
        {
            get
            {
                return _isEqualizerShowing;
            }
            set
            {
                _isEqualizerShowing = value;
                RaisePropertyChanged(nameof(IsPlaybackQueueOpen));
            }
        }

        public bool IsVolumePopupOpened
        {
            get
            {
                return _isVolumePopupOpened;
            }
            set
            {
                _isVolumePopupOpened = value;
                RaisePropertyChanged(nameof(IsVolumePopupOpened));
            }
        }

        public float Volume
        {
            get
            {
                return NAudioEngine.Instance.Volume;
            }
            set
            {
                NAudioEngine.Instance.Volume = value;
                RaisePropertyChanged(nameof(Volume));
            }
        }

        public bool IsMuted
        {
            get
            {
                return _isMuted;
            }
            set
            {
                _isMuted = value;
                RaisePropertyChanged(nameof(IsMuted));
            }
        }

        private void Mute()
        {
            IsMuted = !IsMuted;
            if (IsMuted)
            {
                _lastVolume = Volume;
                Volume = 0;
            }
            else
            {
                Volume = _lastVolume;
            }
        }

        private void OpenVolumePopup()
        {
            IsVolumePopupOpened = !IsVolumePopupOpened;
        }

        private void ShowPlaybackQueue()
        {
            IsPlaybackQueueOpen = !IsPlaybackQueueOpen;
        }

        private void Play()
        {
            if (NAudioEngine.Instance.CanPlay)
            {
                NAudioEngine.Instance.Play();
            }
            else if (NAudioEngine.Instance.CanPause)
            {
                NAudioEngine.Instance.Pause();
            }
        }

        private void PlayNext()
        {
            if (Songs.IndexOf(CurrentSong) != Songs.Count - 1)
            {
                CurrentSong = Songs[Songs.IndexOf(CurrentSong) + 1];
            }
        }

        private void PlayPrevious()
        {
            if (Songs.IndexOf(CurrentSong) != 0)
            {
                CurrentSong = Songs[Songs.IndexOf(CurrentSong) - 1];
            }
        }

        private void PlayRepeat()
        {
            CurrentSong = Songs[Songs.IndexOf(CurrentSong)];
        }

        private void AddMusicFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (!Properties.Settings.Default.MusicFolderPath.Contains(dialog.SelectedPath))
                {
                    Properties.Settings.Default.MusicFolderPath.Add(dialog.SelectedPath);
                }
            }
        }

        private void LoadMusicPath()
        {
            if (Properties.Settings.Default.MusicFolderPath != null)
            {
                foreach (string path in Properties.Settings.Default.MusicFolderPath)
                {
                    string[] FullDataPath = Directory.GetFiles(path, "*.mp3*", SearchOption.AllDirectories);
                    for (int i = 0; i < FullDataPath.Length; i++)
                    {
                        Song song = new Song(FullDataPath[i]);
                        if (!Songs.Contains(song))
                            Songs.Add(song);
                    }
                }
            }
        }
    }
}
