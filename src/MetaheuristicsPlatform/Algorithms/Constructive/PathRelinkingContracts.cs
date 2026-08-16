using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Domain distance used by path relinking.
/// Zero means that the two solutions expose the same path-relinking attributes.
/// </summary>
public interface IPathRelinkingDistance<TSolution>
{
    /// <summary>Returns a finite non-negative integral distance between two solutions.</summary>
    int GetDistance(
        in TSolution first,
        in TSolution second,
        IOptimizationProblem<TSolution> problem);
}

/// <summary>
/// Enumerates moves that introduce attributes of a guiding solution into the current solution.
/// Implementations should expose only target-directed moves. The generic engine verifies that
/// the selected move strictly decreases the configured path distance.
/// </summary>
public interface IPathRelinkingNeighborhood<
    TSolution,
    TMove,
    TEnumerator>
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    /// <summary>Creates an allocation-free cursor over target-directed path moves.</summary>
    TEnumerator GetEnumerator(
        in TSolution current,
        in TSolution guiding,
        IOptimizationProblem<TSolution> problem);
}

/// <summary>Result of one path-relinking invocation.</summary>
public readonly record struct PathRelinkingProcedureResult<TSolution>(
    TSolution BestSolution,
    double BestFitness,
    int PathSteps,
    long CandidateEvaluations,
    bool ReachedGuidingSolution,
    StoppingDecision StoppingDecision);

/// <summary>
/// Reusable path-relinking procedure. It shares the active OptimizationContext so objective
/// probes, callbacks, stopping and best-so-far promotion remain globally exact.
/// </summary>
public interface IPathRelinkingProcedure<TSolution>
{
    /// <summary>Explores a path from an initiating solution toward a guiding elite solution.</summary>
    PathRelinkingProcedureResult<TSolution> Relink(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken);
}

/// <summary>
/// Optional advanced capability implemented by procedures that support direction,
/// mixed trajectories, truncation and greedy-randomized move selection. The guiding
/// fitness is supplied explicitly so backward and mixed strategies never re-evaluate
/// an elite endpoint merely to recover a value already known by the elite pool.
/// </summary>
public interface IAdvancedPathRelinkingProcedure<TSolution> :
    IPathRelinkingProcedure<TSolution>
{
    /// <summary>Explores one or more paths according to the supplied execution policies.</summary>
    PathRelinkingProcedureResult<TSolution> RelinkAdvanced(
        in TSolution initiatingSolution,
        double initiatingFitness,
        in TSolution guidingSolution,
        double guidingFitness,
        PathRelinkingExecutionOptions executionOptions,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumPathSteps,
        CancellationToken cancellationToken);
}