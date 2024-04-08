namespace qmlib.signal;

using MathNet.Filtering;
using qmlib.signal;
using System;

public static class HighPassFilter
{
    public static double[] Filter(double[] x, double freqCutoff, double fs, int order)
    {
        var filter = OnlineFilter.CreateHighpass(ImpulseResponse.Finite,fs, freqCutoff,order);
        var y = filter.ProcessSamples(x);
        return y;
    }
}