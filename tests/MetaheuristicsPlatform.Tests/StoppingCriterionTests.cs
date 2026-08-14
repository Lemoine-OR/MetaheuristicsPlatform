using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class StoppingCriterionTests
{
    [Fact]
    public void MaxIterations_StopsAtLimit()
    {
        var criterion = new MaxIterationsStoppingCriterion(10);
        var state = new OptimizationState(10, 50, TimeSpan.Zero, true, 1.0, 8, 40, 4);

        StoppingDecision decision = criterion.Evaluate(in state, OptimizationSense.Minimize);

        Assert.True(decision.ShouldStop);
        Assert.Equal("MaxIterations", decision.Criterion);
    }

    [Fact]
    public void TargetFitness_RespectsMinimization()
    {
        var criterion = new TargetFitnessStoppingCriterion(10.0);
        var state = new OptimizationState(1, 1, TimeSpan.Zero, true, 9.5, 0, 1, 1);

        StoppingDecision decision = criterion.Evaluate(in state, OptimizationSense.Minimize);

        Assert.True(decision.ShouldStop);
    }

    [Fact]
    public void Any_StopsWhenOneChildStops()
    {
        var criterion = new AnyStoppingCriterion(
            new MaxIterationsStoppingCriterion(100),
            new MaxEvaluationsStoppingCriterion(10));

        var state = new OptimizationState(2, 10, TimeSpan.Zero, true, 1.0, 1, 5, 2);

        StoppingDecision decision = criterion.Evaluate(in state, OptimizationSense.Minimize);

        Assert.True(decision.ShouldStop);
        Assert.Equal("MaxEvaluations", decision.Criterion);
    }
}