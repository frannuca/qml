// See https://aka.ms/new-console-template for more information

using QMLib.optimizers;

Console.WriteLine("Hello, World!");

var expected = new double[] { 5.0,7.0 };
var pGaParams = new GaParams(PopulationSize: 100, CrossoverRate: 0.8, MutationRate: 0.1,
    RandomSelectionPortion: 0.1);
var ga = new GaOpt(pGaParams, new (double, double)[] { (0, 10), (0, 10) }, 
    x => -double.Pow(x[0]-expected[0],2) - double.Pow(x[1]-expected[1],2));

var sol = ga.Fit(50,0.0001,int.MaxValue);
Console.Write($"Solution {String.Join(',',ga.Scale(sol))}");
