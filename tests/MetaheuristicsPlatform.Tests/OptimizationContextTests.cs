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

        Assert.Equal(TimeSpan.Zero, context.State.Elapsed);
        context.Start();
        TimeSpan runningElapsed = context.State.Elapsed;
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
        Assert.True(result.Statistics.Elapsed >= runningElapsed);
        Assert.Equal(result.Statistics.Elapsed, context.State.Elapsed);
    }

    [Fact]
    public void EvaluationLimit_UsesCountBeforeReadingProblemSense()
    {
        var descriptor = new MetaheuristicDescriptor
        {
            Id = "evaluation-limit-test",
            Name = "Evaluation limit test",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.Other,
            Mechanisms = MetaheuristicMechanism.None,
            SearchSpaces = SearchSpaceKind.Continuous
        };

        OptimizationContext<double>? context = null;
        var problem = new ScalarProblem
        {
            OnSenseRead = () => context?.RegisterExternalProbeEvaluation(1.0)
        };
        context = new OptimizationContext<double>(
            descriptor,
            problem,
            new ImmutableSolutionCloner<double>(),
            new MaxEvaluationsStoppingCriterion(1));
        context.Start();

        Assert.False(context.EvaluateStopping().ShouldStop);
        Assert.Equal(1, context.State.Evaluations);
        Assert.True(context.EvaluateStopping().ShouldStop);
        Assert.Equal(2, context.State.Evaluations);
    }

    private sealed class ScalarProblem : IOptimizationProblem<double>
    {
        public Action? OnSenseRead { get; init; }

        public OptimizationSense Sense
        {
            get
            {
                OnSenseRead?.Invoke();
                return OptimizationSense.Minimize;
            }
        }

        public double Evaluate(double solution) => solution;
    }
}