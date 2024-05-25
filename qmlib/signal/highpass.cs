using Deedle;

namespace qmlib.signal;

public static class HighPassFilter
{
    public static Series<TKey, double> Filter<TKey>(Series<TKey, double> x, double freqCutoff, double fs, int order)
        where TKey : IComparable, IComparable<TKey>
    {
        var filter = HighPassButterworthFilter(x.Values.ToArray(), freqCutoff, fs, order);

        return new Series<TKey, double>(x.Keys, filter);
    }

    public static double[] HighPassButterworthFilter(double[] indata, double cutOffFreq, double fs, int order)
    {
        var n = indata.Length;
        var outdata = new double[n];

        var wc = Math.Tan(Math.PI * cutOffFreq / fs);
        var k1 = Math.Sqrt(2) * wc;
        var k2 = wc * wc;
        var a = k2 / (1 + k1 + k2);
        var b = 2 * a;
        var c = a;
        var k3 = b / k2;
        var d = -2 * a + k3;
        var e = 1 - 2 * a - k3;

        for (var i = 0; i < n; i++)
            if (i >= 2)
                outdata[i] = a * indata[i] + b * indata[i - 1] + c * indata[i - 2] + d * outdata[i - 1] +
                             e * outdata[i - 2];
            else
                outdata[i] = indata[i];

        return outdata;
    }
}