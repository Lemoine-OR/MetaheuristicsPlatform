using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops after a maximum number of completed iterations without strict best-so-far improvement.</summary>
public sealed class MaxIterationsWithoutImprovementStoppingCriterion : IStoppingCriterion
{
    public MaxIterationsWithoutImprovementStoppingCriterion(long maxIterationsWithoutImprovement)
    {
        if (maxIterationsWithoutImprovement <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterationsWithoutImprovement));
        }

        MaxIterationsWithoutImprovement = maxIterationsWithoutImprovement;
    }

    public long MaxIterationsWithoutImprovement { get; }

    public string Name => "MaxIterationsWithoutImprovement";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense)
    {
        if (!state.HasBestSolution)
        {
            return StoppingDecision.Continue(Name);
        }

        long stagnantIterations = state.Iteration - state.LastImprovementIteration;
        return stagnantIterations >= MaxIterationsWithoutImprovement
            ? StoppingDecision.Stop(
                Name,
                $"No strict improvement for {stagnantIterations} completed iterations.")
            : StoppingDecision.Continue(Name);
    }
}