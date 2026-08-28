using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.HyperHeuristics;

public interface IHyperHeuristicSolution
{
    IHyperHeuristicSolution Clone();
}

public interface ILowLevelHeuristic
{
    string Id { get; }

    void Apply(
        IHyperHeuristicSolution solution,
        IRandomSource random);
}

public interface IHyperHeuristicDomain
{
    OptimizationSense Sense { get; }

    IReadOnlyList<ILowLevelHeuristic> Heuristics { get; }

    IHyperHeuristicSolution CreateInitial(
        IRandomSource random);

    double Evaluate(
        IHyperHeuristicSolution solution);

    double[] Describe(
        IHyperHeuristicSolution solution);
}

public sealed class HyperHeuristicOptimizationResult
{
    public HyperHeuristicOptimizationResult(
        IHyperHeuristicSolution bestSolution,
        double bestObjective,
        IReadOnlyList<string> heuristicTrace,
        int evaluations,
        int iterations,
        ulong seed)
    {
        ArgumentNullException.ThrowIfNull(bestSolution);
        ArgumentNullException.ThrowIfNull(heuristicTrace);

        BestSolution = bestSolution.Clone();
        BestObjective = bestObjective;
        HeuristicTrace = heuristicTrace.ToArray();
        Evaluations = evaluations;
        Iterations = iterations;
        Seed = seed;
    }

    public IHyperHeuristicSolution BestSolution { get; }
    public double BestObjective { get; }
    public IReadOnlyList<string> HeuristicTrace { get; }
    public int Evaluations { get; }
    public int Iterations { get; }
    public ulong Seed { get; }
}

public interface IHyperHeuristicOptimizer<in TParameters>
    where TParameters : IMetaheuristicParameters
{
    HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        TParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default);
}

internal sealed class HyperHeuristicCandidate
{
    public HyperHeuristicCandidate(
        IHyperHeuristicSolution solution,
        double objective)
    {
        Solution = solution;
        Objective = objective;
    }

    public IHyperHeuristicSolution Solution { get; }
    public double Objective { get; }
}

internal static class HyperHeuristicToolkit
{
    public static IRandomSource CreateRandom(
        OptimizationOptions? options,
        out ulong seed)
    {
        options ??= new OptimizationOptions();
        options.Validate();
        seed = options.Seed;
        return options.RandomSourceFactory.Create(seed);
    }

    public static void ValidateDomain(
        IHyperHeuristicDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(domain.Heuristics);

        if (domain.Heuristics.Count == 0)
            throw new ArgumentException(
                "A hyper-heuristic domain requires at least one low-level heuristic.",
                nameof(domain));

        HashSet<string> ids =
            new(StringComparer.Ordinal);

        foreach (ILowLevelHeuristic heuristic in domain.Heuristics)
        {
            ArgumentNullException.ThrowIfNull(heuristic);

            if (string.IsNullOrWhiteSpace(heuristic.Id))
                throw new ArgumentException(
                    "Low-level heuristic IDs must be non-empty.",
                    nameof(domain));

            if (!ids.Add(heuristic.Id))
                throw new ArgumentException(
                    "Low-level heuristic IDs must be unique.",
                    nameof(domain));
        }
    }

    public static HyperHeuristicCandidate Initialize(
        IHyperHeuristicDomain domain,
        IRandomSource random,
        ref int evaluations)
    {
        IHyperHeuristicSolution solution =
            domain.CreateInitial(random);

        return Evaluate(
            domain,
            solution,
            ref evaluations);
    }

    public static HyperHeuristicCandidate CloneAndApply(
        IHyperHeuristicDomain domain,
        HyperHeuristicCandidate current,
        int heuristicIndex,
        IRandomSource random,
        ref int evaluations)
    {
        IHyperHeuristicSolution solution =
            current.Solution.Clone();

        domain.Heuristics[heuristicIndex].Apply(
            solution,
            random);

        return Evaluate(
            domain,
            solution,
            ref evaluations);
    }

    public static HyperHeuristicCandidate Evaluate(
        IHyperHeuristicDomain domain,
        IHyperHeuristicSolution solution,
        ref int evaluations)
    {
        double objective =
            domain.Evaluate(solution);

        if (!double.IsFinite(objective))
            throw new InvalidOperationException(
                "Hyper-heuristic domain evaluation must return a finite value.");

        evaluations++;

        return new HyperHeuristicCandidate(
            solution,
            objective);
    }

    public static double Key(
        double objective,
        OptimizationSense sense)
    {
        return sense == OptimizationSense.Minimize
            ? objective
            : -objective;
    }

    public static bool Better(
        double left,
        double right,
        OptimizationSense sense)
    {
        return Key(left, sense) <
               Key(right, sense);
    }

    public static double SignedImprovement(
        double before,
        double after,
        OptimizationSense sense)
    {
        return
            Key(before, sense) -
            Key(after, sense);
    }

    public static double Improvement(
        double before,
        double after,
        OptimizationSense sense)
    {
        return Math.Max(
            0.0,
            SignedImprovement(
                before,
                after,
                sense));
    }

    public static int RandomHeuristic(
        IHyperHeuristicDomain domain,
        IRandomSource random)
    {
        return random.NextInt32(
            domain.Heuristics.Count);
    }

    public static int BestScoreIndex(
        IReadOnlyList<double> scores,
        IRandomSource random)
    {
        double best =
            scores.Max();

        int[] ties =
            Enumerable.Range(0, scores.Count)
                .Where(index => scores[index] == best)
                .ToArray();

        return ties[
            random.NextInt32(
                ties.Length)];
    }

    public static double FeatureDistance(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second)
    {
        if (first.Count != second.Count)
            return double.PositiveInfinity;

        double total = 0.0;

        for (int i = 0; i < first.Count; i++)
        {
            double delta =
                first[i] - second[i];

            total +=
                delta * delta;
        }

        return Math.Sqrt(total);
    }

    public static HyperHeuristicOptimizationResult Result(
        HyperHeuristicCandidate best,
        IReadOnlyList<string> trace,
        int evaluations,
        int iterations,
        ulong seed)
    {
        return new HyperHeuristicOptimizationResult(
            best.Solution,
            best.Objective,
            trace,
            evaluations,
            iterations,
            seed);
    }
}
