using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.LocalBranching;

public sealed class LocalBranchingMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<LocalBranchingMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.LocalBranchingMatheuristic,
            Name = "Local Branching",
            Acronym = "LB",
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
                    LocalBranchingMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        LocalBranchingMatheuristicParameters parameters,
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
                BuildLocalBranchingRequest(
                    best,
                    parameters.HammingRadius,
                    parameters.NodeLimit);

            MatheuristicSolveResult result =
                ExactRepairToolkit.SolveExact(
                    domain,
                    request,
                    cancellationToken,
                    ref exactSolves);

            trace.Add("local-branching");

            if (!result.HasSolution)
                break;

            MatheuristicPoint candidate =
                result.Point ??
                throw new InvalidOperationException(
                    "Local Branching exact solve returned no point.");

            if (!ExactRepairToolkit.Better(
                    candidate,
                    best,
                    domain.Sense))
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

    private static ExactRepairRequest BuildLocalBranchingRequest(
        MatheuristicPoint incumbent,
        int hammingRadius,
        int nodeLimit) =>
        new()
        {
            Mode = MatheuristicSolveMode.OriginalObjective,
            ReferenceValues = incumbent.Values,
            HammingRadius = hammingRadius,
            NodeLimit = nodeLimit
        };
}
