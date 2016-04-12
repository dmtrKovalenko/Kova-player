using System;
using GalaSoft.MvvmLight.Command;

namespace Kova.ViewModel
{
    public class AddMusicDialogViewModel
    {
        public RelayCommand CloseCommand { get; private set; }
        private Action<AddMusicDialogViewModel> _closeHandler;

        public AddMusicDialogViewModel(Action<AddMusicDialogViewModel> closeHandler)
        {
            _closeHandler = closeHandler;
            CloseCommand = new RelayCommand(Close);
        }

        private void Close()
        {
            _closeHandler(this);
        }
    }
}
