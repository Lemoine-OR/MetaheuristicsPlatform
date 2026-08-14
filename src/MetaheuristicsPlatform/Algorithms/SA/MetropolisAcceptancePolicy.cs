using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Metropolis acceptance rule used by simulated annealing.
/// </summary>
/// <remarks>
/// Improving and equal transitions are accepted.
///
/// A worsening transition with positive degradation delta is accepted with
/// probability exp(-delta / T).
///
/// The degradation definition is sense-aware:
/// - minimization: candidate - current;
/// - maximization: current - candidate.
///
/// References:
/// Metropolis et al., Journal of Chemical Physics 21, 1087-1092, 1953,
/// DOI 10.1063/1.1699114.
/// Kirkpatrick, Gelatt and Vecchi, Science 220, 671-680, 1983,
/// DOI 10.1126/science.220.4598.671.
/// </remarks>
public sealed class MetropolisAcceptancePolicy :
    ITrajectoryAcceptancePolicy
{
    public MetropolisAcceptancePolicy(
        double temperature)
    {
        SetTemperature(
            temperature);
    }

    public double Temperature { get; private set; }

    public void SetTemperature(
        double temperature)
    {
        if (!double.IsFinite(temperature) ||
            temperature <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature));
        }

        Temperature =
            temperature;
    }

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
            ComputeDegradation(
                context.Sense,
                context.CurrentObjective,
                context.CandidateObjective);

        double probability =
            AcceptanceProbability(
                degradation,
                Temperature);

        return
            random.NextDouble() <
            probability;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ComputeDegradation(
        OptimizationSense sense,
        double currentObjective,
        double candidateObjective)
    {
        double degradation =
            sense switch
            {
                OptimizationSense.Minimize =>
                    candidateObjective -
                    currentObjective,

                OptimizationSense.Maximize =>
                    currentObjective -
                    candidateObjective,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(sense))
            };

        return Math.Max(
            0.0,
            degradation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AcceptanceProbability(
        double degradation,
        double temperature)
    {
        if (!double.IsFinite(degradation) ||
            degradation < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(degradation));
        }

        if (!double.IsFinite(temperature) ||
            temperature <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature));
        }

        if (degradation == 0.0)
        {
            return 1.0;
        }

        return
            Math.Exp(
                -degradation /
                temperature);
    }
}