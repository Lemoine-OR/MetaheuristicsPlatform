using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>Classical Dueck (1993) best-record deviation acceptance.</summary>
public sealed class RecordToRecordTravelAcceptancePolicy : ITrajectoryAcceptancePolicy
{
    public RecordToRecordTravelAcceptancePolicy(double deviation)
    {
        if (!double.IsFinite(deviation) || deviation < 0.0)
            throw new ArgumentOutOfRangeException(nameof(deviation));

        Deviation = deviation;
    }

    public double Deviation { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        double d = TrajectoryObjectiveComparison.ComputeDegradation(
            context.Sense,
            context.BestObjective,
            context.CandidateObjective);

        return d <= Deviation;
    }
}