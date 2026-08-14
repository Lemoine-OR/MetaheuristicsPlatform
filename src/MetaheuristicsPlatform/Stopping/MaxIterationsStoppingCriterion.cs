using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops after a maximum number of completed iterations.</summary>
public sealed class MaxIterationsStoppingCriterion : IStoppingCriterion
{
    public MaxIterationsStoppingCriterion(long maxIterations)
    {
        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations));
        }

        MaxIterations = maxIterations;
    }

    public long MaxIterations { get; }

    public string Name => "MaxIterations";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense) =>
        state.Iteration >= MaxIterations
            ? StoppingDecision.Stop(Name, $"Maximum number of iterations ({MaxIterations}) reached.")
            : StoppingDecision.Continue(Name);
}