using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kova.NAudioCore
{
     public interface ISpectrumPlayer : INotifyPropertyChanged
     {
         bool IsPlaying { get; }

         bool GetFFTData(float[] fftDataBuffer);

         int GetFFTFrequencyIndex(int frequency);
     }
}
