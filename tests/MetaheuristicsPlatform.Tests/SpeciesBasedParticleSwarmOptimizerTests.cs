using MetaheuristicsPlatform.Algorithms.PSO.Speciation;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class SpeciesBasedPsoScientificTests
{
    [Fact]
    public void DescriptorFactoryAndDoiUseCanonicalIdentity()
    {
        var optimizer = new SpeciesBasedParticleSwarmOptimizer();
        Assert.Equal(MetaheuristicAlgorithmIds.SpeciesBasedParticleSwarm, optimizer.Descriptor.Id);
        Assert.Contains(optimizer.Descriptor.References, reference => reference.Doi == "10.1109/TEVC.2005.859468");
        Assert.NotNull(MetaheuristicFactory.Create<SpeciesBasedParticleSwarmOptimizer>(MetaheuristicAlgorithmIds.SpeciesBasedParticleSwarm));
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<double[]> first = Run(12345UL);
        OptimizationResult<double[]> second = Run(12345UL);

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(first.Statistics.Evaluations, second.Statistics.Evaluations);
        Assert.Equal(first.Statistics.Iterations, second.Statistics.Iterations);
    }

    [Fact]
    public void BetterSeedClaimsNearbyParticleInsideSpeciesRadius()
    {
        double[][] positions =
        [
            [0.0, 0.0],
            [0.1, 0.0],
            [3.0, 3.0]
        ];
        double[] fitness = [0.0, 1.0, 2.0];

        int[] seeds = SpeciesPartitioner.AssignSpeciesSeeds(
            positions,
            fitness,
            OptimizationSense.Minimize,
            0.5);

        Assert.Equal(0, seeds[0]);
        Assert.Equal(0, seeds[1]);
        Assert.Equal(2, seeds[2]);
    }

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new SpeciesBasedParticleSwarmOptimizer().Optimize(
            CreateSphere(6),
            new SpeciesBasedPsoParameters { SwarmSize = 6, MaximumIterations = 2, SpeciesRadiusFraction = 0.2 },
            new ArraySolutionCloner<double>(),
            new NeverStoppingCriterion(),
            new OptimizationOptions { Seed = seed },
            cancellationToken: TestContext.Current.CancellationToken);

    private static ContinuousOptimizationProblem CreateSphere(int dimension) =>
        new(
            BoundedContinuousSearchSpace.Uniform(dimension, -5.0, 5.0),
            OptimizationSense.Minimize,
            Sphere);

    private static double Sphere(ReadOnlySpan<double> x)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
        return sum;
    }

    private sealed class NeverStoppingCriterion : IStoppingCriterion
    {
        public string Name => "Never";

        public StoppingDecision Evaluate(
            in OptimizationState state,
            OptimizationSense sense) =>
            StoppingDecision.Continue(Name);
    }
}
