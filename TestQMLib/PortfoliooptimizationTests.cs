using Accord.Statistics;
using Accord.Statistics.Kernels;
using Deedle;
using MathNet.Numerics.LinearAlgebra;
using qml.quoteDownloader;
using Accord.Statistics;
using MathNet.Numerics.Distributions;
using qmlib.measures;

namespace TestQMLib;

public class PortfoliooptimizationTests
{
    private Series<DateTime, double> IRX,FVX,TNX,TYX;
    private Frame<DateTime, string> portfolioSeries;
    [SetUp]
    public async Task Setup()
    {
        IRX = await YahooFinanceDownloader.DownloadTimeSeriesData("^IRX", DateTime.Now.AddDays(-1000), DateTime.Now);
        FVX = await YahooFinanceDownloader.DownloadTimeSeriesData("^FVX", DateTime.Now.AddDays(-1000), DateTime.Now);
        TNX = await YahooFinanceDownloader.DownloadTimeSeriesData("^TNX", DateTime.Now.AddDays(-1000), DateTime.Now);
        TYX = await YahooFinanceDownloader.DownloadTimeSeriesData("^TYX", DateTime.Now.AddDays(-1000), DateTime.Now);
        
        
        portfolioSeries = Frame.FromColumns([
            new KeyValuePair<string, Series<DateTime,double>>("IRX", IRX.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("FVX", FVX.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("TNX", TNX.DropMissing()), 
            new KeyValuePair<string, Series<DateTime,double>>("TYX", TYX.DropMissing())]);
        portfolioSeries = portfolioSeries.Diff(1).FillMissing(Direction.Forward);
        portfolioSeries.SaveCsv("/Users/fran/Downloads/portfolio.csv",includeRowKeys:true);
    }

    [Test]
    public void Test1()
    {
        var m = Matrix<double>.Build.DenseOfArray(portfolioSeries.ToArray2D<double>());
        
        var N = 80;
        var covariances = new List<Matrix<double>>();
        if (covariances == null) throw new ArgumentNullException(nameof(covariances));
        for(int i=N;i<m.RowCount;i++)
        {
            var subMatrixAtT = m.SubMatrix(i-N,N,0,m.ColumnCount);
            var ct = subMatrixAtT.ToArray().Covariance();
            var xC = Matrix<double>.Build.DenseOfArray(ct);
            covariances.Add(xC);
        }

        foreach (var c in covariances)
        {
            var rm = new MinVarianceRb(c);
            var rng = new ContinuousUniform(0, 1);
            
            int n = c.ColumnCount;
            var w0 = new double[n];
            rng.Samples(w0);
            var b = Vector<double>.Build.DenseOfArray( Enumerable.Range(0, n).Select(_ => 10.0).ToArray());
            var xsol = rm.OptimizeWeigts(Vector<double>.Build.DenseOfArray(w0), c, b);
            var rc = rm.RiskContributions(xsol);
            var rcError = rm.RiskContributionError(xsol, b);
        }
        
        


    }
}