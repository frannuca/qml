using Accord.Genetic;
namespace QMLib.optimizers
{
    public abstract class NormalizedGeneticAlgorithm(int populationSize, int chromosomeLength)
    {
        double mutationProbability = 0.1;
        double crossoverProbability = 0.9;
// Create a mutation operator
        public int PopulationSize { get; } = populationSize;
        public int ChromosomeLength { get; } = chromosomeLength;
        protected abstract FitnessFunction Fitness { get; }

        public double[] Fit(int maxEpochs, double fitnessTolerance, int maxIterNoChange)
        {
            
            // Create a mutation operator
            
            ISelectionMethod selection = new EliteSelection();
            var rndSource = new Accord.Statistics.Distributions.Univariate.UniformContinuousDistribution(0,1);
            var chromo = new BoundedDoubleArrayChromosome(ChromosomeLength, lowerBound:0, 1 );
            var pop = new BoundedPopulation(PopulationSize, chromo, Fitness, selection, 0, 1);
            pop.MutationRate = mutationProbability;
            pop.CrossoverRate= crossoverProbability;
            pop.RandomSelectionPortion = 0.1;
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
