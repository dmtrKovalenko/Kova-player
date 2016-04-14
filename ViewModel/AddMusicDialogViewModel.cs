using System;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kova.ViewModel
{
    public class AddMusicDialogViewModel
    {
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand<string> RemoveCommand { get; private set; }

        private Action<AddMusicDialogViewModel> _closeHandler;

        public AddMusicDialogViewModel(Action<AddMusicDialogViewModel> closeHandler)
        {
            _closeHandler = closeHandler;

            CloseCommand = new RelayCommand(Close);
            RemoveCommand = new RelayCommand<string>((item)=>Remove(item));
        }

        private void Remove(string selectedItem)
        {
            Properties.Settings.Default.MusicFolderPath.Remove(selectedItem);
        }

        private void Close()
        {
            Properties.Settings.Default.Save();
            _closeHandler(this);
        }
    }
}
