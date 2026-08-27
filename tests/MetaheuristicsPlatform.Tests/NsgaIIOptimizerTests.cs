using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaII;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;

public sealed class NsgaIIOptimizerTests
{

    [Fact]
    public void ParetoDominance_RespectsMixedObjectiveSenses()
    {
        OptimizationSense[] senses =
            new[] { OptimizationSense.Minimize, OptimizationSense.Maximize };

        Assert.Equal(
            -1,
            ParetoDominance.Compare(
                new double[] { 1.0, 5.0 },
                new double[] { 2.0, 4.0 },
                senses));

        Assert.Equal(
            0,
            ParetoDominance.Compare(
                new double[] { 1.0, 4.0 },
                new double[] { 2.0, 5.0 },
                senses));
    }

    [Fact]
    public void Optimize_ReturnsNondominatedFront_AndFactoryCreatesCanonicalType()
    {
        ContinuousMultiobjectiveOptimizationProblem problem = CreateProblem();
        NsgaIIParameters parameters = new() { MaximumGenerations = 5 };

        MultiobjectiveOptimizationResult result =
            new NsgaIIOptimizer().Optimize(
                problem,
                parameters,
                new OptimizationOptions { Seed = 987654321UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.ParetoFront);
        Assert.True(result.Evaluations > 0);

        Assert.IsType<NsgaIIOptimizer>(
            MetaheuristicFactory.Create<NsgaIIOptimizer>(
                MetaheuristicAlgorithmIds.NsgaII));

        foreach (MultiobjectivePoint first in result.ParetoFront)
        foreach (MultiobjectivePoint second in result.ParetoFront)
        {
            if (ReferenceEquals(first, second))
                continue;

            Assert.NotEqual(
                -1,
                ParetoDominance.Compare(
                    second.Objectives,
                    first.Objectives,
                    problem.ObjectiveSenses));
        }
    }

    private static ContinuousMultiobjectiveOptimizationProblem CreateProblem()
    {
        return new ContinuousMultiobjectiveOptimizationProblem(
            BoundedContinuousSearchSpace.Uniform(4, 0.0, 1.0),
            new[] { OptimizationSense.Minimize, OptimizationSense.Minimize },
            static (ReadOnlySpan<double> x, Span<double> f) =>
            {
                f[0] = x[0];
                double g = 1.0 + 9.0 * (x[1] + x[2] + x[3]) / 3.0;
                f[1] = g * (1.0 - Math.Sqrt(x[0] / g));
            });
    }
}
