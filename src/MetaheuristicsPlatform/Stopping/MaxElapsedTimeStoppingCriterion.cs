using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops after a maximum elapsed wall-clock duration.</summary>
public sealed class MaxElapsedTimeStoppingCriterion : IStoppingCriterion
{
    public MaxElapsedTimeStoppingCriterion(TimeSpan maxElapsed)
    {
        if (maxElapsed <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxElapsed));
        }

        MaxElapsed = maxElapsed;
    }

    public TimeSpan MaxElapsed { get; }

    public string Name => "MaxElapsedTime";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense) =>
        state.Elapsed >= MaxElapsed
            ? StoppingDecision.Stop(Name, $"Maximum elapsed time ({MaxElapsed}) reached.")
            : StoppingDecision.Continue(Name);
}