using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.ImprovedFeasibilityPump;

public sealed class ImprovedFeasibilityPumpOptimizer :
    IExactRepairMatheuristicOptimizer<ImprovedFeasibilityPumpParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ImprovedFeasibilityPump,
            Name = "Improved Feasibility Pump",
            Acronym = "IFP",
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
                    ImprovedFeasibilityPumpOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        ImprovedFeasibilityPumpParameters parameters,
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
        List<string> trace = new(parameters.MaximumIterations + 1);

        MatheuristicSolveResult relaxationResult =
            ExactRepairToolkit.SolveRelaxation(
                domain,
                new ExactRepairRequest(),
                cancellationToken,
                ref relaxationSolves);

        MatheuristicPoint? relaxation =
            relaxationResult.HasSolution
                ? relaxationResult.Point
                : null;

        int iteration = 0;

        while (relaxation is not null &&
               iteration < parameters.MaximumIterations)
        {
            MatheuristicPoint rounded =
                ExactRepairToolkit.RoundRelaxation(
                    domain,
                    relaxation);

            if (rounded.IsIntegerFeasible)
            {
                if (ExactRepairToolkit.Better(
                        rounded,
                        best,
                        domain.Sense))
                    best = rounded;

                trace.Add("improved-feasibility-pump-feasible");
                iteration++;
                break;
            }

            ExactRepairRequest request =
                WeightedPumpStep(
                    rounded.Values,
                    parameters.ObjectiveWeight,
                    parameters.NodeLimit);

            relaxationResult =
                ExactRepairToolkit.SolveRelaxation(
                    domain,
                    request,
                    cancellationToken,
                    ref relaxationSolves);

            trace.Add("improved-feasibility-pump-weighted-projection");

            relaxation =
                relaxationResult.HasSolution
                    ? relaxationResult.Point
                    : null;

            iteration++;
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static ExactRepairRequest WeightedPumpStep(
        IReadOnlyList<double> target,
        double objectiveWeight,
        int nodeLimit) =>
        new()
        {
            Mode = MatheuristicSolveMode.WeightedDistanceAndObjective,
            TargetValues = target,
            OriginalObjectiveWeight = objectiveWeight,
            NodeLimit = nodeLimit
        };
}
