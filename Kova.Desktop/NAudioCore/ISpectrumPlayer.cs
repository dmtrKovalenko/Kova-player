using System.ComponentModel;

namespace Kova.NAudioCore
{
     public interface ISpectrumPlayer : INotifyPropertyChanged
     {
         bool IsPlaying { get; }

         bool GetFFTData(float[] fftDataBuffer);

         int GetFFTFrequencyIndex(int frequency);
     }
}
