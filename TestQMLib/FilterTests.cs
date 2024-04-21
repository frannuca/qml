using System.Globalization;
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
        int N = 150;
        double fs = 1;
        double dt = 1 / fs;
        var time = Enumerable.Range(0, N).Select(n => dt * n);
        var startDate = new DateTime(2023, 1, 1);
        var dates = time.Select(t => startDate.AddDays(t));
        
        double f1 = 1.0/5.0;
        double f2 = 1.0/60;
        var signal = time
            .Select(t => 
                0.2*Math.Sin( 2.0 * Math.PI * t * f1 )+
                Math.Sin( 2.0 * Math.PI * t * f2 ));

        double[] signalTimeData = signal as double[] ?? signal.ToArray();
        var fftSignal = FftCalculator.ComputeFft(signalTimeData.ToArray())?.ToArray() 
                        ?? throw new ArgumentNullException("new FFTCalculator().ComputeFFT(enumerable.ToArray())");
        fftSignal = fftSignal.Take(N / 2).ToArray();
        var filtered = LowPassFilter.Filter(signalTimeData.ToArray(), 1.0/10.0, fs, 5);
        ;
        // Create frequency array
        double[] freqs = Enumerable.Range(0, N/2).Select(n => n*fs/N ).ToArray();
        {
            // Plot the FFT magnitude vs frequency
            var fftPlot = new Scattergl
            {
                x = freqs,
                y = fftSignal.Select(x =>x.Norm()),
                mode = "lines",
                name = "FFT"
            };
        
            var layout = new Layout.Layout(){title="FFT Magnitude vs Frequency"};
            var chart = Chart.Plot(new[] { fftPlot });
            chart.WithLayout(layout);
            chart.Show();
        }
        {
            // Plot the FFT magnitude vs frequency
            var fftPlot = new Scattergl
            {
                x = time.ToArray(),
                y = signal,
                mode = "lines",
                name = "Time Domain Signal"
            };
        
            var fftPlot2 = new Scattergl
            {
                x = time.ToArray(),
                y = filtered,
                mode = "lines",
                name = "Filtered"
            };
            var layout = new Layout.Layout(){title="Signal Magnitude vs Time"};
            var chart = Chart.Plot(new[] { fftPlot,fftPlot2 });
            chart.WithLayout(layout);
            chart.Show();
        }
        //Console.ReadLine();
        
    }
    
}