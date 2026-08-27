using MetaheuristicsPlatform.Algorithms.PSO.ComprehensiveLearning;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class ComprehensiveLearningPsoScientificTests
{
    [Fact]
    public void DescriptorFactoryAndDoiUseCanonicalIdentity()
    {
        var optimizer = new ComprehensiveLearningParticleSwarmOptimizer();
        Assert.Equal(MetaheuristicAlgorithmIds.ComprehensiveLearningParticleSwarm, optimizer.Descriptor.Id);
        Assert.Contains(optimizer.Descriptor.References, reference => reference.Doi == "10.1109/TEVC.2005.857610");
        Assert.NotNull(MetaheuristicFactory.Create<ComprehensiveLearningParticleSwarmOptimizer>(MetaheuristicAlgorithmIds.ComprehensiveLearningParticleSwarm));
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
    public void PublishedLearningProbabilityAndInertiaEndpointsArePreserved()
    {
        Assert.Equal(0.05, ComprehensiveLearningPsoSchedule.LearningProbability(0, 40), 12);
        Assert.Equal(0.50, ComprehensiveLearningPsoSchedule.LearningProbability(39, 40), 12);
        Assert.Equal(0.90, ComprehensiveLearningPsoSchedule.InertiaWeight(0, 100, 0.9, 0.4), 12);
        Assert.Equal(0.40, ComprehensiveLearningPsoSchedule.InertiaWeight(100, 100, 0.9, 0.4), 12);
    }

    private static OptimizationResult<double[]> Run(ulong seed) =>
        new ComprehensiveLearningParticleSwarmOptimizer().Optimize(
            CreateSphere(6),
            new ComprehensiveLearningPsoParameters { SwarmSize = 6, MaximumIterations = 2 },
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
