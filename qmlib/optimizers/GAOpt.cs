using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Accord.Genetic;

namespace QMLib.optimizers
{
    public class GaOpt(
        int populationSize,
        (double lowlimit, double highlimit)[] variableLimits,
        Func<double[], double> fFitness)
        : NormalizedGeneticAlgorithm(populationSize, variableLimits.Length)
    {
        protected override FitnessFunction Fitness => new FitnessFunction(x =>  fFitness(Scale(x)));

        public override double[] Scale(double[] x)
        {
            if(x.Any(s => s>1 || s<0))
            {
                x = x.Select(a => Math.Min(1,Math.Max(0, a))).ToArray();
            }
            return x.Select((a, n) => variableLimits[n].lowlimit + (variableLimits[n].highlimit - variableLimits[n].lowlimit)*a).ToArray();
        }
    }
}
