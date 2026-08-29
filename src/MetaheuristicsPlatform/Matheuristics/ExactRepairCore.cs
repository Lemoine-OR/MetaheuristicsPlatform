using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Matheuristics;

public enum MatheuristicVariableKind
{
    Continuous = 0,
    Integer = 1,
    Binary = 2
}

public enum MatheuristicSolveMode
{
    OriginalObjective = 0,
    DistanceToTarget = 1,
    WeightedDistanceAndObjective = 2,
    ProximityToReference = 3
}

public readonly record struct MatheuristicVariableBound(double Lower, double Upper);

public sealed class MatheuristicPoint
{
    public MatheuristicPoint(
        IReadOnlyList<double> values,
        double objective,
        bool isIntegerFeasible,
        IReadOnlyList<double>? reducedCosts = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (!double.IsFinite(objective))
            throw new ArgumentOutOfRangeException(nameof(objective));

        Values = values.ToArray();
        Objective = objective;
        IsIntegerFeasible = isIntegerFeasible;
        ReducedCosts =
            reducedCosts is null
                ? Array.Empty<double>()
                : reducedCosts.ToArray();
    }

    public IReadOnlyList<double> Values { get; }
    public double Objective { get; }
    public bool IsIntegerFeasible { get; }
    public IReadOnlyList<double> ReducedCosts { get; }
}

public sealed class ExactRepairRequest
{
    public MatheuristicSolveMode Mode { get; init; } = MatheuristicSolveMode.OriginalObjective;
    public IReadOnlyDictionary<int, double> FixedValues { get; init; } = new Dictionary<int, double>();
    public IReadOnlyDictionary<int, MatheuristicVariableBound> Bounds { get; init; } = new Dictionary<int, MatheuristicVariableBound>();
    public IReadOnlyCollection<int>? AllowedActiveIndices { get; init; }
    public IReadOnlyList<double>? TargetValues { get; init; }
    public IReadOnlyList<double>? ReferenceValues { get; init; }
    public int? HammingRadius { get; init; }
    public double? DistanceLimit { get; init; }
    public double? ObjectiveCutoff { get; init; }
    public double OriginalObjectiveWeight { get; init; }
    public int NodeLimit { get; init; } = 1000;
}

public sealed class MatheuristicSolveResult
{
    private MatheuristicSolveResult(bool hasSolution, MatheuristicPoint? point, int exploredNodes)
    {
        HasSolution = hasSolution;
        Point = point;
        ExploredNodes = exploredNodes;
    }

    public bool HasSolution { get; }
    public MatheuristicPoint? Point { get; }
    public int ExploredNodes { get; }

    public static MatheuristicSolveResult NoSolution(int exploredNodes = 0) =>
        new(false, null, exploredNodes);

    public static MatheuristicSolveResult FromPoint(MatheuristicPoint point, int exploredNodes = 0)
    {
        ArgumentNullException.ThrowIfNull(point);
        return new MatheuristicSolveResult(true, point, exploredNodes);
    }
}

public interface IExactRepairMatheuristicDomain
{
    OptimizationSense Sense { get; }
    IReadOnlyList<MatheuristicVariableKind> VariableKinds { get; }

    MatheuristicPoint CreateInitial(IRandomSource random);

    double Evaluate(IReadOnlyList<double> values);

    bool IsIntegerFeasible(IReadOnlyList<double> values);

    MatheuristicSolveResult SolveRelaxation(
        ExactRepairRequest request,
        CancellationToken cancellationToken);

    MatheuristicSolveResult SolveExact(
        ExactRepairRequest request,
        CancellationToken cancellationToken);
}

public sealed class MatheuristicOptimizationResult
{
    public MatheuristicOptimizationResult(
        MatheuristicPoint best,
        IReadOnlyList<string> exactRepairTrace,
        int exactSolves,
        int relaxationSolves,
        int iterations,
        ulong seed)
    {
        ArgumentNullException.ThrowIfNull(best);
        ArgumentNullException.ThrowIfNull(exactRepairTrace);

        Best =
            new MatheuristicPoint(
                best.Values,
                best.Objective,
                best.IsIntegerFeasible,
                best.ReducedCosts);

        ExactRepairTrace = exactRepairTrace.ToArray();
        ExactSolves = exactSolves;
        RelaxationSolves = relaxationSolves;
        Iterations = iterations;
        Seed = seed;
    }

    public MatheuristicPoint Best { get; }
    public double BestObjective => Best.Objective;
    public IReadOnlyList<string> ExactRepairTrace { get; }
    public int ExactSolves { get; }
    public int RelaxationSolves { get; }
    public int Iterations { get; }
    public ulong Seed { get; }
}

public interface IExactRepairMatheuristicOptimizer<in TParameters>
    where TParameters : IMetaheuristicParameters
{
    MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        TParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default);
}

internal static class ExactRepairToolkit
{
    public static IRandomSource CreateRandom(OptimizationOptions? options, out ulong seed)
    {
        options ??= new OptimizationOptions();
        options.Validate();
        seed = options.Seed;
        return options.RandomSourceFactory.Create(seed);
    }

    public static void ValidateDomain(IExactRepairMatheuristicDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(domain.VariableKinds);

        if (domain.VariableKinds.Count == 0)
            throw new ArgumentException(
                "An exact-repair matheuristic domain requires at least one variable.",
                nameof(domain));
    }

    public static MatheuristicPoint Initialize(
        IExactRepairMatheuristicDomain domain,
        IRandomSource random)
    {
        MatheuristicPoint initial = domain.CreateInitial(random);
        ValidatePoint(domain, initial, requireIntegerFeasible: true);
        return initial;
    }

    public static MatheuristicSolveResult SolveRelaxation(
        IExactRepairMatheuristicDomain domain,
        ExactRepairRequest request,
        CancellationToken cancellationToken,
        ref int relaxationSolves)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateRequest(domain, request);

        MatheuristicSolveResult result =
            domain.SolveRelaxation(request, cancellationToken);

        relaxationSolves++;

        if (result.HasSolution)
        {
            if (result.Point is null)
                throw new InvalidOperationException(
                    "Relaxation solver reported a solution without a point.");

            ValidatePoint(domain, result.Point, requireIntegerFeasible: false);

            if (result.Point.ReducedCosts.Count != 0 &&
                result.Point.ReducedCosts.Count != domain.VariableKinds.Count)
                throw new InvalidOperationException(
                    "Relaxation reduced costs must be empty or match the variable count.");
        }

        return result;
    }

    public static MatheuristicSolveResult SolveExact(
        IExactRepairMatheuristicDomain domain,
        ExactRepairRequest request,
        CancellationToken cancellationToken,
        ref int exactSolves)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateRequest(domain, request);

        MatheuristicSolveResult result =
            domain.SolveExact(request, cancellationToken);

        exactSolves++;

        if (result.HasSolution)
        {
            if (result.Point is null)
                throw new InvalidOperationException(
                    "Exact solver reported a solution without a point.");

            ValidatePoint(domain, result.Point, requireIntegerFeasible: true);
        }

        return result;
    }

    public static MatheuristicPoint RoundRelaxation(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint relaxation)
    {
        double[] values = relaxation.Values.ToArray();

        for (int index = 0; index < values.Length; index++)
        {
            switch (domain.VariableKinds[index])
            {
                case MatheuristicVariableKind.Binary:
                    values[index] = values[index] >= 0.5 ? 1.0 : 0.0;
                    break;

                case MatheuristicVariableKind.Integer:
                    values[index] =
                        Math.Round(values[index], MidpointRounding.AwayFromZero);
                    break;
            }
        }

        double objective = domain.Evaluate(values);

        if (!double.IsFinite(objective))
            throw new InvalidOperationException(
                "Rounded objective must be finite.");

        return new MatheuristicPoint(
            values,
            objective,
            domain.IsIntegerFeasible(values));
    }

    public static bool Better(
        MatheuristicPoint candidate,
        MatheuristicPoint incumbent,
        OptimizationSense sense) =>
        sense == OptimizationSense.Minimize
            ? candidate.Objective < incumbent.Objective
            : candidate.Objective > incumbent.Objective;

    public static double ImprovementCutoff(
        MatheuristicPoint incumbent,
        OptimizationSense sense,
        double amount) =>
        sense == OptimizationSense.Minimize
            ? incumbent.Objective - amount
            : incumbent.Objective + amount;

    public static double AbsoluteDistance(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second)
    {
        if (first.Count != second.Count)
            throw new ArgumentException(
                "Distance vectors must have the same dimension.");

        double distance = 0.0;

        for (int index = 0; index < first.Count; index++)
            distance += Math.Abs(first[index] - second[index]);

        return distance;
    }

    public static int HammingDistance(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second,
        IReadOnlyList<MatheuristicVariableKind> kinds)
    {
        if (first.Count != second.Count ||
            first.Count != kinds.Count)
            throw new ArgumentException(
                "Hamming vectors and variable kinds must have the same dimension.");

        int distance = 0;

        for (int index = 0; index < first.Count; index++)
        {
            if (kinds[index] != MatheuristicVariableKind.Binary)
                continue;

            if ((first[index] >= 0.5) != (second[index] >= 0.5))
                distance++;
        }

        return distance;
    }

    public static int[] BinaryIndices(IExactRepairMatheuristicDomain domain) =>
        Enumerable.Range(0, domain.VariableKinds.Count)
            .Where(index => domain.VariableKinds[index] == MatheuristicVariableKind.Binary)
            .ToArray();

    public static int[] ActiveBinaryIndices(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint point) =>
        BinaryIndices(domain)
            .Where(index => point.Values[index] >= 0.5)
            .ToArray();

    public static MatheuristicOptimizationResult Result(
        MatheuristicPoint best,
        IReadOnlyList<string> trace,
        int exactSolves,
        int relaxationSolves,
        int iterations,
        ulong seed) =>
        new(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iterations,
            seed);

    private static void ValidatePoint(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint point,
        bool requireIntegerFeasible)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Values.Count != domain.VariableKinds.Count)
            throw new InvalidOperationException(
                "Matheuristic point dimension does not match the domain variable count.");

        if (!double.IsFinite(point.Objective))
            throw new InvalidOperationException(
                "Matheuristic point objective must be finite.");

        if (requireIntegerFeasible &&
            (!point.IsIntegerFeasible ||
             !domain.IsIntegerFeasible(point.Values)))
            throw new InvalidOperationException(
                "Exact-repair point must be integer feasible.");
    }

    private static void ValidateRequest(
        IExactRepairMatheuristicDomain domain,
        ExactRepairRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.NodeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.NodeLimit));

        if (!double.IsFinite(request.OriginalObjectiveWeight) ||
            request.OriginalObjectiveWeight < 0.0 ||
            request.OriginalObjectiveWeight > 1.0)
            throw new ArgumentOutOfRangeException(
                nameof(request.OriginalObjectiveWeight));

        if (request.TargetValues is not null &&
            request.TargetValues.Count != domain.VariableKinds.Count)
            throw new ArgumentException(
                "Target vector dimension mismatch.",
                nameof(request));

        if (request.ReferenceValues is not null &&
            request.ReferenceValues.Count != domain.VariableKinds.Count)
            throw new ArgumentException(
                "Reference vector dimension mismatch.",
                nameof(request));

        foreach (KeyValuePair<int, double> fixedValue in request.FixedValues)
        {
            if (fixedValue.Key < 0 ||
                fixedValue.Key >= domain.VariableKinds.Count ||
                !double.IsFinite(fixedValue.Value))
                throw new ArgumentException(
                    "Invalid exact-repair fixing.",
                    nameof(request));
        }

        foreach (KeyValuePair<int, MatheuristicVariableBound> bound in request.Bounds)
        {
            if (bound.Key < 0 ||
                bound.Key >= domain.VariableKinds.Count ||
                !double.IsFinite(bound.Value.Lower) ||
                !double.IsFinite(bound.Value.Upper) ||
                bound.Value.Lower > bound.Value.Upper)
                throw new ArgumentException(
                    "Invalid exact-repair variable bound.",
                    nameof(request));
        }

        if (request.AllowedActiveIndices is not null)
        {
            foreach (int index in request.AllowedActiveIndices)
            {
                if (index < 0 || index >= domain.VariableKinds.Count)
                    throw new ArgumentException(
                        "Invalid allowed-active variable index.",
                        nameof(request));
            }
        }
    }
}
