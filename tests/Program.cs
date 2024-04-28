using System;
using System.Data.Common;
using System.Linq;
using Accord;
using Accord.Math;
using Deedle;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using qml.quoteDownloader;
using qmlib.signal;
using XPlot.Plotly;

public class Program
{
    public async static Task Main()
    {
        
        var data = await YahooFinanceDownloader.DownloadTimeSeriesData("SPY", DateTime.Now.AddDays(-1000), DateTime.Now);
        data = data - data.Mean();
        data.DropMissing();
        
        // compute signal:
        var signal_60_20 = LowPassFilter.Filter(data, 1.0/20.0, 1.0, 5)
                                             - LowPassFilter.Filter(data, 1.0/60.0, 1.0, 5);
        
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
            name = "Signal 60-20"
        };
        
        var layout = new Layout.Layout(){title="Time Domain"};
        var chart = Chart.Plot(new[] { signal,original});
        
        var plot = Chart.Plot(new[] { transformation});
        var layout2 = new Layout.Layout(){title="Spectrum Magnitude"};
        var chart2 = Chart.Plot(new[] { transformation});
        
        chart.WithLayout(layout2);
        chart.Show();

        chart2.WithLayout(layout2);
        chart2.Show();
        
        
    }
}