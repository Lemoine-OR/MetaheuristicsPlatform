using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.ProximitySearch;

public sealed class ProximitySearchMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<ProximitySearchMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ProximitySearchMatheuristic,
            Name = "Proximity Search",
            Acronym = "PS",
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
                    ProximitySearchMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        ProximitySearchMatheuristicParameters parameters,
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

        int iteration;
        for (iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            ExactRepairRequest request =
                BuildProximityRequest(
                    best,
                    domain.Sense,
                    parameters.MinimumImprovement,
                    parameters.NodeLimit);

            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    request,
                    cancellationToken,
                    ref exactSolves);

            trace.Add("proximity-search");

            if (!exactResult.HasSolution)
                break;

            MatheuristicPoint candidate =
                exactResult.Point ??
                throw new InvalidOperationException(
                    "Proximity Search exact solve returned no point.");

            if (!ExactRepairToolkit.Better(candidate, best, domain.Sense))
                break;

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

    private static ExactRepairRequest BuildProximityRequest(
        MatheuristicPoint incumbent,
        OptimizationSense sense,
        double minimumImprovement,
        int nodeLimit) =>
        new()
        {
            Mode = MatheuristicSolveMode.ProximityToReference,
            ReferenceValues = incumbent.Values,
            ObjectiveCutoff =
                ExactRepairToolkit.ImprovementCutoff(
                    incumbent,
                    sense,
                    minimumImprovement),
            NodeLimit = nodeLimit
        };
}
