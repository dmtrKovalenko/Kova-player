using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kova.NAudioCore;

namespace Kova.Views
{
    /// <summary>
    /// Логика взаимодействия для MediaControl.xaml
    /// </summary>
    public partial class MediaControl : UserControl
    {
        public MediaControl()
        {
            InitializeComponent();
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            NAudioEngine.Instance.Play();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            NAudioEngine.Instance.Pause();
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void Open()
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "(*.mp3)|*.mp3";
            if (openDialog.ShowDialog() == true)
            {
                NAudioEngine.Instance.OpenFile(openDialog.FileName);
                //   FileText.Text = openDialog.FileName;
            }
        }
    }
}
