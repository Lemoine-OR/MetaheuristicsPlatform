using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.CMSA;

public sealed class CmsaMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<CmsaMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CmsaMatheuristic,
            Name = "Construct, Merge, Solve & Adapt",
            Acronym = "CMSA",
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
                    CmsaMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        CmsaMatheuristicParameters parameters,
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
        Dictionary<int, int> ages = new();

        int iteration;
        for (iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            HashSet<int> components = new();

            foreach (int existing in ages.Keys)
                components.Add(existing);

            for (int construction = 0;
                 construction < parameters.ConstructionsPerIteration;
                 construction++)
            {
                MatheuristicPoint sample =
                    ExactRepairToolkit.Initialize(
                        domain,
                        random);

                foreach (int active in
                    ExactRepairToolkit.ActiveBinaryIndices(
                        domain,
                        sample))
                    components.Add(active);
            }

            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    new ExactRepairRequest
                    {
                        AllowedActiveIndices = components.ToArray(),
                        NodeLimit = parameters.NodeLimit
                    },
                    cancellationToken,
                    ref exactSolves);

            trace.Add("cmsa-solve");

            HashSet<int> used = new();

            if (exactResult.HasSolution)
            {
                MatheuristicPoint candidate =
                    exactResult.Point ??
                    throw new InvalidOperationException(
                        "CMSA exact solve returned no point.");

                foreach (int active in
                    ExactRepairToolkit.ActiveBinaryIndices(
                        domain,
                        candidate))
                    used.Add(active);

                if (ExactRepairToolkit.Better(candidate, best, domain.Sense))
                    best = candidate;
            }

            UpdateComponentAges(
                components,
                used,
                ages,
                parameters.MaximumAge);
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static void UpdateComponentAges(
        IEnumerable<int> components,
        IReadOnlySet<int> used,
        Dictionary<int, int> ages,
        int maximumAge)
    {
        foreach (int component in components)
        {
            if (used.Contains(component))
                ages[component] = 0;
            else
                ages[component] =
                    ages.TryGetValue(
                        component,
                        out int age)
                        ? age + 1
                        : 1;
        }

        int[] expired =
            ages.Where(
                    pair =>
                        pair.Value > maximumAge)
                .Select(
                    pair =>
                        pair.Key)
                .ToArray();

        foreach (int component in expired)
            ages.Remove(component);
    }
}
