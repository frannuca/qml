namespace qmlib.optimizers.convex;

using Accord.Math.Optimization;
using Accord.Math.Differentiation;
using System;

public class AugmentedLagrangianOptimizer
{
    private readonly AugmentedLagrangian _optimizer;
    private double[] _initialGuess = [];

    public AugmentedLagrangianOptimizer(
        Func<double[], double> fitness,
        (double lowerLimit, double upperLimit)[] boundaries)
    {
        var numberOfVariables = boundaries.Length;
        // Now, we create an optimizer
        var g = FiniteDifferences.Gradient(fitness, numberOfVariables, 1);
        
        var fFitness = new NonlinearObjectiveFunction(numberOfVariables, fitness, gradient:(double[] x) => g(x));
        IConstraint[] constraints = boundaries.SelectMany<(double lowerLimit, double upperLimit),IConstraint>((bound, idx) =>
        {
            IConstraint a = new NonlinearConstraint(
                fFitness,
                (double[] x) => x[idx],
                ConstraintType.GreaterThanOrEqualTo,
                bound.lowerLimit,
                gradient:x => Enumerable.Range(0,numberOfVariables).Select(n => n==idx?1.0:0.0).ToArray());
            IConstraint b = new NonlinearConstraint(
                fFitness,
                (double[] x) => x[idx],
                ConstraintType.LesserThanOrEqualTo,
                bound.upperLimit,
                gradient:x => Enumerable.Range(0,numberOfVariables).Select(n => n==idx?1.0:0.0).ToArray());

            return [a,b];
        }).ToArray();
        _optimizer=new AugmentedLagrangian(fFitness, constraints);
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