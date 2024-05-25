using Accord.Math.Optimization;

namespace qmlib.optimizers.convex;

public class NelderMeadOptimizer
{
    private readonly NelderMead _optimizer;

    private double[] _initialGuess = [];

    public NelderMeadOptimizer(
        Func<double[], double> fitness,
        (double lowerLimit, double upperLimit)[] boundaries)
    {
        var numberOfVariables = boundaries.Length;
        _optimizer = new NelderMead(numberOfVariables, fitness);
        for (var i = 0; i < boundaries.Length; i++)
        {
            _optimizer.LowerBounds[i] = boundaries[i].lowerLimit;
            _optimizer.UpperBounds[i] = boundaries[i].upperLimit;
        }
    }

    public void SetInitialGuess(double[] initialGuess)
    {
        _initialGuess = initialGuess;
    }

    public double[] Optimize()
    {
        var success = _initialGuess.Length > 0 ? _optimizer.Minimize(_initialGuess) : _optimizer.Minimize();
        if (!success) throw new Exception("Optimization failed.");

        return _optimizer.Solution;
    }
}