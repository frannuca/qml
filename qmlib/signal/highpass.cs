using Deedle;

namespace qmlib.signal;

using MathNet.Filtering;
using qmlib.signal;
using System;

public static class HighPassFilter
{
    public static Series<TKey,double> Filter<TKey>(Series<TKey,double>  x, double freqCutoff, double fs, int order)
     where TKey : IComparable, IComparable<TKey>
    {
        var filter = HighPassFilter.HighPassButterworthFilter(x.Values.ToArray(), freqCutoff, fs, order);
       
        return new Series<TKey, double>(x.Keys,filter);
    }
    public static double[] HighPassButterworthFilter(double[] indata, double cutOffFreq, double fs, int order)
    {
        int n = indata.Length;
        double[] outdata = new double[n];

        double wc = Math.Tan(Math.PI * cutOffFreq / fs);
        double k1 = Math.Sqrt(2) * wc;
        double k2 = wc * wc;
        double a = k2 / (1 + k1 + k2);
        double b = 2 * a;
        double c = a;
        double k3 = b / k2;
        double d = -2 * a + k3;
        double e = 1 - (2 * a) - k3;

        for (int i = 0; i < n; i++)
        {
            if (i >= 2)
            {
                outdata[i] = a * indata[i] + b * indata[i - 1] + c * indata[i - 2] + d * outdata[i - 1] + e * outdata[i - 2];
            }
            else
            {
                outdata[i] = indata[i];
            }
        }

        return outdata;
    }
}