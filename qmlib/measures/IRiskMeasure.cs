using MathNet.Numerics.LinearAlgebra;
using qmlib.optimizers.convex;

namespace qmlib.measures;

public abstract class RiskMeasureBase(Matrix<double> covMatrix)
{
    public readonly Matrix<double> CovMatrix = covMatrix;
    public int N => CovMatrix.RowCount;
    public abstract double CalculateRisk(Vector<double> weights);
    public abstract Vector<double> RiskContributions(Vector<double> weights);
    
    protected virtual double LagrangianRisk(Vector<double> weights,  Vector<double> riskBudgets, double lambda)
    {
        weights = weights.Map(v => Math.Max(v,1e-6));
       
        var risk = CalculateRisk(weights);
        var logW = weights.Map(Math.Log2);
        var sum =  riskBudgets * logW;
        var error = risk - lambda * sum;
        Console.WriteLine(error);
        return error;
    }
    
    public Vector<double> OptimizeWeigts(Vector<double> initialWeights, Matrix<double> covMatrix, Vector<double> riskBudgets)
    {
        var lambda = riskBudgets.Sum();
        var fitness = (double[] x) => LagrangianRisk(Vector<double>.Build.DenseOfArray(x),riskBudgets,lambda);
        var solver = new AugmentedLagrangianOptimizer(fitness, Enumerable.Repeat((1.0,500.0),N).ToArray());
        solver.SetInitialGuess(initialWeights.ToArray());
        var sol = solver.Optimize();
        return Vector<double>.Build.DenseOfArray(sol);
    }
    
    public double RiskContributionError(Vector<double> weights, Vector<double> riskBudgets)
    {
        var rc = RiskContributions(weights);
        var error = rc / riskBudgets;
        return (error-error[0]).L2Norm();
    }
}

public class MinVarianceRb(Matrix<double> covMatrix): RiskMeasureBase(covMatrix)
{
    public override double CalculateRisk(Vector<double> weights)
    {
        var risk = weights.ToRowMatrix() * CovMatrix * weights.ToColumnMatrix();
        return Math.Sqrt(risk[0,0]);
    }

    public override  Vector<double> RiskContributions(Vector<double> weights)
    {
        var totalRisk = CalculateRisk(weights);
        var rx = CovMatrix * weights.ToColumnMatrix();
        var riskContributions = weights.ToColumnMatrix().PointwiseMultiply(rx)/ totalRisk;
        return riskContributions.Column(0);
    }
}