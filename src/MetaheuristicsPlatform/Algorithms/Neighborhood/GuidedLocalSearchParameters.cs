using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Neighborhood;

/// <summary>Parameters for canonical Guided Local Search.</summary>
public sealed class GuidedLocalSearchParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Penalty weight lambda in the augmented objective.
    /// For maximization the implementation subtracts the penalty term so that
    /// penalized features remain unattractive under the common optimization sense.
    /// </summary>
    public double PenaltyWeight { get; init; } = 1.0;

    /// <summary>Maximum number of canonical feature-penalty updates.</summary>
    public int MaximumPenaltyUpdates { get; init; } = 100;

    /// <summary>
    /// Safety cap on accepted augmented-objective moves between two penalty updates.
    /// </summary>
    public int MaximumAcceptedMovesPerPenaltyPhase { get; init; } = int.MaxValue;

    /// <summary>First- or best-improvement selection under the augmented objective.</summary>
    public LocalSearchSelectionPolicy SelectionPolicy { get; init; } =
        LocalSearchSelectionPolicy.BestImprovement;

    /// <inheritdoc />
    public void Validate()
    {
        if (!double.IsFinite(PenaltyWeight) || PenaltyWeight <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(PenaltyWeight));
        }

        if (MaximumPenaltyUpdates <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumPenaltyUpdates));
        }

        if (MaximumAcceptedMovesPerPenaltyPhase <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumAcceptedMovesPerPenaltyPhase));
        }

        if (!Enum.IsDefined(SelectionPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(SelectionPolicy));
        }
    }
}
