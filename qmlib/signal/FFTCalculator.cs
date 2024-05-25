using System.Numerics;
using Deedle;
using MathNet.Numerics.IntegralTransforms;

namespace qmlib.signal;

public static class FftCalculator
{
    public static Series<double, Complex> ComputeFft<TKey>(Series<TKey, double> series, double fs)
    {
        var data = series.Values.ToArray();
        var complexData = new Complex[data.Length];
        for (var i = 0; i < data.Length; i++) complexData[i] = new Complex(data[i], 0.0);

        var freqs = Enumerable.Range(0, data.Length).Select(n => n * fs / data.Length).ToArray();
        Fourier.Forward(complexData, FourierOptions.Matlab);

        return new Series<double, Complex>(freqs, complexData);
    }
}