using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Allocation-free cursor over feasible construction components for one ant.</summary>
public interface IAntColonyCandidateEnumerator<TComponent>
{
    /// <summary>Moves to the next feasible construction component.</summary>
    bool MoveNext(out TComponent component);
}

/// <summary>
/// Domain contract used by the generic Ant System construction engine.
/// The model owns feasibility, heuristic information and the mapping from a
/// construction decision to its pheromone-memory key.
/// </summary>
public interface IAntColonyConstructionModel<
    TSolution,
    TComponent,
    TPheromoneKey,
    TEnumerator>
    where TPheromoneKey : notnull
    where TEnumerator : struct, IAntColonyCandidateEnumerator<TComponent>
{
    /// <summary>Creates a fresh empty/partial solution for one ant.</summary>
    TSolution CreateInitialSolution(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);

    /// <summary>Returns whether the partial solution is complete.</summary>
    bool IsComplete(
        in TSolution solution,
        IOptimizationProblem<TSolution> problem);

    /// <summary>Creates a fresh cursor over feasible next components.</summary>
    TEnumerator GetCandidateEnumerator(
        in TSolution solution,
        IOptimizationProblem<TSolution> problem);

    /// <summary>
    /// Maps one feasible decision to the pheromone-memory key reinforced when that
    /// decision belongs to a completed ant solution.
    /// </summary>
    TPheromoneKey GetPheromoneKey(
        in TSolution solution,
        in TComponent component,
        IOptimizationProblem<TSolution> problem);

    /// <summary>
    /// Returns the strictly-positive domain heuristic information eta for this
    /// decision when beta is positive.
    /// </summary>
    double EvaluateHeuristic(
        in TSolution solution,
        in TComponent component,
        IOptimizationProblem<TSolution> problem);

    /// <summary>Applies the selected construction component to the owned partial solution.</summary>
    void ApplyComponent(
        ref TSolution solution,
        in TComponent component,
        IOptimizationProblem<TSolution> problem);
}

/// <summary>Result of one ant construction.</summary>
public readonly record struct AntColonyConstructionResult<TSolution, TPheromoneKey>(
    TSolution Solution,
    IReadOnlyList<TPheromoneKey> PheromoneKeys,
    int ConstructionSteps,
    long TransitionEvaluations)
    where TPheromoneKey : notnull;

/// <summary>Computes the non-negative pheromone deposit assigned to one completed ant.</summary>
public interface IAntSystemDepositPolicy<TSolution>
{
    /// <summary>Stable component identifier.</summary>
    string Id { get; }

    /// <summary>Returns the deposit multiplier applied to every used pheromone key.</summary>
    double GetDeposit(
        in TSolution solution,
        double objective,
        int antIndex,
        int antCount,
        IOptimizationProblem<TSolution> problem);
}
