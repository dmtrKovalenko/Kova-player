using GalaSoft.MvvmLight.Command;
using System.Windows.Media;
using System.Windows.Input;
using MahApps.Metro;
using System.Windows;

namespace Kova.Model
{
    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }
    }
}
