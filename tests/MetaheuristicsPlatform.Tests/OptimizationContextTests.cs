using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class OptimizationContextTests
{
    [Fact]
    public void Context_TracksEvaluationsIterationsAndBest()
    {
        var descriptor = new MetaheuristicDescriptor
        {
            Id = "test",
            Name = "Test Algorithm",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.None,
            SearchSpaces = SearchSpaceKind.Continuous
        };

        var problem = new ScalarProblem();
        var context = new OptimizationContext<double>(
            descriptor,
            problem,
            new ImmutableSolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(2));

        context.Start();
        context.Evaluate(5.0);
        context.CompleteIteration();
        context.Evaluate(3.0);
        context.CompleteIteration();

        StoppingDecision stop = context.EvaluateStopping();
        OptimizationResult<double> result = context.Complete(stop);

        Assert.True(stop.ShouldStop);
        Assert.Equal(3.0, result.BestFitness);
        Assert.Equal(2, result.Statistics.Iterations);
        Assert.Equal(2, result.Statistics.Evaluations);
        Assert.Equal(2, result.Statistics.Improvements);
    }

    private sealed class ScalarProblem : IOptimizationProblem<double>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(double solution) => solution;
    }
}