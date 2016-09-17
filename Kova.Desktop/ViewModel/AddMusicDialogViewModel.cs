using System;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using GalaSoft.MvvmLight.Messaging;

namespace Kova.ViewModel
{
    public class AddMusicDialogViewModel
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand<string> RemoveCommand { get; private set; }
        public RelayCommand AddMusicFolderCommand { get; private set; }

        private bool _isLibraryUpdated; 

        public AddMusicDialogViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            AddMusicFolderCommand = new RelayCommand(AddMusicFolder);
            CloseCommand = new RelayCommand(Close);
            RemoveCommand = new RelayCommand<string>((item) => ShowRemovingMessageAsync(item));
        }

        private async void ShowRemovingMessageAsync(string selectedItem)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "Cancel",
            };

            await _dialogCoordinator.ShowMessageAsync(this, "Folder deleting", "After deleting this folder, it will no longer appear in this application, but will not be deleted physically.",
                MessageDialogStyle.AffirmativeAndNegative, mySettings).ContinueWith(t =>
                {
                    if (t.Result != MessageDialogResult.Negative)
                    {
                        Remove(selectedItem);
                    }
                });
        }

        private void Remove(string removeItem)
        {
            Action action = () =>
            {
                Properties.Settings.Default.MusicFolderPath.Remove(removeItem);
            };
            App.Current.Dispatcher.BeginInvoke(action);
            _isLibraryUpdated = true;
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

            _isLibraryUpdated = true;
        }

        private void Close()
        {
            Properties.Settings.Default.Save();
            Messenger.Default.Send(_isLibraryUpdated);
        }
    }
}
