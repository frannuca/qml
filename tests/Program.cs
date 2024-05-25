using System;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Accord;
using Accord.Math;
using Accord.Statistics;
using Deedle;
using Deedle.Vectors;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using qml.quoteDownloader;
using qmlib.signal;
using XPlot.Plotly;
using qmlib.measures;
using qmlib.portfolio;
using Index = System.Index;

public class Program
{

    static IDictionary<DateTime,Series<string,double>> ComputeSignal(Frame<DateTime, string> portfolioSeries, int nWindow,
        (int shortWindow, int longWindow, double weight)[] filters)
    {
        var signalCalc = new SignalCalculator(portfolioSeries);
        return signalCalc.Run(filters, nWindow);
    }
    

public static async Task Setup()
{
    const int n = 10 * 250;
        Frame<DateTime, string> portfolioSeries;
        Series<DateTime, double> DJI = await YahooFinanceDownloader.DownloadTimeSeriesData("^DJI", DateTime.Now.AddDays(-n), DateTime.Now);
        Series<DateTime, double> DAX = await YahooFinanceDownloader.DownloadTimeSeriesData("DAX", DateTime.Now.AddDays(-n), DateTime.Now);
        Series<DateTime, double> FTSE = await YahooFinanceDownloader.DownloadTimeSeriesData("^FTSE", DateTime.Now.AddDays(-n), DateTime.Now);
        Series<DateTime, double> SPX = await YahooFinanceDownloader.DownloadTimeSeriesData("^SPX", DateTime.Now.AddDays(-n), DateTime.Now);
        Series<DateTime, double> BZ = await YahooFinanceDownloader.DownloadTimeSeriesData("BZ=F", DateTime.Now.AddDays(-n), DateTime.Now);
        Series<DateTime, double> CT = await YahooFinanceDownloader.DownloadTimeSeriesData("CT=F", DateTime.Now.AddDays(-n), DateTime.Now);
        
        
        portfolioSeries = Frame.FromColumns([
            new KeyValuePair<string, Series<DateTime,double>>("DJI", DJI.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("DAX", DAX.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("FTSE", FTSE.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("BZ=F", BZ.DropMissing()),
            new KeyValuePair<string, Series<DateTime,double>>("CT", CT.DropMissing()),
            new KeyValuePair<string, Series<DateTime,double>>("SPX", SPX.DropMissing())]);
        portfolioSeries = (portfolioSeries/portfolioSeries.Shift(1) - 1.0).FillMissing(Direction.Forward).FillMissing(Direction.Backward);
        portfolioSeries.SaveCsv("/Users/fran/Downloads/portfolio.csv",includeRowKeys:true);
        int NWindow = 100;
        var filter_windows = new []{(5, 10), (5, 20),(10, 30),(5, 30)};
        var popt = new PortfolioOptimizer(new PortfolioOptimizationData()
        {
            portfolioSeries = portfolioSeries,
            nCovarianceWindow = 80,
            singleFilterWindow = NWindow,
            tragetVolatility = 0.07,
            Filters = filter_windows
        });
        var weights = popt.Run();
        /*double[] weights =
        [
            1.0005140577789464, 0.7762156546621752, 0.30823891112033225, 0.5021794599025414, 0.7710478220931477,
            0.21434077816751823, 0.7551946345508074
        ];*/
        var filterweights = filter_windows.Zip(weights)
            .Select(kv => (kv.First.Item1, kv.First.Item2, kv.Second)).ToArray();
        var modulationSignal = ComputeSignal(portfolioSeries,NWindow,filterweights);
        var pcalc = new PortfolioCalculator("MinVarianceRb");
        var results = pcalc.Run(portfolioSeries, 80, 0.07,modulationSignal);
        var frame = Frame.FromRows(results.Select(r => r.ToSeries())).IndexRowsWith(results.Select(l => l.Date));
        var modulationFrame = Frame.FromRows(modulationSignal.ToArray());
        var totalFrame = frame.Join(modulationFrame,JoinKind.Outer);
        string filepath = "/Users/fran/Downloads/portfolioResults.csv";
        var EQW = portfolioSeries.Transpose().Sum()/portfolioSeries.ColumnCount;
        totalFrame.AddColumn("EQW",EQW);
        totalFrame.SaveCsv(filepath,includeRowKeys:true);
        
    }
    public static async Task Main()
    {
        //await Setup();
        var data = await YahooFinanceDownloader.DownloadTimeSeriesData("SPY", DateTime.Now.AddDays(-1000), DateTime.Now);
        data = data - data.Mean();
        data.DropMissing();
        var scaling = 10.0;

        int shortWindow = 10;
        int longWindow = 40;
        // compute signal:
        var signal_60_20 = LowPassFilter.Filter(data, 0.5/shortWindow, 1.0, 11)
                                                - LowPassFilter.Filter(data, 0.5/longWindow, 1.0, 11);
        
        
        var signal_60_20_MA = CrossMovingAverage.ComputeCrossMovingAverage(data, shortWindow, longWindow);
        
        // Define the signal parameters
        int N = data.KeyCount; // Number of samples
        double fs = 1.0; // Sampling frequency
        double dt = 1.0 / fs; // Time step
       
        // Generate the signal
        var spectral = FftCalculator.ComputeFft(data, fs);
        
        var original = new Scattergl
        {
            x = data.Keys.ToArray(),
            y = data.Values.ToArray(),
            mode = "lines",
            name = "Orignal Signal"
        };

        var transformation = new Scattergl
        {
            x = spectral.Keys.Take(N/2).ToArray(),
            y = spectral.Values.Take(N/2).Select(v => v.Magnitude).ToArray(),
            mode = "lines",
            name = "spectrum Signal"
        };
        
        var signal = new Scattergl
        {
            x = signal_60_20.Keys.ToArray(),
            y = signal_60_20.Values.Take(N).ToArray(),
            mode = "lines",
            name = "Signal Filter"
        };
        
        
        var signala = new Scattergl
        {
            x = signal_60_20_MA.Keys.ToArray(),
            y = signal_60_20_MA.Values.Take(N).ToArray(),
            mode = "lines",
            name = "Signal CMA"
        };
        var layout = new Layout.Layout(){title="Time Domain"};
        var chart = Chart.Plot(new[] { signal,signala,original});
        
        var plot = Chart.Plot(new[] { transformation});
        var layout2 = new Layout.Layout(){title="Spectrum Magnitude"};
        var chart2 = Chart.Plot(new[] { transformation});
        
        chart.WithLayout(layout2);
        chart.Show();
        
        chart2.WithLayout(layout2);
        chart2.Show();
        
        
    }
}