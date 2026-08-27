using MetaheuristicsPlatform.Algorithms.Multiobjective.Mopso;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace MetaheuristicsPlatform.Tests;

public sealed class MopsoOptimizerTests
{

    [Fact]
    public void Optimize_ReturnsNondominatedFront_AndFactoryCreatesCanonicalType()
    {
        ContinuousMultiobjectiveOptimizationProblem problem = CreateProblem();
        MopsoParameters parameters = new() { MaximumIterations = 5 };

        MultiobjectiveOptimizationResult result =
            new MopsoOptimizer().Optimize(
                problem,
                parameters,
                new OptimizationOptions { Seed = 987654321UL },
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.ParetoFront);
        Assert.True(result.Evaluations > 0);

        Assert.IsType<MopsoOptimizer>(
            MetaheuristicFactory.Create<MopsoOptimizer>(
                MetaheuristicAlgorithmIds.Mopso));

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
