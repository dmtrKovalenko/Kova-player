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
        private ViewModelBase _currentViewModel { get; set; }
        private SettingsViewModel _settingsVM { get; set; }
        private AllCompositionsViewModel _allCompositionsVM { get; set; }


        public RelayCommand ChangeViewCommand { get; private set; }
        public RelayCommand ChangeToCommand { get; private set; }

        public MainViewModel()
        {
            _currentViewModel = new AllCompositionsViewModel();
            _settingsVM = new SettingsViewModel();
            _allCompositionsVM = new AllCompositionsViewModel();

            ChangeViewCommand = new RelayCommand(ChangeView);
            ChangeToCommand = new RelayCommand(ChangeTo);
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
            CurrentViewModel = _settingsVM;
        }

        private void ChangeTo()
        {
            CurrentViewModel = _allCompositionsVM;
        }
    }
}