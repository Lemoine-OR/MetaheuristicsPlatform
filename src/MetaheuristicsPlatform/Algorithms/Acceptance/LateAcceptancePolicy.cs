using System.Runtime.CompilerServices;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Acceptance;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

/// <summary>
/// Final Burke-Bykov Late Acceptance Hill-Climbing acceptance controller.
/// </summary>
/// <remarks>
/// For minimization, a candidate is accepted when it strictly improves the active
/// history value or is not worse than the current solution. The active history value
/// is replaced only by a strict improvement of the resulting current objective.
/// Maximization is mirrored through <see cref="OptimizationSense"/>.
/// </remarks>
public sealed class LateAcceptancePolicy : ITrajectoryAcceptancePolicy
{
    private readonly double[] _history;
    private int _index;

    public LateAcceptancePolicy(int historyLength, double initialObjective)
    {
        if (historyLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(historyLength));

        if (!double.IsFinite(initialObjective))
            throw new ArgumentOutOfRangeException(nameof(initialObjective));

        _history = new double[historyLength];
        Array.Fill(_history, initialObjective);
    }

    public int HistoryLength => _history.Length;

    public int CurrentIndex => _index;

    public double CurrentReference => _history[_index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAccept(
        in TrajectoryAcceptanceContext context,
        IRandomSource random)
    {
        double historyReference = _history[_index];

        return
            TrajectoryObjectiveComparison.IsBetter(
                context.Sense,
                context.CandidateObjective,
                historyReference) ||
            IsNoWorse(
                context.Sense,
                context.CandidateObjective,
                context.CurrentObjective);
    }

    /// <summary>
    /// Completes one sampled-candidate iteration using the objective of the resulting
    /// current state, whether the candidate was accepted or rejected.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CompleteTransition(
        OptimizationSense sense,
        double resultingObjective)
    {
        if (TrajectoryObjectiveComparison.IsBetter(
                sense,
                resultingObjective,
                _history[_index]))
        {
            _history[_index] = resultingObjective;
        }

        _index++;
        if (_index == _history.Length)
            _index = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNoWorse(
        OptimizationSense sense,
        double candidate,
        double current) =>
        TrajectoryObjectiveComparison.IsBetter(sense, candidate, current) ||
        TrajectoryObjectiveComparison.IsEqual(candidate, current);
}
