using Accord;
using Accord.Statistics;
using Accord.Statistics.Kernels;
using qmlib.measures;
using Deedle;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace qmlib.portfolio;

public readonly record struct PortfolioOptimizationResult(
    DateTime Date,
    Series<string, double> Weights,
    Series<string, double> RiskContributions,
    double PnL)
{
    public Series<string,object> ToSeries()
    {
        try
        {
            var (date, weights, rc, pnl) = this;
            var wkv = weights.Keys.Select(c => new KeyValuePair<string,object>($"{c}_Weight", weights[c]));
            var rckv = rc.Keys.Select(c => new KeyValuePair<string,object>($"{c}_RC", rc[c]));
            var pnlkv = new []{new KeyValuePair<string,object>("PnL", pnl)};
            var row = new List<KeyValuePair<string,object>>();
            row.Add(new KeyValuePair<string, object>("Date",date));
            row.AddRange(wkv);
            row.AddRange(rckv);
            row.AddRange(pnlkv);
            var rowSeries = new Series<string,object>(row);
            var frame = Frame.FromRows([rowSeries]);
            frame.IndexRows<DateTime>("Date");
            return frame.GetRowAt<object>(0);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}

public class PortfolioCalculator(string riskMeasureName)
{
    private (int idxdate,Matrix<double>Cov)[] ComputeCovariances(Frame<DateTime, string> portfolioSeries,int nCovarianceWindow)
    {
        var m = Matrix<double>.Build.DenseOfArray(portfolioSeries.ToArray2D<double>());
        
        var N = nCovarianceWindow;
        var covariances = new List<(int idx,Matrix<double> cov)>();
        if (covariances == null) throw new ArgumentNullException(nameof(covariances));
        for(int i=N;i<m.RowCount;i++)
        {
            var date = portfolioSeries.GetRowKeyAt(i);
            var subMatrixAtT = m.SubMatrix(i-N,N,0,m.ColumnCount);
            var ct = subMatrixAtT.ToArray().Covariance();
            var xC = Matrix<double>.Build.DenseOfArray(ct);
            covariances.Add((i,xC));
        }
        return covariances.ToArray();
    }
    
    public IEnumerable<PortfolioOptimizationResult> Run(Frame<DateTime, string> portfolioSeries, int nCovarianceWindow, double targetVol,
        IDictionary<DateTime, Series<string, double>>? weightModulationSignal=null)
    {
        var rng = new ContinuousUniform(0, 1);
        int n = portfolioSeries.ColumnCount;
        var w0 = new double[n];
        rng.Samples(w0);
        
        
        var covariances = ComputeCovariances(portfolioSeries,nCovarianceWindow);
        var results = new List<PortfolioOptimizationResult>();
        foreach (var (idate,c) in covariances)
        {
            var date = portfolioSeries.GetRowKeyAt(idate - 1);
            var datepnl = portfolioSeries.GetRowKeyAt(idate);
            Console.WriteLine($"Computing {date}");
            var rm = RiskMeasureFactory.CreateRiskMeasure(riskMeasureName,c);
            var str = c.ToString();
            var dailyvol=targetVol/Math.Sqrt(250);
            var b = Vector<double>.Build.DenseOfArray( Enumerable.Range(0, n).Select(_ => 1.0/ n).ToArray());
            var xsol = rm.OptimizeWeigts(Vector<double>.Build.DenseOfArray(w0), c, b);
            
            
            if(weightModulationSignal is not null && weightModulationSignal.TryGetValue(date, out var signal))
            {
                var xsignal = Vector<double>.Build.DenseOfArray(portfolioSeries.ColumnKeys.Select(c => signal[c]).ToArray());
                xsol  = xsol.PointwiseMultiply(xsignal);
            }
            var rc = rm.RiskContributions(xsol);
            var xxsol = new Series<string, double>(portfolioSeries.ColumnKeys, xsol);
            
            var scale = dailyvol / (rc.Sum()+1e-10);
            xxsol *=scale;
            var xrc = new Series<string,double>(portfolioSeries.ColumnKeys,rc);
            var rcError = rm.RiskContributionError(xsol, b);
            
            var ret = portfolioSeries.Rows[datepnl].As<double>();
            var pnl = (ret * xxsol).Sum();
            results.Add(new PortfolioOptimizationResult(datepnl,xxsol,xrc,pnl));
        }

        return results.ToArray();
    }
}