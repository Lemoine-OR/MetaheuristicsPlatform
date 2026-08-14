using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class CallbackTests
{
    [Fact]
    public void ConvergenceTrace_RecordsOnlySelectedEvents()
    {
        var descriptor = new MetaheuristicDescriptor
        {
            Id = "test",
            Name = "Test Algorithm",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.Other,
            SearchSpaces = SearchSpaceKind.Continuous
        };

        var trace = new ConvergenceTraceCallback<double>(
            OptimizationCallbackEvents.BestImproved |
            OptimizationCallbackEvents.Completed);

        var context = new OptimizationContext<double>(
            descriptor,
            new ScalarProblem(),
            new ImmutableSolutionCloner<double>(),
            new MaxIterationsStoppingCriterion(1),
            callback: trace);

        context.Start();
        context.Evaluate(5.0);
        context.Evaluate(3.0);
        context.CompleteIteration();

        StoppingDecision decision = context.EvaluateStopping();
        context.Complete(decision);

        IReadOnlyList<ConvergencePoint> points = trace.GetSnapshot();

        Assert.Equal(3, points.Count);
        Assert.Equal(OptimizationEventKind.BestImproved, points[0].EventKind);
        Assert.Equal(OptimizationEventKind.BestImproved, points[1].EventKind);
        Assert.Equal(OptimizationEventKind.Completed, points[2].EventKind);
        Assert.Equal(3.0, points[2].BestFitness);
    }

    private sealed class ScalarProblem : IOptimizationProblem<double>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(double solution) => solution;
    }
}