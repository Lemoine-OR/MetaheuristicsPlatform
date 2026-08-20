using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>
/// Deterministic one-point Demon credit/energy acceptance controller.
/// </summary>
/// <remarks>
/// For minimization, let delta = f(candidate) - f(current). The candidate is accepted
/// exactly when delta &lt;= D, where D is the current non-negative Demon credit. After an
/// accepted transition, D becomes D - delta. Improving moves therefore replenish credit;
/// worsening moves spend it. Maximization mirrors the objective orientation.
/// </remarks>
public sealed class DemonAcceptancePolicy : ITrajectoryAcceptancePolicy
{
    public DemonAcceptancePolicy(double initialCredit)
    {
        if (!double.IsFinite(initialCredit) || initialCredit < 0.0)
            throw new ArgumentOutOfRangeException(nameof(initialCredit));

        InitialCredit = initialCredit;
        Credit = initialCredit;
    }

    public double InitialCredit { get; }

    public double Credit { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);
        double energyChange = ComputeEnergyChange(
            context.Sense,
            context.CurrentObjective,
            context.CandidateObjective);

        return energyChange <= Credit;
    }

    /// <summary>
    /// Applies the Demon energy balance after a completed attempted transition.
    /// Rejected candidates leave the credit unchanged.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CompleteTransition(
        OptimizationSense sense,
        in TrajectoryStepResult step)
    {
        if (!step.Accepted)
            return;

        double energyChange = ComputeEnergyChange(
            sense,
            step.PreviousObjective,
            step.CandidateObjective);

        double next = Credit - energyChange;
        if (!double.IsFinite(next))
            throw new InvalidOperationException("The Demon credit became non-finite.");

        // ShouldAccept guarantees non-negativity in exact arithmetic. Clamp only the
        // possible negative zero / last-bit round-off residue.
        Credit = Math.Max(0.0, next);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double ComputeEnergyChange(
        OptimizationSense sense,
        double currentObjective,
        double candidateObjective) =>
        sense switch
        {
            OptimizationSense.Minimize => candidateObjective - currentObjective,
            OptimizationSense.Maximize => currentObjective - candidateObjective,
            _ => throw new ArgumentOutOfRangeException(nameof(sense))
        };
}
