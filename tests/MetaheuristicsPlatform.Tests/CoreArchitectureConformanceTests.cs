using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class CoreArchitectureConformanceTests
{
    [Fact]
    public void SingleSolutionSearch_UsesCommonContext()
    {
        MetaheuristicDescriptor descriptor = Descriptor(
            "single-probe",
            MetaheuristicSolutionModel.SingleSolution,
            MetaheuristicFamily.TrajectoryBased,
            MetaheuristicMechanism.Trajectory);

        var context = NewContext(descriptor);
        context.Start();

        double current = 8.0;
        context.Evaluate(current);

        while (true)
        {
            StoppingDecision stop = context.EvaluateStopping();
            if (stop.ShouldStop)
            {
                OptimizationResult<double> result = context.Complete(stop);
                Assert.Equal(5.0, result.BestFitness);
                Assert.Equal(3, result.Statistics.Iterations);
                return;
            }

            current -= 1.0;
            context.Evaluate(current);
            context.CompleteIteration(current);
        }
    }

    [Fact]
    public void PopulationBasedSearch_UsesSameCommonContext()
    {
        MetaheuristicDescriptor descriptor = Descriptor(
            "population-probe",
            MetaheuristicSolutionModel.Population,
            MetaheuristicFamily.Evolutionary,
            MetaheuristicMechanism.EvolutionaryOperators);

        var context = NewContext(descriptor);
        context.Start();

        double[][] populations =
        {
            new[] { 10.0, 8.0, 12.0 },
            new[] { 7.0, 6.0, 9.0 },
            new[] { 5.0, 4.0, 8.0 }
        };

        foreach (double[] population in populations)
        {
            foreach (double candidate in population)
            {
                context.Evaluate(candidate);
            }

            context.CompleteIteration();
        }

        StoppingDecision stop = context.EvaluateStopping();
        OptimizationResult<double> result = context.Complete(stop);

        Assert.True(stop.ShouldStop);
        Assert.Equal(4.0, result.BestFitness);
        Assert.Equal(9, result.Statistics.Evaluations);
        Assert.Equal(3, result.Statistics.Iterations);
    }

    [Fact]
    public void NeighborhoodBasedSearch_UsesSameCommonContext()
    {
        MetaheuristicDescriptor descriptor = Descriptor(
            "neighborhood-probe",
            MetaheuristicSolutionModel.SingleSolution,
            MetaheuristicFamily.LocalSearch | MetaheuristicFamily.TrajectoryBased,
            MetaheuristicMechanism.Neighborhood | MetaheuristicMechanism.Trajectory);

        var context = NewContext(descriptor);
        context.Start();

        double current = 10.0;
        context.Evaluate(current);

        for (int iteration = 0; iteration < 3; iteration++)
        {
            double left = current - 1.0;
            double right = current + 1.0;

            double leftFitness = context.Evaluate(left);
            double rightFitness = context.Evaluate(right);

            current = leftFitness <= rightFitness ? left : right;
            context.CompleteIteration(current);
        }

        StoppingDecision stop = context.EvaluateStopping();
        OptimizationResult<double> result = context.Complete(stop);

        Assert.True(stop.ShouldStop);
        Assert.Equal(7.0, result.BestFitness);
        Assert.Equal(7, result.Statistics.Evaluations);
    }

    private static OptimizationContext<double> NewContext(MetaheuristicDescriptor descriptor) =>
        new(
            descriptor,
            new ScalarProblem(),
            new ImmutableSolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(3),
            new OptimizationOptions { Seed = 1UL });

    private static MetaheuristicDescriptor Descriptor(
        string id,
        MetaheuristicSolutionModel solutionModel,
        MetaheuristicFamily family,
        MetaheuristicMechanism mechanism) =>
        new()
        {
            Id = id,
            Name = id,
            SolutionModel = solutionModel,
            Families = family,
            Mechanisms = mechanism,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = false
        };

    private sealed class ScalarProblem : IOptimizationProblem<double>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(double solution) => solution;
    }
}