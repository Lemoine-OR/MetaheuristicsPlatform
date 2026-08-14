using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Stopping;

/// <summary>Stops when the target fitness has been reached.</summary>
public sealed class TargetFitnessStoppingCriterion : IStoppingCriterion
{
    public TargetFitnessStoppingCriterion(double targetFitness)
    {
        if (double.IsNaN(targetFitness))
        {
            throw new ArgumentOutOfRangeException(nameof(targetFitness));
        }

        TargetFitness = targetFitness;
    }

    public double TargetFitness { get; }

    public string Name => "TargetFitness";

    public StoppingDecision Evaluate(in OptimizationState state, OptimizationSense sense)
    {
        bool reached = state.HasBestSolution &&
                       sense.IsTargetReached(state.BestFitness, TargetFitness);

        return reached
            ? StoppingDecision.Stop(Name, $"Target fitness ({TargetFitness:G17}) reached.")
            : StoppingDecision.Continue(Name);
    }
}