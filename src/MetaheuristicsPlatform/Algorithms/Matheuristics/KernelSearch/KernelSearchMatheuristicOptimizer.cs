using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.KernelSearch;

public sealed class KernelSearchMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<KernelSearchMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.KernelSearchMatheuristic,
            Name = "Kernel Search",
            Acronym = "KS",
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
                    KernelSearchMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        KernelSearchMatheuristicParameters parameters,
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
                "Kernel Search relaxation returned no point.");

        List<int[]> buckets =
            BuildKernelBuckets(
                domain,
                relaxation,
                parameters.KernelSize,
                parameters.BucketSize,
                out HashSet<int> kernel);

        int iteration = 0;

        foreach (int[] bucket in buckets)
        {
            HashSet<int> allowed =
                new(kernel);

            allowed.UnionWith(bucket);

            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    new ExactRepairRequest
                    {
                        AllowedActiveIndices = allowed.ToArray(),
                        NodeLimit = parameters.NodeLimit
                    },
                    cancellationToken,
                    ref exactSolves);

            trace.Add("kernel-search-bucket");
            iteration++;

            if (!exactResult.HasSolution)
                continue;

            MatheuristicPoint candidate =
                exactResult.Point ??
                throw new InvalidOperationException(
                    "Kernel Search exact solve returned no point.");

            if (ExactRepairToolkit.Better(candidate, best, domain.Sense))
                best = candidate;

            foreach (int active in
                ExactRepairToolkit.ActiveBinaryIndices(
                    domain,
                    candidate))
                if (allowed.Contains(active))
                    kernel.Add(active);
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static List<int[]> BuildKernelBuckets(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint relaxation,
        int kernelSize,
        int bucketSize,
        out HashSet<int> kernel)
    {
        int[] binary =
            ExactRepairToolkit.BinaryIndices(domain);

        int[] ranked =
            binary.OrderByDescending(
                    index =>
                        relaxation.Values[index])
                .ThenBy(
                    index =>
                        relaxation.ReducedCosts.Count ==
                            domain.VariableKinds.Count
                            ? Math.Abs(
                                relaxation.ReducedCosts[index])
                            : 0.0)
                .ToArray();

        kernel =
            new HashSet<int>(
                ranked.Take(
                    Math.Min(
                        kernelSize,
                        ranked.Length)));

        List<int[]> buckets = new();

        int offset = kernel.Count;

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
