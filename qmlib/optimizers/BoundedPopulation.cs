using Accord.Genetic;

namespace QMLib.optimizers;

using Accord.Genetic;

public class BoundedPopulation : Population
{
    private readonly double lowerBound;
    private readonly double upperBound;

    
    public BoundedPopulation(int size, BoundedDoubleArrayChromosome ancestor, IFitnessFunction fitnessFunction, ISelectionMethod selectionMethod, double lowerBound, double upperBound)
        : base(size, ancestor, fitnessFunction, selectionMethod)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
    }

  
    private void SetBounds()
    {
        // Check the boundaries of each chromosome
        for (int index = 0; index < this.Size; ++index)
        {
            DoubleArrayChromosome boundedChromosome =  this[index] as DoubleArrayChromosome
                                                              ??throw new ArgumentException("Only double array chromosomes are expected XXX");

            for (int i = 0; i < boundedChromosome.Length; i++)
            {
                // If a gene is outside the boundaries, adjust it to the nearest boundary
                if (boundedChromosome.Value[i] < lowerBound)
                {
                    boundedChromosome.Value[i] = lowerBound;
                }
                else if (boundedChromosome.Value[i] > upperBound)
                {
                    boundedChromosome.Value[i] = upperBound;
                }
            }
        }
    }
    public override void Crossover()
    {
        base.Crossover();
        SetBounds();

    }
    public override void Selection()
    {
        base.Selection();
        SetBounds();
    }
    public override void Mutate()
    {
        // Call the base Mutate method
        base.Mutate();
        SetBounds();
        
        
    }
}