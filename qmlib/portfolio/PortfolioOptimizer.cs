using Accord;
using Deedle;
using qmlib.optimizers.GA;

namespace qmlib.portfolio;

public class PortfolioOptimizationData
{
    public Frame<DateTime, string> portfolioSeries;
    public int nCovarianceWindow;
    public int singleFilterWindow;
    public double tragetVolatility;
    public (int shortWindoe, int longWindow)[] Filters;
}
public class PortfolioOptimizer(PortfolioOptimizationData data)
{
    double fitness(double[] x)
    {
        var weights = x.Take(data.portfolioSeries.ColumnCount).ToArray();
        var filterWeights = x.Skip(data.portfolioSeries.ColumnCount).ToArray();
        
        var signalCalc = new SignalCalculator(data.portfolioSeries);
        var signal = signalCalc
            .Run(data.Filters.Select((y,i) => (y.shortWindoe, y.longWindow, filterWeights[i])).ToArray(), data.singleFilterWindow);
        
        var pcalc = new PortfolioCalculator("MinVarianceRb");
        var results = pcalc.Run(data.portfolioSeries, data.nCovarianceWindow, data.tragetVolatility,signal);

        var pnlfinal = results.Select(x => x.PnL).Sum();
        var pnl0 = -results.First().PnL;
        var r = pnlfinal - pnl0;
        var xstr = String.Join(',', x);
        Console.WriteLine($"{xstr} ->"+r);
        return -r;

    }

    public double[] Run()
    {
        var pGaParams = new GaParams(PopulationSize: 20, CrossoverRate: 0.8, MutationRate: 0.1,
            RandomSelectionPortion: 0.1);
        
        var bounds = new List<(double,double)>();
        var w = data.portfolioSeries.ColumnKeys.Select(_ => (0.001, 100.0));
        bounds.AddRange(w);
        var fw = data.Filters.Select(_ => (0.001, 1.0));
        bounds.AddRange(fw);
        
        var ga = new GaOpt(pGaParams, bounds.ToArray(), fitness);

        return ga.Fit(50,0.01,int.MaxValue);
    }
}