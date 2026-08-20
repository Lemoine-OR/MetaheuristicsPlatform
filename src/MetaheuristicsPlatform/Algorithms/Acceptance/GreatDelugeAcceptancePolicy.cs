using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>Classical Dueck (1993) Great Deluge absolute-level acceptance.</summary>
/// <remarks>
/// This intentionally does not add the later Extended-GDA hill-climbing disjunct.
/// </remarks>
public sealed class GreatDelugeAcceptancePolicy : ITrajectoryAcceptancePolicy
{
    public GreatDelugeAcceptancePolicy(double initialWaterLevel)
    {
        if (!double.IsFinite(initialWaterLevel))
            throw new ArgumentOutOfRangeException(nameof(initialWaterLevel));

        WaterLevel = initialWaterLevel;
    }

    public double WaterLevel { get; private set; }

    public void AdvanceLevel(OptimizationSense sense, double rainSpeed)
    {
        if (!double.IsFinite(rainSpeed) || rainSpeed <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(rainSpeed));

        double next = sense switch
        {
            OptimizationSense.Minimize => WaterLevel - rainSpeed,
            OptimizationSense.Maximize => WaterLevel + rainSpeed,
            _ => throw new ArgumentOutOfRangeException(nameof(sense))
        };

        if (!double.IsFinite(next))
            throw new InvalidOperationException("The Great Deluge water level became non-finite.");

        WaterLevel = next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        return context.Sense switch
        {
            OptimizationSense.Minimize => context.CandidateObjective <= WaterLevel,
            OptimizationSense.Maximize => context.CandidateObjective >= WaterLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(context))
        };
    }
}