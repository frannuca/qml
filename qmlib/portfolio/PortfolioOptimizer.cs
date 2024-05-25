using Accord;
using Deedle;
using MathNet.Numerics.LinearAlgebra;
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
   
    double Fitness(double[] x)
    {
        var filterWeights = x.ToArray();
        
        var signalCalc = new SignalCalculator(data.portfolioSeries);
        var signal = signalCalc
            .Run(data.Filters.Select((y,i) => (y.shortWindoe, y.longWindow, filterWeights[i])).ToArray(), data.singleFilterWindow);
        var framesignal = Frame.FromRows(signal);
        
        var pnlseries = data.portfolioSeries.Shift(1)*framesignal;
        var pnl2 = pnlseries.FillMissing(0).GetColumns<double>().Values.Aggregate((a,b)=>a+b);
        var pnl = pnl2.Values.ToArray();
        var pnlfinal = pnl.Sum();
        var pnl0 = pnl[0];
        var r = pnlfinal - pnl0;
        var xstr = String.Join(',', x);
        //Console.WriteLine($"{xstr} ->"+r);
        return -r;

    }

    public double[] Run()
    {
        var pGaParams = new GaParams(PopulationSize: 10, CrossoverRate: 0.8, MutationRate: 0.1,
            RandomSelectionPortion: 0.1);
        
        var bounds = new List<(double,double)>();
        var fw = data.Filters.Select(_ => (-1.0, 1.0));
        bounds.AddRange(fw);
        
        var ga = new GaOpt(pGaParams, bounds.ToArray(), Fitness);

        return ga.Fit(50,0.01,3);
    }
}