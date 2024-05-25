using Accord.Math;
using Accord.Statistics.Kernels;
using Deedle;
using MathNet.Numerics.LinearAlgebra;
using qmlib.signal;

namespace qmlib.portfolio;

public class SignalCalculator(Frame<DateTime, string> portfolioSeries)
{
    public Frame<DateTime, string> PortfolioSeries => portfolioSeries;
    double l95 = Normal.Inverse(0.95);
    double l9 = Normal.Inverse(0.9);
    double l8 = Normal.Inverse(0.8);
    private static double QuantifyScaling(double x)
    {
        var p = MathNet.Numerics.Distributions.Normal.CDF(0, 1, x);
        return p;
            
    }
    public static double ToPortfolioMultiplicationSignal(double s, double threshold, double multiplier)
    {
        return s;
    }
    public IDictionary<DateTime,Series<string,double>> Run(IEnumerable<(int shortWindow, int longWindow, double weight)> filters, int nWindow)
    {
        var fs = 1.0;
        var order = 11;

        Series<DateTime, double> Fsignal(Series<DateTime, double> data, double shortwindow, double longwindow)
        {
            var r = LowPassFilter.Filter(data, 1.0 / shortwindow, fs, order) 
                    - LowPassFilter.Filter(data, 1.0 / longwindow, fs, order);
            return r;
        }

        var alldates = portfolioSeries.RowKeys.ToList();
        var filtersArray = filters as (int shortWindow, int longWindow, double weight)[] ?? filters.ToArray();
        
        return Enumerable.Range(nWindow, portfolioSeries.RowCount-nWindow).SelectMany(i =>
        {
            if (i >= alldates.Count)
            {
                var datex = alldates.Last();
            }
            var dates = alldates.Where(d => d >= alldates[i - nWindow] && d <= alldates[i]);
            var dateTimes = dates as DateTime[] ?? dates.ToArray();
            var subframe = portfolioSeries.GetRows(Enumerable.ToArray(dateTimes));
            var date = dateTimes.First();
            return subframe.ColumnKeys.Select(columnName =>
            {
                var signal = subframe[columnName];
                var psignal = filtersArray
                    .Select(x => x.weight * Fsignal(signal, x.shortWindow, x.longWindow))
                    .Aggregate((x, y) => x + y);

                var std = psignal.StdDev();
                var mean = psignal.Mean();
                var zscore = (psignal - mean) / std;
                return (columnName, zscore.LastKey(), -zscore.LastValue());
            });
        }).GroupBy(x => x.Item2)
            .ToDictionary(x => x.Key, 
            x => 
                new Series<string,double>(x.Select(y =>
                    {
                        var s = new KeyValuePair<string, double>(y.columnName, QuantifyScaling(y.Item3));
                        return s;
                    }
                   )));
    }
}