using System.Globalization;
using Accord;
using Deedle;
using MathNet.Numerics;
using qmlib.signal;
using XPlot.Plotly;
using static System.String;

namespace TestQMLib;

public class FilterTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        int N = 1000;
        double fs = 1;
        double dt = 1 / fs;
        var time = Enumerable.Range(0, N).Select(n => dt * n);
        
        double f1 = 1.0/5.0;
        double f2 = 1.0/60;
        var signal = time
            .Select(t => 
                0.2*Math.Sin( 2.0 * Math.PI * t * f1 )+
                Math.Sin( 2.0 * Math.PI * t * f2 ));

        var signalTimeData = new Series<double,double>(time,  signal as double[] ?? signal.ToArray());
        var fftSignal = FftCalculator.ComputeFft(signalTimeData, fs); 
        
        var filtered = LowPassFilter.Filter(signalTimeData, 0.05, fs, 3);
        ;
        // Create frequency array
        
        {
            // Plot the FFT magnitude vs frequency
            var original = new Scattergl
            {
                x = signalTimeData.Keys.ToArray(),
                y = signalTimeData.Values.ToArray(),
                mode = "lines",
                name = "Original"
            };
            
            var filteredplot = new Scattergl
            {
                x = filtered.Keys.ToArray(),
                y = filtered.Values.ToArray(),
                mode = "lines",
                name = "Filtered"
            };
        
            var layout = new Layout.Layout(){title="Time signals"};
            var chart = Chart.Plot(new[] { original, filteredplot});
            chart.WithLayout(layout);
            chart.Show();
        }
        {
            // Plot the FFT magnitude vs frequency
            var fftPlot = new Scattergl
            {
                x = fftSignal.Keys.ToArray(),
                y = fftSignal.Values.Select(x =>x.Magnitude),
                mode = "lines",
                name = "Time Domain Signal"
            };
        
           
            var layout2 = new Layout.Layout(){title="Signal Spectrum Magnitude"};
            var chart2 = Chart.Plot(new[] { fftPlot });
            chart2.WithLayout(layout2);
            chart2.Show();
        }
        //Console.ReadLine();
        
    }
    
}