using MetaheuristicsPlatform.Algorithms.Multimodal.NeighborhoodMutationDe;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;

public sealed class NeighborhoodMutationDeOptimizerTests
{
    [Fact]
    public void Optimize_ReturnsDistinctOptima_AndFactoryCreatesCanonicalType()
    {
        ContinuousMultimodalOptimizationProblem problem =
            new(
                BoundedContinuousSearchSpace.Uniform(
                    2,
                    -1.0,
                    1.0),
                OptimizationSense.Minimize,
                static x =>
                    Math.Sin(3.0 * Math.PI * x[0]) *
                    Math.Sin(3.0 * Math.PI * x[0]) +
                    Math.Sin(3.0 * Math.PI * x[1]) *
                    Math.Sin(3.0 * Math.PI * x[1]));

        MultimodalOptimizationResult result =
            new NeighborhoodMutationDeOptimizer().Optimize(
                problem,
                new NeighborhoodMutationDeParameters { MaximumGenerations = 4, PopulationSize = 24, NeighborhoodSize = 6 },
                new OptimizationOptions
                {
                    Seed = 55667788UL
                },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Optima);
        Assert.True(result.Evaluations > 0);
        Assert.All(
            result.Optima,
            point => Assert.True(double.IsFinite(point.Objective)));

        Assert.IsType<NeighborhoodMutationDeOptimizer>(
            MetaheuristicFactory.Create<NeighborhoodMutationDeOptimizer>(
                MetaheuristicAlgorithmIds.NeighborhoodMutationDe));

        for (int i = 0; i < result.Optima.Count; i++)
        for (int j = i + 1; j < result.Optima.Count; j++)
            Assert.True(
                MultimodalDistance(
                    result.Optima[i].Solution,
                    result.Optima[j].Solution) > 0.0);
    }

    private static double MultimodalDistance(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second)
    {
        double sum = 0.0;

        for (int i = 0; i < first.Count; i++)
        {
            double delta = first[i] - second[i];
            sum += delta * delta;
        }

        return Math.Sqrt(sum);
    }
}
