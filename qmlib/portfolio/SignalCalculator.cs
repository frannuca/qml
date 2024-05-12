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
    private double quantifyScaling(double x)
    {
        if (x > l95)
            return 2.0;
        if (x > l9)
             return 1.5;
        if (x > l8)  
            return 1.1;
        if (x < -l95)
        {
            return -0.5;
        }
        if(x < -l9)
            return -0.25;
        
        return 1.0;
    }
    public IDictionary<DateTime,Series<string,double>> Run(IEnumerable<(int shortWindow, int longWindow, double weight)> filters, int nWindow)
    {
        
        
        var fs = 1.0;
        var order = 3;
        var fsignal = (Series<DateTime, double> data, double low, double hi) =>
        {
            var r = LowPassFilter.Filter(data, 1.0/low, fs, order) -
                    LowPassFilter.Filter(data, 1.0/hi, fs, order);

            return r;
        };
            
        
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
                    .Select(x => x.weight * fsignal(signal, x.shortWindow, x.longWindow))
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
                        var s = new KeyValuePair<string, double>(y.columnName, quantifyScaling(y.Item3));
                        return s;
                    }
                   )));
    }
}