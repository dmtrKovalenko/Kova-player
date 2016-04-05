using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;

namespace Kova.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private ViewModelBase _currentViewModel { get; set; }

        private ViewModelLocator _VMlock { get; set; }
        public RelayCommand LaunchKovaCommand { get; private set; }
        public RelayCommand<ViewModelBase> ChangeViewCommand { get; private set; }

        public MainViewModel()
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _VMlock = new ViewModelLocator();
            CurrentViewModel = _VMlock.Player;

            ChangeViewCommand = new RelayCommand<ViewModelBase>((View) => ChangeView(View));
            LaunchKovaCommand = new RelayCommand(LaunchKova);
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