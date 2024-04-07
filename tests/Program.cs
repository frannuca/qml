using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using XPlot.Plotly;

public class Program
{
    public static void Main()
    {
        // Define the signal parameters
        int N = 1000;
        double fs = 200.0; // Sampling frequency
        double f1 = 50.0; // Signal frequency
        double f2 = 10.0; // Signal frequency

        // Generate the signal
        var t = Enumerable.Range(0, N).Select(n => n / fs).ToArray(); // Time vector
        var signal = t.Select(time => Math.Sin(2.0 * Math.PI * f1 * time)+0.5*Math.Sin(2.0 * Math.PI * f2 * time)).ToArray(); // Signal

        // Compute the FFT
        var fft = new Complex32[signal.Length];
        for (int i = 0; i < signal.Length; i++)
        {
            fft[i] = new Complex32((float)signal[i], 0.0f);
        }
        Fourier.Forward(fft, FourierOptions.Matlab);
        fft = fft.Take(fft.Length / 2).ToArray();

        // Compute the frequencies for the FFT bins
        var freqs = Enumerable.Range(0, N).Select(n => n * fs / N).ToArray();

        // Compute the magnitude of the FFT
        var magnitude = fft.Select(x => x.Magnitude).ToArray();

        // Plot the FFT magnitude vs frequency
        var fftPlot = new Scattergl
        {
            x = freqs,
            y = magnitude,
            mode = "lines",
            name = "FFT"
        };

        var layout = new Layout.Layout(){title="FFT Magnitude vs Frequency"};
        var chart = Chart.Plot(new[] { fftPlot });
        chart.WithLayout(layout);
        chart.Show();
        Console.ReadLine();
    }
}