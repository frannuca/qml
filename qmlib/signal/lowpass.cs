namespace qmlib.signal;

using MathNet.Filtering;
using qmlib.signal;
using System;

public static class LowPassFilter
{
    public static double[] Filter(double[] x, double freqCutoff, double fs, int order)
    {
        var filter = OnlineFilter.CreateLowpass(ImpulseResponse.Finite,fs, freqCutoff,order);
        var y = filter.ProcessSamples(x);
        return y;
    }
}