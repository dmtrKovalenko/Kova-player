using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows.Input;

namespace Kova.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private ViewModelBase _currentViewModel;
        private ViewModelLocator _VMLocator;

        public RelayCommand LaunchKovaCommand { get; private set; }
        public RelayCommand<ViewModelBase> ChangeViewCommand { get; private set; }
        public RelayCommand ShowMessegeDialogCommand { get; private set; }

        public MainViewModel()
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _VMLocator = new ViewModelLocator();
            CurrentViewModel = _VMLocator.Player;

            ChangeViewCommand = new RelayCommand<ViewModelBase>((View) => ChangeView(View));
            LaunchKovaCommand = new RelayCommand(LaunchKova);
            ShowMessegeDialogCommand = new RelayCommand(ShowDialogAsync);
        }

        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                _currentViewModel = value;
                RaisePropertyChanged(nameof(CurrentViewModel));
            }
        }

        private async void ShowDialogAsync()
        {
            var customDialog = new CustomDialog() { Title = "Add music" };
            var addMusicDialog = new AddMusicDialogViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, _dialogCoordinator);

            customDialog.Content = new Views.AddMusicDialog() { DataContext = addMusicDialog };
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void ChangeView(ViewModelBase other)
        {
            CurrentViewModel = other;
        }

        private void LaunchKova()
        {
            System.Diagnostics.Process.Start("https://github.com/dmtrKovalenko/Kova-player");
        }
    }
}