using System.Windows;
using Kova.ViewModel;
using MahApps.Metro.Controls;
using Kova.NAudioCore;

namespace Kova
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) =>
            {
                ViewModelLocator.Cleanup();
                Properties.Settings.Default.Save();
                NAudioEngine.Instance.Dispose();
            };
        }
    }
}