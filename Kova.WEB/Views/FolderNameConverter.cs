using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Kova.Views
{
    [ValueConversion(typeof(string), typeof(string))]
    public class FolderNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DirectoryInfo folder = new DirectoryInfo((string)value);
            if (folder.Name.Length>28)
            {
                return folder.Name.Substring(0, 28); //Correct view by the title property of the tile
            }
            else
            {
                return folder.Name;
            }           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
