using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.MipAdaptiveLns;

public sealed class MipAdaptiveLargeNeighborhoodSearchOptimizer :
    IExactRepairMatheuristicOptimizer<MipAdaptiveLargeNeighborhoodSearchParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.MipAdaptiveLargeNeighborhoodSearch,
            Name = "MIP-based Adaptive Large Neighborhood Search",
            Acronym = "MIP-ALNS",
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
                    MipAdaptiveLargeNeighborhoodSearchOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        MipAdaptiveLargeNeighborhoodSearchParameters parameters,
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
        List<string> trace = new(parameters.MaximumIterations);
        double destroyFraction = parameters.DestroyFraction;

        int iteration;
        for (iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            HashSet<int> destroy =
                SelectDestroySet(
                    domain.VariableKinds.Count,
                    destroyFraction,
                    random);

            Dictionary<int, double> fixings = new();

            for (int index = 0; index < domain.VariableKinds.Count; index++)
                if (!destroy.Contains(index))
                    fixings[index] = best.Values[index];

            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    new ExactRepairRequest
                    {
                        FixedValues = fixings,
                        NodeLimit = parameters.NodeLimit
                    },
                    cancellationToken,
                    ref exactSolves);

            trace.Add("mip-alns-exact-repair");

            if (exactResult.HasSolution)
            {
                MatheuristicPoint candidate =
                    exactResult.Point ??
                    throw new InvalidOperationException(
                        "MIP-ALNS exact repair returned no point.");

                if (ExactRepairToolkit.Better(candidate, best, domain.Sense))
                {
                    best = candidate;
                    destroyFraction =
                        Math.Max(
                            0.1,
                            destroyFraction * 0.9);
                    continue;
                }
            }

            destroyFraction =
                Math.Min(
                    1.0,
                    destroyFraction * 1.1);
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static HashSet<int> SelectDestroySet(
        int variableCount,
        double fraction,
        IRandomSource random)
    {
        int target =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    fraction *
                    variableCount));

        HashSet<int> selected = new();

        while (selected.Count < target)
            selected.Add(
                random.NextInt32(
                    variableCount));

        return selected;
    }
}
