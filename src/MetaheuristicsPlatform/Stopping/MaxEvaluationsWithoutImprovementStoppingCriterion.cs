using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops after a maximum number of objective evaluations without strict best-so-far improvement.</summary>
public sealed class MaxEvaluationsWithoutImprovementStoppingCriterion : IStoppingCriterion
{
    public MaxEvaluationsWithoutImprovementStoppingCriterion(long maxEvaluationsWithoutImprovement)
    {
        if (maxEvaluationsWithoutImprovement <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEvaluationsWithoutImprovement));
        }

        MaxEvaluationsWithoutImprovement = maxEvaluationsWithoutImprovement;
    }

    public long MaxEvaluationsWithoutImprovement { get; }

    public string Name => "MaxEvaluationsWithoutImprovement";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense)
    {
        if (!state.HasBestSolution)
        {
            return StoppingDecision.Continue(Name);
        }

        long stagnantEvaluations = state.Evaluations - state.LastImprovementEvaluation;
        return stagnantEvaluations >= MaxEvaluationsWithoutImprovement
            ? StoppingDecision.Stop(
                Name,
                $"No strict improvement for {stagnantEvaluations} objective evaluations.")
            : StoppingDecision.Continue(Name);
    }
}