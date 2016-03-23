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

namespace Kova.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public List<AccentColorMenuData> AccentColors { get; set; }
        public SettingsViewModel()
        {
            AccentColors = ThemeManager.Accents.Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                                                .ToList();
        }
    }
}
