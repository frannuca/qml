using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Accord.Genetic;

namespace qmlib.optimizers
{
    public class GAOpt : IGenetic
    {

        public GAOpt(int population_size, IEnumerable<(double lowlimit, double highlimit)> variable_limits ,Func<double[], double> f_fitness): base(population_size, variable_limits.Count(), f_fitness) 
        { 
            limits = variable_limits.ToArray();
        }

        private readonly (double lowlimit, double highlimit)[] limits;

        protected override double[] Scale(double[] x)
        {
            return x.Select((a, n) => limits[n].lowlimit + (limits[n].highlimit - limits[n].lowlimit)*a).ToArray();
        }
    }
}
