using Accord.Genetic;
using Accord.Math.Random;
using Accord.Statistics.Distributions.Univariate;

namespace QMLib.optimizers
{
    public record class GaParams(int PopulationSize,double CrossoverRate, double MutationRate, double RandomSelectionPortion);
    
    public abstract class NormalizedGeneticAlgorithm(GaParams pGaParams, int chromosomeLength)
    {
        static readonly IRandomNumberGenerator<double> UniformGenerator =
            new UniformContinuousDistribution(0, 1);

        static NormalizedGeneticAlgorithm()
        {
            Accord.Math.Random.Generator.Seed = 42;
        }


        // Create a mutation operator
        private int PopulationSize => pGaParams.PopulationSize>0?pGaParams.PopulationSize:100;

        private double MutationRate => pGaParams.MutationRate is > 0 and < 1
                                        ?pGaParams.MutationRate:0.1;

        private double CrossOverRate => pGaParams.CrossoverRate is > 0 and < 1
                                        ?pGaParams.CrossoverRate:0.9;
        public double CrossoverRate => pGaParams.CrossoverRate;
        private double RandomSelectionPortion => pGaParams.RandomSelectionPortion;
        private int ChromosomeLength { get; } = chromosomeLength;

        protected abstract FitnessFunction Fitness { get; }

        public double[] Fit(int maxEpochs, double fitnessTolerance, int maxIterNoChange)
        {
            
            // Create a mutation operator
            var limits = Enumerable.Range(0, ChromosomeLength).Select(_ => (0.0, 1.0)).ToArray();
            ISelectionMethod selection = new EliteSelection();
            var rndSource = new Accord.Statistics.Distributions.Univariate.UniformContinuousDistribution(0,1);
            var chromo = new BoundedDoubleArrayChromosome(ChromosomeLength,limits,UniformGenerator );
            var pop = new BoundedPopulation(PopulationSize, chromo, Fitness, selection, CrossOverRate,
                MutationRate,RandomSelectionPortion,UniformGenerator);
            
            double lastFitness = 0;
            int nIterNoChange = maxIterNoChange;
            
            for (int i = 0; i < maxEpochs; i++)
            {
                pop.RunEpoch();
                var xx =(pop.BestChromosome as DoubleArrayChromosome)?.Value?? throw new ArgumentException("Only double array chromosomes are expected 1");
                
                var newFitness = pop.BestChromosome.Fitness;
                if(lastFitness < newFitness && Math.Abs(lastFitness - newFitness) > fitnessTolerance)
                {
                    lastFitness = newFitness;
                    nIterNoChange = maxIterNoChange;
                }
                else if (lastFitness < newFitness)
                {
                    lastFitness = newFitness;
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
            var sol = (pop.BestChromosome as DoubleArrayChromosome)?.Value?? Array.Empty<double>();
            return Scale(sol);
        }

        public abstract double[] Scale(double[] x);
    }

  
}
