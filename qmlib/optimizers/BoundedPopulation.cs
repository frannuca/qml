using Accord.Genetic;
using Accord.Math.Random;

namespace QMLib.optimizers;

using Accord.Genetic;

public class BoundedPopulation: Population
{
    public BoundedPopulation(
        int size,
        BoundedDoubleArrayChromosome ancestor,
        IFitnessFunction fitnessFunction,
        ISelectionMethod selectionMethod,
        double crossoverRate,
        double mutationRate,
        double randomSelectionPortion,
        IRandomNumberGenerator<double> uniformGenerator)
        : base(size, ancestor, fitnessFunction, selectionMethod)
    {
        this.CrossoverRate = crossoverRate;
        this.MutationRate = mutationRate;
        this.RandomSelectionPortion = randomSelectionPortion;
        this._uniformGenerator=uniformGenerator;
    }

    private readonly IRandomNumberGenerator<double> _uniformGenerator;
    public override void Crossover()
    {
        for (int index = 1; index < this.Size; index += 2)
        {
            if (_uniformGenerator.Generate() <= CrossoverRate)
            {
                IChromosome chromosome = this[index - 1].Clone();
                IChromosome pair = this[index].Clone();
                chromosome.Crossover(pair);
                (chromosome as BoundedDoubleArrayChromosome)?.SetBounds();
                (pair as BoundedDoubleArrayChromosome)?.SetBounds();
                chromosome.Evaluate(FitnessFunction);
                pair.Evaluate(FitnessFunction);
                AddChromosome(chromosome);
                AddChromosome(pair);
            }
        }
    }
    public override void Mutate()
    {
        // Call the base Mutate method
        for (int index = 0; index < this.Size; ++index)
        {
            if (Generator.Random.NextDouble() <= MutationRate)
            {
                IChromosome chromosome = this[index].Clone();
                chromosome.Mutate();
                (chromosome as BoundedDoubleArrayChromosome)?.SetBounds();
                chromosome.Evaluate(FitnessFunction);
                AddChromosome(chromosome);
            }
        }
    }
}