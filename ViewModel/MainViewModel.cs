using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows.Input;

namespace Kova.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private ViewModelLocator _VMLocator;

        public RelayCommand LaunchKovaCommand { get; private set; }
        public RelayCommand<ViewModelBase> ChangeViewCommand { get; private set; }
       
        public MainViewModel()
        {
            _VMLocator = new ViewModelLocator();
            CurrentViewModel = _VMLocator.Player;

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