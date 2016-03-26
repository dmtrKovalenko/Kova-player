using GalaSoft.MvvmLight;
using MahApps.Metro;
using Kova.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;

namespace Kova.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private List<AccentColorMenuData> _accentColors { get; set; }
        private List<AppTheme> _appThemes { get; set; }

        private AccentColorMenuData _selectedAccentColor { get; set; }
        private AppTheme _selectedTheme { get; set; }

        public SettingsViewModel()
        {
            _appThemes = ThemeManager.AppThemes.ToList();

            _accentColors = ThemeManager.Accents
                                          .Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                          .ToList();
            var theme = ThemeManager.DetectAppStyle(Application.Current);

            SelectedAccentColor = AccentColors[Properties.Settings.Default.SelectedAccentIndex];
            SelectedTheme = AppThemes[Properties.Settings.Default.SelectedThemeIndex];
        }

        public List<AccentColorMenuData> AccentColors
        {
            get { return _accentColors; }
        }

        public List<AppTheme> AppThemes
        {
            get { return _appThemes; }
        }

        public AccentColorMenuData SelectedAccentColor
        {
            get
            {
                return _selectedAccentColor;
            }
            set
            {
                _selectedAccentColor = value;
                DoChangeAccent(value);
                Properties.Settings.Default.SelectedAccentIndex = AccentColors.IndexOf(value);
                RaisePropertyChanged(nameof(SelectedAccentColor));
            }
        }

        public AppTheme SelectedTheme
        {
            get
            {
                return _selectedTheme;
            }
            set
            {
                _selectedTheme = value;
                DoChangeTheme(value);
                Properties.Settings.Default.SelectedThemeIndex = AppThemes.IndexOf(value);
                RaisePropertyChanged(nameof(SelectedTheme));
            }
        }

        private void DoChangeAccent(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(SelectedAccentColor.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }

        private void DoChangeTheme(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, SelectedTheme);
        }
    }
}
