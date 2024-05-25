using Accord.Math.Differentiation;
using Accord.Math.Optimization;

namespace qmlib.optimizers.convex;

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
        var g = FiniteDifferences.Gradient(fitness, numberOfVariables);

        var fFitness = new NonlinearObjectiveFunction(numberOfVariables, fitness, x => g(x));
        var constraints = boundaries.SelectMany<(double lowerLimit, double upperLimit), IConstraint>((bound, idx) =>
        {
            IConstraint a = new NonlinearConstraint(
                fFitness,
                x => x[idx],
                ConstraintType.GreaterThanOrEqualTo,
                bound.lowerLimit,
                x => Enumerable.Range(0, numberOfVariables).Select(n => n == idx ? 1.0 : 0.0).ToArray());
            IConstraint b = new NonlinearConstraint(
                fFitness,
                x => x[idx],
                ConstraintType.LesserThanOrEqualTo,
                bound.upperLimit,
                x => Enumerable.Range(0, numberOfVariables).Select(n => n == idx ? 1.0 : 0.0).ToArray());

            return [a, b];
        }).ToArray();
        _optimizer = new AugmentedLagrangian(fFitness, constraints);
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