using Accord.Genetic;

namespace qmlib.optimizers.GA;

public class FitnessFunction(Func<double[], double> fFitness) : IFitnessFunction
{
    private Func<double[], double> Fitness { get; } = fFitness;

    public double Evaluate(IChromosome chromosome)
    {
        var dchromosome = chromosome as DoubleArrayChromosome;
        if (dchromosome == null) throw new ArgumentException("Only double array chromosomes are expected UUU");

        return Fitness(dchromosome.Value);
    }
}