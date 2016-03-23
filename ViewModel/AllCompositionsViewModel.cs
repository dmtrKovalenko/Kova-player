using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Collections.ObjectModel;
using Kova.NAudioCore;
using System.ComponentModel;

namespace Kova.ViewModel
{
    public class AllCompositionsViewModel : ViewModelBase
    {
        private ObservableCollection<Song> _songs = new ObservableCollection<Song>();
        private Song _currentSong;
        private bool _inTimerPorsitionUpdate;
        private TimeSpan _currentTime;
        private TimeSpan _totalTime;
        private bool _isPlaying;
        private bool _inRepeatMode;

        public RelayCommand AddMusicFolderCommand { get; private set; }
        public RelayCommand LoadMusicPathCommand { get; private set; }
        public RelayCommand SeekValueChangedCommand { get; private set; }
        public RelayCommand PlayNextCommand { get; private set; }
        public RelayCommand PlayPreviousCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }

        public AllCompositionsViewModel()
        {
            NAudioEngine.Instance.PropertyChanged += NAudioENgine_PropertyChanged;
            AddMusicFolderCommand = new RelayCommand(AddMusicFolder);
            LoadMusicPathCommand = new RelayCommand(LoadMusicPath);
            PlayNextCommand = new RelayCommand(PlayNext);
            PlayPreviousCommand = new RelayCommand(PlayPrevious);
            PlayCommand = new RelayCommand(Play);
        }

        private void NAudioENgine_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                    TotalTime = NAudioEngine.Instance.ActiveStream.TotalTime;
            }

            if (e.PropertyName == "IsPlaying")
            {
                IsPlaying = NAudioEngine.Instance.IsPlaying;
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
            else
            {
                CurrentSong = Songs[0];
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
                Properties.Settings.Default.MusicFolderPath = dialog.SelectedPath;
                Properties.Settings.Default.Save();
                string[] FullDataPath = Directory.GetFiles(dialog.SelectedPath, "*.mp3*", SearchOption.AllDirectories);

                for (int i = 0; i < FullDataPath.Length; i++)
                {
                    Songs.Add(new Song(FullDataPath[i]));
                }
            }
        }

        private void LoadMusicPath()
        {
            string[] FullDataPath = Directory.GetFiles(Properties.Settings.Default.MusicFolderPath, "*.mp3*", SearchOption.AllDirectories);

            for (int i = 0; i < FullDataPath.Length; i++)
            {
                Songs.Add(new Song(FullDataPath[i]));
            }
            CurrentSong = Songs[0];
            NAudioEngine.Instance.Pause();
        }
    }
}
