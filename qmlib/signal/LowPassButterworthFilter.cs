namespace qmlib.signal;

using MathNet.Filtering;
using qmlib.signal;
using System;

public static class LowPassFilter
{
    public static double[] Filter(double[] x, double lowFreqCut, double fs, int order)
    {
        var cutoff = lowFreqCut ;/// (fs / 2); // Normalize the frequency
        var filter = OnlineFilter.CreateLowpass(ImpulseResponse.Finite,fs, cutoff,order);
        var y = filter.ProcessSamples(x);
        return y;
    }
}