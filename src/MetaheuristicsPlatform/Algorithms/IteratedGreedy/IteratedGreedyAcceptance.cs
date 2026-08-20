using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;

namespace MetaheuristicsPlatform.Algorithms.IteratedGreedy;

/// <summary>Acceptance data for one complete reconstructed Iterated Greedy candidate.</summary>
public readonly record struct IteratedGreedyAcceptanceContext(
    OptimizationSense Sense,
    long Iteration,
    double CurrentObjective,
    double CandidateObjective,
    double BestObjective)
{
    public double Degradation =>
        TrajectoryObjectiveComparison.ComputeDegradation(
            Sense,
            CurrentObjective,
            CandidateObjective);
}

/// <summary>Acceptance policy for a complete Iterated Greedy candidate.</summary>
public interface IIteratedGreedyAcceptancePolicy
{
    bool ShouldAccept(
        in IteratedGreedyAcceptanceContext context,
        IRandomSource random);
}

/// <summary>Deterministic strict-improvement acceptance.</summary>
public sealed class ImprovingOnlyIteratedGreedyAcceptancePolicy :
    IIteratedGreedyAcceptancePolicy
{
    public static ImprovingOnlyIteratedGreedyAcceptancePolicy Instance { get; } = new();

    private ImprovingOnlyIteratedGreedyAcceptancePolicy()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in IteratedGreedyAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        return
            double.IsFinite(context.CandidateObjective) &&
            TrajectoryObjectiveComparison.IsBetter(
                context.Sense,
                context.CandidateObjective,
                context.CurrentObjective);
    }
}

/// <summary>
/// Constant-temperature Metropolis acceptance used by the classical Ruiz-Stützle IG
/// when the supplied temperature has already been scaled for the application domain.
/// </summary>
public sealed class ConstantTemperatureIteratedGreedyAcceptancePolicy :
    IIteratedGreedyAcceptancePolicy
{
    public ConstantTemperatureIteratedGreedyAcceptancePolicy(double temperature)
    {
        if (!double.IsFinite(temperature) || temperature <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(temperature));

        Temperature = temperature;
    }

    public double Temperature { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in IteratedGreedyAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (!double.IsFinite(context.CandidateObjective) ||
            !double.IsFinite(context.CurrentObjective))
        {
            return false;
        }

        if (TrajectoryObjectiveComparison.IsBetter(
                context.Sense,
                context.CandidateObjective,
                context.CurrentObjective) ||
            TrajectoryObjectiveComparison.IsEqual(
                context.CandidateObjective,
                context.CurrentObjective))
        {
            return true;
        }

        double probability =
            Math.Exp(-context.Degradation / Temperature);

        return random.NextDouble() < probability;
    }
}
