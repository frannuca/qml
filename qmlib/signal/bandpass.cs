using Deedle;
using MathNet.Filtering;

namespace qmlib.signal;

public static class BandPassFilter
{
    public static Series<DateTime, double> Filter(Series<DateTime, double> indata, double lowcut, double highcut,
        double fs, int order)
    {
        //narrow bandpass filter
        var fc1 = lowcut; //low cutoff frequency
        var fc2 = highcut; //high cutoff frequency
        var bandpassnarrow = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, fs, fc1, fc2);
        var filter = bandpassnarrow.ProcessSamples(indata.Values.ToArray());
        return new Series<DateTime, double>(indata.Keys, filter);
    }
}