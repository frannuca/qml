using qmlib.optimizers.convex;
using qmlib.optimizers.GA;

namespace TestQMLib;

public class BoundedGATests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var expected = new double[] { 5.0,7.0 };
        var pGaParams = new GaParams(PopulationSize: 100, CrossoverRate: 0.8, MutationRate: 0.1,
            RandomSelectionPortion: 0.1);
        var ga = new GaOpt(pGaParams, new (double, double)[] { (0, 10), (0, 10) }, 
            x => -double.Pow(x[0]-expected[0],2) - double.Pow(x[1]-expected[1],2));

        var sol = ga.Fit(50,0.0001,int.MaxValue);
        Assert.That(actual:expected[0], Is.EqualTo(sol[0]).Within(0.05));
        Assert.That(actual:expected[1], Is.EqualTo(sol[1]).Within(0.05));
    }

    [Test]
    public void Test2()
    {
        var expected = new double[] { 5.0,7.0 };
        var auglag = new AugmentedLagrangianOptimizer(
            x => double.Pow(x[0] - expected[0], 2) + double.Pow(x[1] - expected[1], 2),
            [(0,10),(0,10)]);
        
        var sol2 = auglag.Optimize();
        Assert.That(actual:expected[0], Is.EqualTo(sol2[0]).Within(0.001));
        Assert.That(actual:expected[1], Is.EqualTo(sol2[1]).Within(0.001));
    }
}