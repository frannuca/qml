// See https://aka.ms/new-console-template for more information

using QMLib.optimizers;

Console.WriteLine("Hello, World!");

var ga = new GaOpt(100, new (double, double)[] { (0, 10), (0, 10) }, 
        x => -double.Pow(x[0]-5,2) - double.Pow(x[1]-7,2));

var sol = ga.Fit(500,0.0001,int.MaxValue);
Console.Write($"Solution {String.Join(',',ga.Scale(sol))}");
