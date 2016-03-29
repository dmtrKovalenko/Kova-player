using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Collections.ObjectModel;
using Kova.NAudioCore;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Linq;

namespace Kova.ViewModel
{
    public class AllCompositionsViewModel : ViewModelBase
    {
        private bool _isMusicPathLoaded { get; set; }
        private ObservableCollection<Song> _songs;
        private Song _currentSong { get; set; }
        private bool _inTimerPorsitionUpdate { get; set; }
        private TimeSpan _currentTime { get; set; }
        private TimeSpan _totalTime { get; set; }
        private bool _isPlaying { get; set; }
        private bool _inRepeatMode { get; set; }
        private BitmapImage _albumArtWork { get; set; }
        private bool _isEqualizerShowing { get; set; }

        public RelayCommand AddMusicFolderCommand { get; private set; }
        public RelayCommand LoadMusicPathCommand { get; private set; }
        public RelayCommand PlayNextCommand { get; private set; }
        public RelayCommand PlayPreviousCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand ShowEqualizerCommand { get; private set;}

        public AllCompositionsViewModel()
        {
            _songs = new ObservableCollection<Song>();
            LoadMusicPath();

            AddMusicFolderCommand = new RelayCommand(AddMusicFolder);
            LoadMusicPathCommand = new RelayCommand(LoadMusicPath);
            PlayNextCommand = new RelayCommand(PlayNext);
            PlayPreviousCommand = new RelayCommand(PlayPrevious);
            PlayCommand = new RelayCommand(Play);
            ShowEqualizerCommand = new RelayCommand(ShowEqualizer);

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

                if (NAudioEngine.Instance.ChannelPosition == 100)
                {
                    if (InRepeatMode)
                    {
                        PlayRepeat();
                    }
                    else
                    {
                        PlayNext();
                    }
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

        public bool InRepeatMode
        {
            get
            {
                return _inRepeatMode;
            }
            set
            {
                _inRepeatMode = value;
                RaisePropertyChanged(nameof(InRepeatMode));
            }
        }

        public bool IsEqualizerVisible
        {
            get
            {
                return _isEqualizerShowing;
            }
            set
            {
                _isEqualizerShowing = value;
                RaisePropertyChanged(nameof(IsEqualizerVisible));
            }
        }

        private void ShowEqualizer()
        {
            IsEqualizerVisible = !IsEqualizerVisible;
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
                CurrentSong = _songs[Songs.IndexOf(CurrentSong) + 1];
            }
        }

        private void PlayPrevious()
        {
            if (Songs.IndexOf(CurrentSong) != 0)
            {
                CurrentSong = Songs[Songs.IndexOf(CurrentSong) - 1];
            }
        }

        private void OpenEqualizer()
        {
 
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
            if (!_isMusicPathLoaded)
            {
                if (Properties.Settings.Default.MusicFolderPath != null)
                {
                    foreach (string path in Properties.Settings.Default.MusicFolderPath)
                    {
                        string[] FullDataPath = Directory.GetFiles(path, "*.mp3*", SearchOption.AllDirectories);
                        for (int i = 0; i < FullDataPath.Length; i++)
                        {
                            Songs.Add(new Song(FullDataPath[i]));
                        }
                    }
                    CurrentSong = Songs[0];
                    NAudioEngine.Instance.Pause();
                    _isMusicPathLoaded = true;
                }
            }
        }
    }
}
