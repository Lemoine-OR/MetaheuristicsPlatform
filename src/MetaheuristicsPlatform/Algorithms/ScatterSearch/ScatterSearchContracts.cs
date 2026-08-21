using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>Owned solution plus objective value used by Scatter Search.</summary>
public sealed class ScatterSearchReferencePoint<TSolution>
{
    public ScatterSearchReferencePoint(
        TSolution solution,
        double objective,
        bool isNew)
    {
        Solution = solution;
        Objective = objective;
        IsNew = isNew;
    }

    public TSolution Solution { get; set; }
    public double Objective { get; set; }
    public bool IsNew { get; set; }
}

/// <summary>Subset of reference points supplied to the solution-combination method.</summary>
public sealed class ScatterSearchSubset<TSolution>
{
    public ScatterSearchSubset(
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> members)
    {
        ArgumentNullException.ThrowIfNull(members);

        if (members.Count < 2)
            throw new ArgumentException(
                "A Scatter Search combination subset must contain at least two reference points.",
                nameof(members));

        Members = members;
    }

    public IReadOnlyList<ScatterSearchReferencePoint<TSolution>> Members { get; }
}

/// <summary>
/// Diversification Generation Method: creates one solution candidate.
/// The optimizer invokes it repeatedly to construct the initial diversified population.
/// </summary>
public interface IScatterSearchDiversificationGenerationMethod<TSolution>
{
    TSolution Generate(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

/// <summary>
/// Optional Improvement Method. It is applied to complete solutions only.
/// </summary>
public interface IScatterSearchImprovementMethod<TSolution>
{
    void Improve(
        ref TSolution solution,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        CancellationToken cancellationToken);
}

/// <summary>
/// Representation-specific distance used for diversity control.
/// Distances must be finite and non-negative.
/// </summary>
public interface IScatterSearchDistance<TSolution>
{
    double Distance(
        in TSolution left,
        in TSolution right);
}

/// <summary>
/// Reference Set Update Method: builds and maintains the RefSet.
/// Implementations own the responsibility for cloning accepted solutions.
/// </summary>
public interface IScatterSearchReferenceSetUpdateMethod<TSolution>
{
    void Initialize(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> population,
        int referenceSetSize,
        int qualityReferenceSetSize,
        IScatterSearchDistance<TSolution> distance,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner);

    bool TryUpdate(
        IList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        ScatterSearchReferencePoint<TSolution> candidate,
        IScatterSearchDistance<TSolution> distance,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner);
}

/// <summary>
/// Subset Generation Method: selects subsets of RefSet members to combine.
/// </summary>
public interface IScatterSearchSubsetGenerationMethod<TSolution>
{
    IReadOnlyList<ScatterSearchSubset<TSolution>> Generate(
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> referenceSet);
}

/// <summary>
/// Solution Combination Method: maps one reference subset to one or more complete candidates.
/// </summary>
public interface IScatterSearchSolutionCombinationMethod<TSolution>
{
    IEnumerable<TSolution> Combine(
        ScatterSearchSubset<TSolution> subset,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random);
}

public delegate TSolution ScatterSearchDiversificationDelegate<TSolution>(
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public delegate void ScatterSearchImprovementDelegate<TSolution>(
    ref TSolution solution,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random,
    CancellationToken cancellationToken);

public delegate double ScatterSearchDistanceDelegate<TSolution>(
    in TSolution left,
    in TSolution right);

public delegate IEnumerable<TSolution> ScatterSearchCombinationDelegate<TSolution>(
    ScatterSearchSubset<TSolution> subset,
    IOptimizationProblem<TSolution> problem,
    IRandomSource random);

public sealed class DelegateScatterSearchDiversificationGenerationMethod<TSolution> :
    IScatterSearchDiversificationGenerationMethod<TSolution>
{
    private readonly ScatterSearchDiversificationDelegate<TSolution> _generator;

    public DelegateScatterSearchDiversificationGenerationMethod(
        ScatterSearchDiversificationDelegate<TSolution> generator)
    {
        _generator =
            generator ??
            throw new ArgumentNullException(nameof(generator));
    }

    public TSolution Generate(
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _generator(problem, random);
}

public sealed class DelegateScatterSearchImprovementMethod<TSolution> :
    IScatterSearchImprovementMethod<TSolution>
{
    private readonly ScatterSearchImprovementDelegate<TSolution> _improvement;

    public DelegateScatterSearchImprovementMethod(
        ScatterSearchImprovementDelegate<TSolution> improvement)
    {
        _improvement =
            improvement ??
            throw new ArgumentNullException(nameof(improvement));
    }

    public void Improve(
        ref TSolution solution,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random,
        CancellationToken cancellationToken) =>
        _improvement(
            ref solution,
            problem,
            random,
            cancellationToken);
}

public sealed class DelegateScatterSearchDistance<TSolution> :
    IScatterSearchDistance<TSolution>
{
    private readonly ScatterSearchDistanceDelegate<TSolution> _distance;

    public DelegateScatterSearchDistance(
        ScatterSearchDistanceDelegate<TSolution> distance)
    {
        _distance =
            distance ??
            throw new ArgumentNullException(nameof(distance));
    }

    public double Distance(
        in TSolution left,
        in TSolution right) =>
        _distance(in left, in right);
}

public sealed class DelegateScatterSearchSolutionCombinationMethod<TSolution> :
    IScatterSearchSolutionCombinationMethod<TSolution>
{
    private readonly ScatterSearchCombinationDelegate<TSolution> _combination;

    public DelegateScatterSearchSolutionCombinationMethod(
        ScatterSearchCombinationDelegate<TSolution> combination)
    {
        _combination =
            combination ??
            throw new ArgumentNullException(nameof(combination));
    }

    public IEnumerable<TSolution> Combine(
        ScatterSearchSubset<TSolution> subset,
        IOptimizationProblem<TSolution> problem,
        IRandomSource random) =>
        _combination(subset, problem, random);
}
