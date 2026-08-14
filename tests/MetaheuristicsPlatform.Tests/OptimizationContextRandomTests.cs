using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class OptimizationContextRandomTests
{
    [Fact]
    public void ContextsWithSameSeed_ExposeSameRandomSequence()
    {
        OptimizationContext<double> first = CreateContext(20260814UL);
        OptimizationContext<double> second = CreateContext(20260814UL);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(first.Random.NextUInt64(), second.Random.NextUInt64());
        }
    }

    private static OptimizationContext<double> CreateContext(ulong seed)
    {
        var descriptor = new MetaheuristicDescriptor
        {
            Id = "test",
            Name = "Test Algorithm",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.Other,
            SearchSpaces = SearchSpaceKind.Continuous
        };

        return new OptimizationContext<double>(
            descriptor,
            new ScalarProblem(),
            new ImmutableSolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(1),
            new OptimizationOptions { Seed = seed });
    }

    private sealed class ScalarProblem : IOptimizationProblem<double>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(double solution) => solution;
    }
}