using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для AllCompositions.xaml
    /// </summary>
    public partial class AllCompositions : UserControl
    {
        string[] fullfilesPat;
        public AllCompositions()
        {
            InitializeComponent();
            Composititons.SelectionChanged += Composititons_Selected;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                fullfilesPat = Directory.GetFiles(dialog.SelectedPath, "*.mp3*", SearchOption.AllDirectories);
                Composititons.ItemsSource = fullfilesPat;
            }
        }

        private void Composititons_Selected(object sender, RoutedEventArgs e)
        {
            NAudioEngine.Instance.OpenFile(fullfilesPat[Composititons.SelectedIndex]);
            NAudioEngine.Instance.Play();
        }
    }
}
