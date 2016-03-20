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
using System.ComponentModel;
using System.IO;

namespace Kova.Views
{
    /// <summary>
    /// Логика взаимодействия для NowPlaying.xaml
    /// </summary>
    public partial class NowPlaying : UserControl
    {
        NAudioEngine soundEngine;
        public NowPlaying()
        {
            InitializeComponent();

            //          soundEngine.PropertyChanged += NAudioEngine_PropertyChanged;

            soundEngine = NAudioEngine.Instance;
            spectrumAnalyzer1.RegisterSoundPlayer(soundEngine);
     //       OpenFile();
        }

        private void NAudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NAudioEngine engine = NAudioEngine.Instance;
            switch (e.PropertyName)
            {
                case "FileTag":
                    if (engine.FileTag != null)
                    {
                        TagLib.Tag tag = engine.FileTag.Tag;
                        if (tag.Pictures.Length > 0)
                        {
                            using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                            {
                                try
                                {
                                    BitmapImage albumImage = new BitmapImage();
                                    albumImage.BeginInit();
                                    albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                    albumImage.StreamSource = albumArtworkMemStream;
                                    albumImage.EndInit();
                                    albumArtPanel.AlbumArtImage = albumImage;
                                }
                                catch (NotSupportedException)
                                {
                                    albumArtPanel.AlbumArtImage = null;
                                    // System.NotSupportedException:
                                    // No imaging component suitable to complete this operation was found.
                                }
                                albumArtworkMemStream.Close();
                            }
                        }
                        else
                        {
                            albumArtPanel.AlbumArtImage = null;
                        }
                    }
                    else
                    {
                        albumArtPanel.AlbumArtImage = null;
                    }
                    break;
                default:
                    // Do Nothing
                    break;
            }

        }

        private void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "(*.mp3)|*.mp3";
            if (openDialog.ShowDialog() == true)
            {
                soundEngine.OpenFile(openDialog.FileName);
                //   FileText.Text = openDialog.FileName;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            soundEngine.Play();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            soundEngine.Pause();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Resources.MergedDictionaries.Clear();
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }


    }
}
