using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.KernelPump;

public sealed class KernelPumpMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<KernelPumpMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.KernelPumpMatheuristic,
            Name = "Kernel Pump",
            Acronym = "KP",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families =
                MetaheuristicFamily.Other |
                MetaheuristicFamily.Hybrid,
            Mechanisms =
                MetaheuristicMechanism.Hybrid |
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.Decomposition,
            SearchSpaces =
                SearchSpaceKind.Continuous |
                SearchSpaceKind.Binary |
                SearchSpaceKind.Integer |
                SearchSpaceKind.Combinatorial |
                SearchSpaceKind.Mixed,
            IsStochastic = true,
            References =
                new[]
                {
                    KernelPumpMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        KernelPumpMatheuristicParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        ExactRepairToolkit.ValidateDomain(domain);

        IRandomSource random =
            ExactRepairToolkit.CreateRandom(options, out ulong seed);

        MatheuristicPoint best =
            ExactRepairToolkit.Initialize(domain, random);

        int exactSolves = 0;
        int relaxationSolves = 0;
        List<string> trace = new();

        MatheuristicSolveResult relaxationResult =
            ExactRepairToolkit.SolveRelaxation(
                domain,
                new ExactRepairRequest(),
                cancellationToken,
                ref relaxationSolves);

        if (!relaxationResult.HasSolution)
            return ExactRepairToolkit.Result(
                best, trace, exactSolves, relaxationSolves, 0, seed);

        MatheuristicPoint relaxation =
            relaxationResult.Point ??
            throw new InvalidOperationException(
                "Kernel Pump initial relaxation returned no point.");

        List<int[]> buckets =
            BuildKernelPumpBuckets(
                domain,
                relaxation,
                parameters.BucketCount,
                out HashSet<int> kernel);

        int iteration = 0;

        foreach (int[] bucket in buckets)
        {
            kernel.UnionWith(bucket);

            MatheuristicPoint currentRelaxation =
                relaxation;

            for (int pump = 0;
                 pump < parameters.MaximumPumpIterations;
                 pump++)
            {
                MatheuristicPoint target =
                    ExactRepairToolkit.RoundRelaxation(
                        domain,
                        currentRelaxation);

                if (target.IsIntegerFeasible)
                {
                    if (ExactRepairToolkit.Better(
                            target,
                            best,
                            domain.Sense))
                        best = target;

                    trace.Add("kernel-pump-feasible");
                    iteration++;
                    return ExactRepairToolkit.Result(
                        best,
                        trace,
                        exactSolves,
                        relaxationSolves,
                        iteration,
                        seed);
                }

                MatheuristicSolveResult projected =
                    ExactRepairToolkit.SolveRelaxation(
                        domain,
                        new ExactRepairRequest
                        {
                            Mode = MatheuristicSolveMode.DistanceToTarget,
                            TargetValues = target.Values,
                            AllowedActiveIndices = kernel.ToArray(),
                            NodeLimit = parameters.NodeLimit
                        },
                        cancellationToken,
                        ref relaxationSolves);

                trace.Add("kernel-pump-projection");
                iteration++;

                if (!projected.HasSolution)
                    break;

                currentRelaxation =
                    projected.Point ??
                    throw new InvalidOperationException(
                        "Kernel Pump projection returned no point.");
            }
        }

        MatheuristicSolveResult exactFinish =
            ExactRepairToolkit.SolveExact(
                domain,
                new ExactRepairRequest
                {
                    AllowedActiveIndices =
                        ExactRepairToolkit.BinaryIndices(domain),
                    NodeLimit = parameters.NodeLimit
                },
                cancellationToken,
                ref exactSolves);

        trace.Add("kernel-pump-exact-finish");

        if (exactFinish.HasSolution)
        {
            MatheuristicPoint candidate =
                exactFinish.Point ??
                throw new InvalidOperationException(
                    "Kernel Pump exact finish returned no point.");

            if (ExactRepairToolkit.Better(candidate, best, domain.Sense))
                best = candidate;
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static List<int[]> BuildKernelPumpBuckets(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint relaxation,
        int bucketCount,
        out HashSet<int> kernel)
    {
        int[] binary =
            ExactRepairToolkit.BinaryIndices(domain);

        int[] ranked =
            binary.OrderBy(
                    index =>
                        Math.Abs(
                            relaxation.Values[index] -
                            Math.Round(
                                relaxation.Values[index],
                                MidpointRounding.AwayFromZero)))
                .ThenBy(
                    index =>
                        relaxation.ReducedCosts.Count ==
                            domain.VariableKinds.Count
                            ? relaxation.ReducedCosts[index]
                            : 0.0)
                .ToArray();

        if (ranked.Length == 0)
        {
            kernel = new HashSet<int>();
            return new List<int[]>
            {
                Array.Empty<int>()
            };
        }

        int bucketSize =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    ranked.Length /
                    (double)(bucketCount + 1)));

        kernel =
            new HashSet<int>(
                ranked.Take(bucketSize));

        List<int[]> buckets = new();

        int offset = bucketSize;

        while (offset < ranked.Length)
        {
            int[] bucket =
                ranked.Skip(offset)
                    .Take(bucketSize)
                    .ToArray();

            buckets.Add(bucket);
            offset += bucket.Length;
        }

        if (buckets.Count == 0)
            buckets.Add(Array.Empty<int>());

        return buckets;
    }
}
