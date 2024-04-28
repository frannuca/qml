using Deedle;
using MathNet.Filtering;

namespace qmlib.signal;

public static class BandPassFilter
{
    public static Series<TKey, double> Filter<TKey>(Series<TKey, double> x, double lowFreqCut, double highFreqCut,
        double fs, int order) where TKey : IComparable, IComparable<TKey>
    {
        var filtered = BandPassButterworthFilter(x.Values.ToArray(),lowFreqCut, highFreqCut, fs, order);
        return new Series<TKey, double>(x.Keys, filtered);
    }
    
    public static double[] BandPassButterworthFilter(double[] indata, double lowcut, double highcut, double fs, int order)
    {
        int n = indata.Length;
        double[] outdata = new double[n];
        double[] filtered = new double[n];

        // Apply low-pass filter
        for (int i = 0; i < n; i++)
        {
            double wc = Math.Tan(Math.PI * lowcut / fs);
            double k1 = Math.Sqrt(2) * wc;
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;
            if (i >= 2)
            {
                outdata[i] = a * indata[i] + b * indata[i - 1] + c * indata[i - 2] + d * outdata[i - 1] + e * outdata[i - 2];
            }
            else
            {
                outdata[i] = indata[i];
            }
        }

        // Apply high-pass filter
        for (int i = 0; i < n; i++)
        {
            double wc = Math.Tan(Math.PI * highcut / fs);
            double k1 = Math.Sqrt(2) * wc;
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;
            if (i >= 2)
            {
                filtered[i] = a * outdata[i] + b * outdata[i - 1] + c * outdata[i - 2] + d * filtered[i - 1] + e * filtered[i - 2];
            }
            else
            {
                filtered[i] = outdata[i];
            }
        }

        return filtered;
    }
}