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
        private AccentColorMenuData _selectedAccentColor { get; set; }

        public SettingsViewModel()
        {
            _accentColors = ThemeManager.Accents.Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush }).ToList();
        }

        public List<AccentColorMenuData> AccentColors
        {
            get { return _accentColors; }
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
                DoChangeTheme(value);
                RaisePropertyChanged(nameof(SelectedAccentColor));
            }
        }

        protected virtual void DoChangeTheme(object sender)
        {

            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(SelectedAccentColor.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }
}
