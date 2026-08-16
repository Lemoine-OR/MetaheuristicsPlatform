using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Direction in which the domain-defined GRASP greedy score is better.</summary>
public enum GraspGreedyScoreSense
{
    /// <summary>Lower greedy scores are preferred.</summary>
    Minimize = 0,

    /// <summary>Higher greedy scores are preferred.</summary>
    Maximize = 1
}

/// <summary>Allocation-free cursor over constructive GRASP candidates.</summary>
public interface IGraspCandidateEnumerator<TCandidate>
{
    /// <summary>Moves to the next candidate.</summary>
    bool MoveNext(out TCandidate candidate);
}

/// <summary>
/// Domain model used by the canonical GRASP construction engine.
/// Scores are recomputed after every accepted component, which preserves the adaptive
/// part of Greedy Randomized Adaptive Search Procedures.
/// </summary>
public interface IGraspConstructionModel<
    TSolution,
    TCandidate,
    TEnumerator>
    where TEnumerator : struct, IGraspCandidateEnumerator<TCandidate>
{
    /// <summary>Gets whether lower or higher greedy scores are preferred.</summary>
    GraspGreedyScoreSense GreedyScoreSense { get; }

    /// <summary>Creates the initial partial solution for one GRASP construction.</summary>
    TSolution CreateInitialSolution(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);

    /// <summary>Returns whether the partial solution is complete and ready for objective evaluation.</summary>
    bool IsComplete(
        in TSolution solution,
        IOptimizationProblem<TSolution> problem);

    /// <summary>Creates a fresh candidate cursor for the current partial solution.</summary>
    TEnumerator GetCandidateEnumerator(
        in TSolution solution,
        IOptimizationProblem<TSolution> problem);

    /// <summary>
    /// Computes the domain-owned greedy score of one candidate against the current partial solution.
    /// The score must be finite and side-effect free.
    /// </summary>
    double EvaluateGreedyScore(
        in TSolution solution,
        in TCandidate candidate,
        IOptimizationProblem<TSolution> problem);

    /// <summary>Applies the selected construction component to the owned partial solution.</summary>
    void ApplyCandidate(
        ref TSolution solution,
        in TCandidate candidate,
        IOptimizationProblem<TSolution> problem);
}

/// <summary>Result of one canonical GRASP construction phase.</summary>
public readonly record struct GraspConstructionResult<TSolution>(
    TSolution Solution,
    int ConstructionSteps,
    long GreedyScoreEvaluations);

/// <summary>Reusable GRASP construction procedure.</summary>
public interface IGraspConstructionProcedure<TSolution>
{
    /// <summary>Builds one complete randomized greedy solution.</summary>
    GraspConstructionResult<TSolution> Construct(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        double alpha,
        int maximumConstructionSteps,
        CancellationToken cancellationToken);
}
