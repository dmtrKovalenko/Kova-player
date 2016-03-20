using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.IO;
using Kova;
using System.Collections.ObjectModel;

namespace Kova.ViewModel
{
    public class AllCompositionsViewModel : ViewModelBase
    {
        private ObservableCollection<Song> _songs = new ObservableCollection<Song>();
        public RelayCommand AddMusicFolderCommand { get; private set; }

        public AllCompositionsViewModel()
        {
            AddMusicFolderCommand = new RelayCommand(AddMusicFolder);
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
    }
}
