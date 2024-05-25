using Accord.Genetic;
using Accord.Math.Random;

namespace qmlib.optimizers.GA;

public class BoundedPopulation : Population
{
    private readonly IRandomNumberGenerator<double> _uniformGenerator;

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
        CrossoverRate = crossoverRate;
        MutationRate = mutationRate;
        RandomSelectionPortion = randomSelectionPortion;
        _uniformGenerator = uniformGenerator;
    }

    public override void Crossover()
    {
        for (var index = 1; index < Size; index += 2)
            if (_uniformGenerator.Generate() <= CrossoverRate)
            {
                var chromosome = this[index - 1].Clone();
                var pair = this[index].Clone();
                chromosome.Crossover(pair);
                (chromosome as BoundedDoubleArrayChromosome)?.SetBounds();
                (pair as BoundedDoubleArrayChromosome)?.SetBounds();
                chromosome.Evaluate(FitnessFunction);
                pair.Evaluate(FitnessFunction);
                AddChromosome(chromosome);
                AddChromosome(pair);
            }
    }

    public override void Mutate()
    {
        // Call the base Mutate method
        for (var index = 0; index < Size; ++index)
            if (Generator.Random.NextDouble() <= MutationRate)
            {
                var chromosome = this[index].Clone();
                chromosome.Mutate();
                (chromosome as BoundedDoubleArrayChromosome)?.SetBounds();
                chromosome.Evaluate(FitnessFunction);
                AddChromosome(chromosome);
            }
    }
}