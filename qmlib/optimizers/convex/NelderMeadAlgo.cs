namespace qmlib.optimizers.convex;

using Accord.Math.Optimization;
using Accord.Math.Differentiation;
using System;

public class NelderMeadOptimizer
{
    private readonly NelderMead _optimizer;

    private double[] _initialGuess = [];

    public NelderMeadOptimizer(
        Func<double[], double> fitness,
        (double lowerLimit, double upperLimit)[] boundaries)
    {
        var numberOfVariables = boundaries.Length;
        _optimizer=new NelderMead(numberOfVariables, fitness);
        for(int i=0;i<boundaries.Length;i++)
        {
            _optimizer.LowerBounds[i] = boundaries[i].lowerLimit;
            _optimizer.UpperBounds[i] = boundaries[i].upperLimit;
        }
    }
    
    public void SetInitialGuess(double[] initialGuess)
    {
        this._initialGuess= initialGuess;
    }

    public double[] Optimize()
    {
        bool success = this._initialGuess.Length > 0 ? _optimizer.Minimize(this._initialGuess): _optimizer.Minimize();
        if (!success)
        {
            throw new Exception("Optimization failed.");
        }

        return _optimizer.Solution;
    }
}