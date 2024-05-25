using Deedle;

namespace qmlib.signal;

public static class CrossMovingAverage
{
    public static Series<DateTime, double> MovingAverages(this Series<DateTime, double> series, int w)
    {
        return series.Window(w).Select(window => { return window.Value.Values.Average(); });
    }

    public static Series<DateTime, double> ComputeCrossMovingAverage(Series<DateTime, double> series, int w1, int w2)
    {
        if (series.IsEmpty || series.KeyCount < Math.Max(w1, w2))
            throw new ArgumentException("Series does not have enough data points for the specified window size.");

        return (series.MovingAverages(w1) - series.MovingAverages(w2)).DropMissing();
    }
}