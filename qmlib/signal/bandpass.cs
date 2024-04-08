using Deedle;

namespace qmlib.signal;

using MathNet.Filtering;
using qmlib.signal;
using System;

public static class BandPassFilter
{
    public static Series<DateTime,double> Filter(Series<DateTime,double> x, double lowFreqCut,double highFreqCut, double fs, int order)
    {
        var lcutoff = lowFreqCut/fs;
        var hcutoff = highFreqCut/fs;
        
        var filter = OnlineFilter.CreateBandpass(ImpulseResponse.Finite,1, lcutoff,hcutoff,order);
        var y = filter.ProcessSamples(x.Values.ToArray());
        var newseries = new Series<DateTime,double>(x.Keys,y);
        return newseries;
    }
}