using Accord.Genetic;
using Accord.Math.Random;
using Accord.Statistics.Distributions.Univariate;

namespace QMLib.optimizers
{
    public abstract class NormalizedGeneticAlgorithm(int populationSize, int chromosomeLength)
    {
        static readonly IRandomNumberGenerator<double> UniformGenerator =
            new UniformContinuousDistribution(0, 1);

        static NormalizedGeneticAlgorithm()
        {
            Accord.Math.Random.Generator.Seed = 42;
        }
        
        private const double MutationProbability = 0.1;

        private const double CrossoverProbability = 0.9;

        // Create a mutation operator
        private int PopulationSize { get; } = populationSize;
        private int ChromosomeLength { get; } = chromosomeLength;
        protected abstract FitnessFunction Fitness { get; }

        public double[] Fit(int maxEpochs, double fitnessTolerance, int maxIterNoChange)
        {
            
            // Create a mutation operator
            var limits = Enumerable.Range(0, ChromosomeLength).Select(_ => (0.0, 1.0)).ToArray();
            ISelectionMethod selection = new EliteSelection();
            var rndSource = new Accord.Statistics.Distributions.Univariate.UniformContinuousDistribution(0,1);
            var chromo = new BoundedDoubleArrayChromosome(ChromosomeLength,limits,UniformGenerator );
            var pop = new BoundedPopulation(PopulationSize, chromo, Fitness, selection, 0,
                0.1, 0.1,UniformGenerator);
            
            double lastFitness = 0;
            int nIterNoChange = maxIterNoChange;
            for (int i = 0; i < maxEpochs; i++)
            {
                pop.RunEpoch();
                var xx =(pop.BestChromosome as DoubleArrayChromosome)?.Value?? throw new ArgumentException("Only double array chromosomes are expected 1");
                //Console.WriteLine($"{String.Join(',',xx)} {pop.BestChromosome.Fitness}");
                var newfitness = pop.BestChromosome.Fitness;
                if(lastFitness < newfitness && Math.Abs(lastFitness - newfitness) > fitnessTolerance)
                {
                    lastFitness = newfitness;
                    nIterNoChange = maxIterNoChange;
                }
                else if (lastFitness < newfitness)
                {
                    lastFitness = newfitness;
                    nIterNoChange--;
                }
                else
                {
                    nIterNoChange--;
                }
                if(nIterNoChange == 0)
                {
                    break;
                }
            }
            return (pop.BestChromosome as DoubleArrayChromosome)?.Value?? Array.Empty<double>();
        }

        public abstract double[] Scale(double[] x);
    }

  
}
