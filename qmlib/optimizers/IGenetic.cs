using Accord.Genetic;
using Accord.Math.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace qmlib.optimizers
{
    abstract public class IGenetic
    {
        public IGenetic(int population_size, int chromosome_length, Func<double[],double> f_fitness)
        {
            PopulationSize = population_size;     
            Fitness = new FitnessFunction(f_fitness);
            ChromosomeLength = chromosome_length;
        }

        public int PopulationSize { get; }
        public int ChromosomeLength { get; }
        public FitnessFunction Fitness { get; }

        public double[] Fit(int max_epochs)
        {
            ISelectionMethod selection = new EliteSelection();
            var rndsource = new Accord.Statistics.Distributions.Univariate.UniformContinuousDistribution(0,1);
            var chromo = new DoubleArrayChromosome(rndsource, rndsource, rndsource, ChromosomeLength);
            var pop = new Population(PopulationSize, chromo, Fitness, selection);
            return [0,0];
        }

        abstract protected double[] Scale(double[] x);
    }

    public class FitnessFunction : IFitnessFunction
    {
        public FitnessFunction(Func<double[], double> f_fitness) 
        {
            Fitness = f_fitness;
        }

        public Func<double[], double> Fitness { get; }

        public double Evaluate(IChromosome chromosome)
        {
            var dchromosome = chromosome as DoubleArrayChromosome;
            if(dchromosome == null)
            {
                throw new ArgumentException("Only double array chromosomes are expected");
            }

            return Fitness(dchromosome.Value);

        }
    }
}
