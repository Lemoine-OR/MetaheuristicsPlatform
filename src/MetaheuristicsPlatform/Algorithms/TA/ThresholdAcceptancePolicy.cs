using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.TA;

/// <summary>
/// Deterministic Dueck-Scheuer threshold-acceptance rule.
/// </summary>
/// <remarks>
/// A non-worsening transition is always accepted. A worsening transition is accepted
/// exactly when its sense-aware objective degradation is no larger than the active
/// threshold. No acceptance random draw and no exponential evaluation are required.
/// </remarks>
public sealed class ThresholdAcceptancePolicy :
    ITrajectoryAcceptancePolicy
{
    public ThresholdAcceptancePolicy(
        double threshold)
    {
        SetThreshold(
            threshold);
    }

    /// <summary>Current non-negative acceptance threshold.</summary>
    public double Threshold { get; private set; }

    /// <summary>Updates the active non-negative threshold.</summary>
    public void SetThreshold(
        double threshold)
    {
        if (!double.IsFinite(threshold) ||
            threshold < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(threshold));
        }

        Threshold =
            threshold;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (context.Quality !=
            TrajectoryTransitionQuality.Worsening)
        {
            return true;
        }

        double degradation =
            TrajectoryObjectiveComparison.ComputeDegradation(
                context.Sense,
                context.CurrentObjective,
                context.CandidateObjective);

        return
            degradation <=
            Threshold;
    }
}