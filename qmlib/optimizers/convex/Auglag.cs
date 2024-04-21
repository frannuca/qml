namespace qmlib.optimizers.convex;

using Accord.Math.Optimization;
using Accord.Math.Differentiation;
using System;

public class AugmentedLagrangianOptimizer
{
    private AugmentedLagrangian optimizer;
    private NonlinearObjectiveFunction fFitness;
    private IConstraint[] constraints;
    private readonly int numberOfVariables;
    private double[] initialGuess = Array.Empty<double>();

    public AugmentedLagrangianOptimizer(
        Func<double[], double> fitness,
        (double lowerLimit, double upperLimit)[] boundaries)
    {
        numberOfVariables = boundaries.Length;
        // Now, we create an optimizer
        var g = FiniteDifferences.Gradient(fitness, numberOfVariables, 1);
        
        fFitness = new NonlinearObjectiveFunction(numberOfVariables, fitness, gradient:(double[] x) => g(x));
        constraints = boundaries.SelectMany<(double lowerLimit, double upperLimit),IConstraint>((bound, idx) =>
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
        optimizer=new AugmentedLagrangian(fFitness, constraints);
    }
    
    public void SetInitialGuess(double[] initialGuess)
    {
        this.initialGuess= initialGuess;
    }

    public double[] Optimize()
    {
        bool success = this.initialGuess.Length > 0 ? optimizer.Minimize(this.initialGuess): optimizer.Minimize();
        if (!success)
        {
            throw new Exception("Optimization failed.");
        }

        return optimizer.Solution;
    }
}