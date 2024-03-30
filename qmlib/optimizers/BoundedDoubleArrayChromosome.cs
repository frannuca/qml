using Accord.Math.Random;

namespace QMLib.optimizers;

using Accord.Genetic;
using Accord.Statistics.Distributions.Univariate;

public class BoundedDoubleArrayChromosome(
    int length,
    double lowerBound,
    double upperBound) : DoubleArrayChromosome(uniformGenerator,
    uniformGenerator,
    uniformGenerator, length)
{
    
    private static readonly IRandomNumberGenerator<double> uniformGenerator =
        new UniformContinuousDistribution(0, 1);

    static BoundedDoubleArrayChromosome()
    {
        // Set the global seed
        Accord.Math.Random.Generator.Seed = 42;
    }

    
    public override IChromosome CreateNew()
    {
        return new BoundedDoubleArrayChromosome(
            Length,
            lowerBound,
            upperBound);
    }

    public override void Generate()
    {
        for (int i = 0; i < Length; i++)
        {
            // Generate a value within the desired boundaries
            double value = uniformGenerator.Generate() * (upperBound - lowerBound) + lowerBound;

            // Assign the value to the chromosome
            Value[i] = value;
        }
    }
}