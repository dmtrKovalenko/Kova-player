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

        private ViewModelLocator VMlock { get; set; }
        public RelayCommand LaunchKovaCommand { get; private set; }
        public RelayCommand ChangeViewCommand { get; private set; }
        public RelayCommand ChangeToCommand { get; private set; }

        public MainViewModel()
        {
            VMlock = new ViewModelLocator();

            _settingsVM = new SettingsViewModel();
            _allCompositionsVM = new AllCompositionsViewModel();
            CurrentViewModel = new AllCompositionsViewModel();

            ChangeViewCommand = new RelayCommand(ChangeView);
            ChangeToCommand = new RelayCommand(ChangeTo);
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

        private void ChangeView()
        {
            CurrentViewModel = VMlock.Settings;
        }

        private void ChangeTo()
        {
            CurrentViewModel = VMlock.AllCompositions;
        }

        private void LaunchKova()
        {
            System.Diagnostics.Process.Start("https://github.com/dmtrKovalenko/Kova-player");
        }
    }
}