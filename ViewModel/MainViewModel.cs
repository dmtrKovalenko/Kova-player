using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Kova.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ViewModelBase _currentViewModel { get; set; }

        public RelayCommand ChangeViewCommand { get; private set; }

        public MainViewModel()
        {
            _currentViewModel = new AllCompositionsViewModel();
            ChangeViewCommand = new RelayCommand(ChangeView);
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

        private void ChangeView()
        {
            CurrentViewModel = new SettingsViewModel();
        }
    }
}