using Accord;
using Accord.Statistics;
using Accord.Statistics.Kernels;
using qmlib.measures;
using Deedle;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace qmlib.portfolio;

public record struct PortfolioOptimizationResult(DateTime Date, Series<string,double> Weights, Series<string,double> RiskContributions, double PnL);

public class PortfolioCalculator(string riskMeasureName)
{
    private (DateTime date,Matrix<double>Cov)[] ComputeCovariances(Frame<DateTime, string> portfolioSeries,int nCovarianceWindow)
    {
        var m = Matrix<double>.Build.DenseOfArray(portfolioSeries.ToArray2D<double>());
        
        var N = nCovarianceWindow;
        var covariances = new List<(DateTime date,Matrix<double> cov)>();
        if (covariances == null) throw new ArgumentNullException(nameof(covariances));
        for(int i=N;i<m.RowCount;i++)
        {
            var date = portfolioSeries.GetRowKeyAt(i);
            var subMatrixAtT = m.SubMatrix(i-N,N,0,m.ColumnCount);
            var ct = subMatrixAtT.ToArray().Covariance();
            var xC = Matrix<double>.Build.DenseOfArray(ct);
            covariances.Add((date:date,xC));
        }
        return covariances.ToArray();
    }
    public IEnumerable<PortfolioOptimizationResult> Run(Frame<DateTime, string> portfolioSeries, int nCovarianceWindow, double targetVol)
    {
        var rng = new ContinuousUniform(0, 1);
        int n = portfolioSeries.ColumnCount;
        var w0 = new double[n];
        rng.Samples(w0);
        
        
        var covariances = ComputeCovariances(portfolioSeries,nCovarianceWindow);
        var results = new List<PortfolioOptimizationResult>();
        foreach (var (date,c) in covariances)
        {
            var rm = RiskMeasureFactory.CreateRiskMeasure(riskMeasureName,c);
            var str = c.ToString();
            var b = Vector<double>.Build.DenseOfArray( Enumerable.Range(0, n).Select(_ => targetVol/ n).ToArray());
            var xsol = rm.OptimizeWeigts(Vector<double>.Build.DenseOfArray(w0), c, b);
            var xxsol = new Series<string, double>(portfolioSeries.ColumnKeys, xsol);
            
            var rc = rm.RiskContributions(xsol);
            var xrc = new Series<string,double>(portfolioSeries.ColumnKeys,rc);
            var rcError = rm.RiskContributionError(xsol, b);
            
            var ret = portfolioSeries.Rows[date].As<double>();
            var pnl = (ret * xxsol).Sum();
            results.Add(new PortfolioOptimizationResult(date,xxsol,xrc,pnl));
        }

        return results;
    }
}