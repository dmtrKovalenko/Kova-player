using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Collections.ObjectModel;
using Kova.NAudioCore;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls.Dialogs;

namespace Kova.ViewModel
{
    public class PlayerViewModel : ViewModelBase
    {
        
        private ObservableCollection<Song> _songs;
        private Song _currentSong;
        private TimeSpan _currentTime;
        private bool _inRepeatSet;
        private BitmapImage _albumArtWork;
        private bool _isPlayBackQueueOpened;
        private bool _isVolumePopupOpened;
        private bool _isMuted;
        private float _lastVolume;

        public RelayCommand VolumePopupOpenCommand { get; private set; }
        public RelayCommand AddMusicFolderCommand { get; private set; }
        public RelayCommand PlayNextCommand { get; private set; }
        public RelayCommand PlayPreviousCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand ShowPlaybackQueueCommand { get; private set; }
        public RelayCommand MuteCommand { get; private set; }
        public NAudioEngine Player { get; private set; }

        public PlayerViewModel()
        {
            LoadMusicPath();

            Player = NAudioEngine.Instance;
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
                        }
                    }
                    else
                    {
                        AlbumArtWork = null;
                    }
                }
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
                if (_currentSong == null)
                {
                    CurrentSong = Songs[25];
                    NAudioEngine.Instance.Stop();
                }
                return _currentSong;
            }
            set
            {
                _currentSong = value;
                Player.OpenFile(value.OriginalPath);
                Player.Play();
                RaisePropertyChanged(nameof(CurrentSong));
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

        public bool IsPlaybackQueueOpened
        {
            get
            {
                return _isPlayBackQueueOpened;
            }
            set
            {
                _isPlayBackQueueOpened = value;
                RaisePropertyChanged(nameof(IsPlaybackQueueOpened));
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
                _lastVolume = Player.Volume;
                Player.Volume = 0;
            }
            else
            {
                Player.Volume = _lastVolume;
            }
        }

        private void OpenVolumePopup()
        {
            IsVolumePopupOpened = !IsVolumePopupOpened;
        }

        private void ShowPlaybackQueue()
        {
            IsPlaybackQueueOpened = !IsPlaybackQueueOpened;
        }

        private void Play()
        {
            if (Player.CanPlay)
            {
                Player.Play();
            }
            else if (Player.CanPause)
            {
                Player.Pause();
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
                    var DirectoryMusicPathes = Directory.GetFiles(dialog.SelectedPath, "*.mp3*", SearchOption.AllDirectories);
                    for (int i = 0; i < DirectoryMusicPathes.Length; i++)
                    {
                        Song song = new Song(DirectoryMusicPathes[i]);
                        if (!Songs.Contains(song))
                            Songs.Add(song);
                    }
                }
            }
        }

        private void LoadMusicPath()
        {
            Songs = new ObservableCollection<Song>();
            if (Properties.Settings.Default.MusicFolderPath != null)
            {
                foreach (string path in Properties.Settings.Default.MusicFolderPath)
                {
                    foreach (var item in Directory.GetFiles(path, "*.mp3*", SearchOption.AllDirectories))
                    {
                        Song song = new Song(item);
                        if (!Songs.Contains(song))
                        {
                            Songs.Add(song);
                        }
                    }
                }
            }
        }
    }
}
