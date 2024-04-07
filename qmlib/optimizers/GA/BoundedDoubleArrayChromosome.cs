using Accord.Genetic;
using Accord.Math.Random;

namespace qmlib.optimizers.GA;

public class BoundedDoubleArrayChromosome(
    int length,
    (double lowerBound,double upperBound)[] limits,
    IRandomNumberGenerator<double> uniformGenerator) : DoubleArrayChromosome(uniformGenerator,
    uniformGenerator,
    uniformGenerator, length)
{
    private readonly IRandomNumberGenerator<double> _uniformGenerator = uniformGenerator;
    
    public override IChromosome CreateNew()
    {
        return new BoundedDoubleArrayChromosome(
            Length,
            limits,
            _uniformGenerator);
    }

    public void SetBounds()
    {
        for (int i = 0; i < Length; i++)
        {
            var (lowerBound,upperBound) = limits[i];
            if (Value[i] < lowerBound)
            {
                Value[i] = lowerBound;
            }
            else if (Value[i] > upperBound)
            {
                Value[i] = upperBound;
            }
        }
    }
    public override void Generate()
    {
        for (int i = 0; i < Length; i++)
        {
            var (lowerBound,upperBound) = limits[i];
            // Generate a value within the desired boundaries
            double value = _uniformGenerator.Generate() * (upperBound - lowerBound) + lowerBound;

            // Assign the value to the chromosome
            Value[i] = value;
        }
    }
}