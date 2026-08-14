using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;

namespace MetaheuristicsPlatform.Trajectory;

public static class TrajectoryObjectiveComparison
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetter(
        OptimizationSense sense,
        double candidate,
        double reference) =>
        sense switch
        {
            OptimizationSense.Minimize =>
                candidate < reference,

            OptimizationSense.Maximize =>
                candidate > reference,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(sense))
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEqual(
        double candidate,
        double reference) =>
        candidate == reference;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TrajectoryTransitionQuality Classify(
        OptimizationSense sense,
        double candidate,
        double current)
    {
        if (IsBetter(
                sense,
                candidate,
                current))
        {
            return
                TrajectoryTransitionQuality.Improving;
        }

        if (IsEqual(
                candidate,
                current))
        {
            return
                TrajectoryTransitionQuality.Equal;
        }

        return
            TrajectoryTransitionQuality.Worsening;
    }
}