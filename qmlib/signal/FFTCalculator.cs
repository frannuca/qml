using MathNet.Numerics.IntegralTransforms;

namespace qmlib.signal;

using MathNet.Numerics;
using System.Numerics;

public class FFTCalculator
{
    public Complex[] ComputeFFT(double[] data)
    {
        var complexData = new Complex[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            complexData[i] = new Complex(data[i], 0.0);
        }

        Fourier.Forward(complexData, FourierOptions.Matlab);

        return complexData;
    }
}