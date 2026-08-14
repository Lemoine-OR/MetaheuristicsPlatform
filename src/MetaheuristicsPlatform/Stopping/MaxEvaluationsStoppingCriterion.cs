using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops after a maximum number of objective evaluations.</summary>
public sealed class MaxEvaluationsStoppingCriterion : IStoppingCriterion
{
    public MaxEvaluationsStoppingCriterion(long maxEvaluations)
    {
        if (maxEvaluations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEvaluations));
        }

        MaxEvaluations = maxEvaluations;
    }

    public long MaxEvaluations { get; }

    public string Name => "MaxEvaluations";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense) =>
        state.Evaluations >= MaxEvaluations
            ? StoppingDecision.Stop(Name, $"Maximum number of evaluations ({MaxEvaluations}) reached.")
            : StoppingDecision.Continue(Name);
}