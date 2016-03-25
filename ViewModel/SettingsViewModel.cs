using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Collections.ObjectModel;
using Kova.NAudioCore;
using System.ComponentModel;
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
        private List<AccentColorMenuData> _appThemes { get; set; }

        private AccentColorMenuData _selectedAccentColor;
        protected AccentColorMenuData _selectedTheme { get; set; }

        public SettingsViewModel()
        {
            _appThemes = ThemeManager.AppThemes
                                          .Select(a => new AccentColorMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush })
                                          .ToList();

            _accentColors = ThemeManager.Accents
                                          .Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                          .ToList();
            var theme = ThemeManager.DetectAppStyle(Application.Current);

            SelectedAccentColor = AccentColors[6];
            SelectedTheme = AppThemes[1];
        }

        public List<AccentColorMenuData> AccentColors
        {
            get { return _accentColors; }
        }

        public List<AccentColorMenuData> AppThemes
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
                RaisePropertyChanged(nameof(SelectedAccentColor));
            }
        }

        public AccentColorMenuData SelectedTheme
        {
            get
            {
                return _selectedTheme;
            }
            set
            {
                _selectedTheme = value;
                DoChangeTheme(value);
                RaisePropertyChanged(nameof(SelectedTheme));
            }
        }

        protected virtual void DoChangeAccent(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(SelectedAccentColor.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }

        protected virtual void DoChangeTheme(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(SelectedTheme.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
    }
}
