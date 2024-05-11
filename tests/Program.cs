using System;
using System.Data.Common;
using System.Linq;
using Accord;
using Accord.Math;
using Accord.Statistics;
using Deedle;
using Deedle.Vectors;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using qml.quoteDownloader;
using qmlib.signal;
using XPlot.Plotly;
using qmlib.measures;
using qmlib.portfolio;

public class Program
{
    
    public async static Task Setup()
    {
        
        Frame<DateTime, string> portfolioSeries;
        Series<DateTime, double> DJI = await YahooFinanceDownloader.DownloadTimeSeriesData("^DJI", DateTime.Now.AddDays(-1000), DateTime.Now);
        Series<DateTime, double> DAX = await YahooFinanceDownloader.DownloadTimeSeriesData("DAX", DateTime.Now.AddDays(-1000), DateTime.Now);
        Series<DateTime, double> FTSE = await YahooFinanceDownloader.DownloadTimeSeriesData("^FTSE", DateTime.Now.AddDays(-1000), DateTime.Now);
        Series<DateTime, double> SPX = await YahooFinanceDownloader.DownloadTimeSeriesData("^SPX", DateTime.Now.AddDays(-1000), DateTime.Now);
        Series<DateTime, double> BZ = await YahooFinanceDownloader.DownloadTimeSeriesData("BZ=F", DateTime.Now.AddDays(-1000), DateTime.Now);
        Series<DateTime, double> CT = await YahooFinanceDownloader.DownloadTimeSeriesData("CT=F", DateTime.Now.AddDays(-1000), DateTime.Now);
        
        
        portfolioSeries = Frame.FromColumns([
            new KeyValuePair<string, Series<DateTime,double>>("DJI", DJI.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("DAX", DAX.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("FTSE", FTSE.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("BZ=F", BZ.DropMissing()),
            new KeyValuePair<string, Series<DateTime,double>>("CT", CT.DropMissing()),
            new KeyValuePair<string, Series<DateTime,double>>("SPX", SPX.DropMissing())]);
        portfolioSeries = (portfolioSeries/portfolioSeries.Shift(1) - 1.0).FillMissing(Direction.Forward).FillMissing(Direction.Backward);
        portfolioSeries.SaveCsv("/Users/fran/Downloads/portfolio.csv",includeRowKeys:true);
        
        var pcalc = new PortfolioCalculator("MinVarianceRb");
        var results = pcalc.Run(portfolioSeries, 80, 0.07);
        var frame = Frame.FromRows(results.Select(r => r.ToSeries()));
        
        string filepath = "/Users/fran/Downloads/portfolioResults.csv";
        frame.SaveCsv(filepath,includeRowKeys:true);
    }
    public async static Task Main()
    {

        await Setup();
        var data = await YahooFinanceDownloader.DownloadTimeSeriesData("SPY", DateTime.Now.AddDays(-1000), DateTime.Now);
        data = data - data.Mean();
        data.DropMissing();
        var scaling = 10.0;
        // compute signal:
        var signal_60_20a = BandPassFilter.Filter(data, 1.0/40,1.0/20, 1.0, 6)*scaling;
                                            // - LowPassFilter.Filter(data, 1.0/40.0, 1.0, 5);
        
        var signal_60_20 = BandPassFilter.Filter(signal_60_20a, 1.0/40.0,1.0/20.0, 1.0, 6);
                                               // - LowPassFilter.Filter(signal_60_20a, 1.0/40.0, 1.0, 5);

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
            name = "Signal 60-20 second derivatives"
        };
        var signala = new Scattergl
        {
            x = signal_60_20a.Keys.ToArray(),
            y = signal_60_20a.Values.Take(N).ToArray(),
            mode = "lines",
            name = "Signal 60-20 first derivatives"
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