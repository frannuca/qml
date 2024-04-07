namespace qmlib.signal;

public interface IFilter
{
    public double[] Filter(double[] x, double lowFreqCut, double? highFreqCut,double fs);
}