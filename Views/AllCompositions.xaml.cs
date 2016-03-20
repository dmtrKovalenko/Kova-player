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
using Kova;

namespace Kova.Views
{
    /// <summary>
    /// Логика взаимодействия для AllCompositions.xaml
    /// </summary>
    public partial class AllCompositions : UserControl
    {
         List<Song> FullDataPath1;
         public AllCompositions()
        {
            InitializeComponent();
      //      Composititons.SelectionChanged += Composititons_Selected;
    //        Composititons.Loaded += Loded;
        }

        private void Loded(object sender, RoutedEventArgs e)
        {
            string[] FullDataPath = Directory.GetFiles(Properties.Settings.Default.MusicFolderPath, "*.mp3*", SearchOption.AllDirectories);
            FullDataPath1 = new List<Song>(FullDataPath.Length);
            for (int i = 0; i < FullDataPath.Length; i++)
            {
                FullDataPath1.Add(new Song(FullDataPath[i]));
            }
            Composititons.ItemsSource = FullDataPath1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.MusicFolderPath = dialog.SelectedPath;
                Properties.Settings.Default.Save();
                string []FullDataPath = Directory.GetFiles(dialog.SelectedPath, "*.mp3*", SearchOption.AllDirectories);
                FullDataPath1 = new List<Song>(FullDataPath.Length);
                for(int i=0; i<FullDataPath.Length; i++)
                {
                    FullDataPath1.Add(new Song(FullDataPath[i]));
                }
                Composititons.ItemsSource = FullDataPath1;
            }
        }

        private void Composititons_Selected(object sender, RoutedEventArgs e)
        {
            NAudioEngine.Instance.OpenFile(FullDataPath1[Composititons.SelectedIndex].OriginalPath);
            NAudioEngine.Instance.Play();
        }
    }
}
