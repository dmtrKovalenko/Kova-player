using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Kova.NAudioCore;

namespace Kova.Views
{
    /// <summary>
    /// Логика взаимодействия для NowPlaying.xaml
    /// </summary>
    public partial class NowPlaying : UserControl
    {
        public NowPlaying()
        {
            InitializeComponent();

            spectrumAnalyzer1.RegisterSoundPlayer(NAudioEngine.Instance);
        }
    }
}
