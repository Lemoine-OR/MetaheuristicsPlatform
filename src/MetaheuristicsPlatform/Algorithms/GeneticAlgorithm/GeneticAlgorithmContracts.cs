using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Owned complete solution and its objective value inside one GA population.
/// </summary>
public sealed class GeneticPopulationMember<TSolution>
{
    public GeneticPopulationMember(
        TSolution solution,
        double objective)
    {
        Solution = solution;
        Objective = objective;
    }

    public TSolution Solution { get; }
    public double Objective { get; }
}

/// <summary>
/// Two complete offspring produced by one crossover event.
/// </summary>
public readonly record struct GeneticOffspringPair<TSolution>(
    TSolution First,
    TSolution Second);

/// <summary>
/// Creates one complete initial population member.
/// The optimizer invokes this method repeatedly until PopulationSize is reached.
/// </summary>
public interface IGeneticPopulationInitializer<TSolution>
{
    TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>
/// Selects one parent index from the current evaluated population.
/// </summary>
public interface IGeneticParentSelectionMethod<TSolution>
{
    int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random);
}

/// <summary>
/// Representation-specific recombination of two parent snapshots.
/// </summary>
public interface IGeneticCrossoverMethod<TSolution>
{
    GeneticOffspringPair<TSolution> Crossover(
        TSolution firstParent,
        TSolution secondParent,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>
/// Representation-specific mutation of one owned offspring snapshot.
/// The implementation may mutate in place or return a replacement object.
/// </summary>
public interface IGeneticMutationMethod<TSolution>
{
    TSolution Mutate(
        TSolution solution,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

public delegate TSolution GeneticPopulationInitializerDelegate<TSolution>(
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public delegate int GeneticParentSelectionDelegate<TSolution>(
    IReadOnlyList<GeneticPopulationMember<TSolution>> population,
    OptimizationSense sense,
    IRandomSource random);

public delegate GeneticOffspringPair<TSolution> GeneticCrossoverDelegate<TSolution>(
    TSolution firstParent,
    TSolution secondParent,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public delegate TSolution GeneticMutationDelegate<TSolution>(
    TSolution solution,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public sealed class DelegateGeneticPopulationInitializer<TSolution> :
    IGeneticPopulationInitializer<TSolution>
{
    private readonly GeneticPopulationInitializerDelegate<TSolution> _initializer;

    public DelegateGeneticPopulationInitializer(
        GeneticPopulationInitializerDelegate<TSolution> initializer)
    {
        _initializer =
            initializer ??
            throw new ArgumentNullException(nameof(initializer));
    }

    public TSolution Create(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _initializer(problem, random);
}

public sealed class DelegateGeneticParentSelectionMethod<TSolution> :
    IGeneticParentSelectionMethod<TSolution>
{
    private readonly GeneticParentSelectionDelegate<TSolution> _selection;

    public DelegateGeneticParentSelectionMethod(
        GeneticParentSelectionDelegate<TSolution> selection)
    {
        _selection =
            selection ??
            throw new ArgumentNullException(nameof(selection));
    }

    public int SelectParent(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random) =>
        _selection(population, sense, random);
}

public sealed class DelegateGeneticCrossoverMethod<TSolution> :
    IGeneticCrossoverMethod<TSolution>
{
    private readonly GeneticCrossoverDelegate<TSolution> _crossover;

    public DelegateGeneticCrossoverMethod(
        GeneticCrossoverDelegate<TSolution> crossover)
    {
        _crossover =
            crossover ??
            throw new ArgumentNullException(nameof(crossover));
    }

    public GeneticOffspringPair<TSolution> Crossover(
        TSolution firstParent,
        TSolution secondParent,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _crossover(
            firstParent,
            secondParent,
            problem,
            random);
}

public sealed class DelegateGeneticMutationMethod<TSolution> :
    IGeneticMutationMethod<TSolution>
{
    private readonly GeneticMutationDelegate<TSolution> _mutation;

    public DelegateGeneticMutationMethod(
        GeneticMutationDelegate<TSolution> mutation)
    {
        _mutation =
            mutation ??
            throw new ArgumentNullException(nameof(mutation));
    }

    public TSolution Mutate(
        TSolution solution,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _mutation(
            solution,
            problem,
            random);
}
