using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.GeneralFeasibilityPump;

public sealed class GeneralMipFeasibilityPumpOptimizer :
    IExactRepairMatheuristicOptimizer<GeneralMipFeasibilityPumpParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.GeneralMipFeasibilityPump,
            Name = "General-MIP Feasibility Pump",
            Acronym = "GFP",
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
                    GeneralMipFeasibilityPumpOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        GeneralMipFeasibilityPumpParameters parameters,
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

        double[]? lastTarget = null;
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

                trace.Add("general-feasibility-pump-feasible");
                iteration++;
                break;
            }

            double[] target =
                rounded.Values.ToArray();

            if (lastTarget is not null &&
                EqualTargets(lastTarget, target))
                PerturbTarget(
                    domain,
                    target,
                    parameters.PerturbationFraction,
                    random);

            relaxationResult =
                ExactRepairToolkit.SolveRelaxation(
                    domain,
                    new ExactRepairRequest
                    {
                        Mode = MatheuristicSolveMode.DistanceToTarget,
                        TargetValues = target,
                        NodeLimit = parameters.NodeLimit
                    },
                    cancellationToken,
                    ref relaxationSolves);

            trace.Add("general-feasibility-pump-projection");
            lastTarget = target;
            relaxation =
                relaxationResult.HasSolution
                    ? relaxationResult.Point
                    : null;
            iteration++;
        }

        if ((relaxation is null ||
             !ExactRepairToolkit.RoundRelaxation(
                 domain,
                 relaxation).IsIntegerFeasible) &&
            lastTarget is not null)
        {
            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    new ExactRepairRequest
                    {
                        Mode = MatheuristicSolveMode.DistanceToTarget,
                        TargetValues = lastTarget,
                        NodeLimit = parameters.NodeLimit
                    },
                    cancellationToken,
                    ref exactSolves);

            trace.Add("general-feasibility-pump-exact-finish");

            if (exactResult.HasSolution)
            {
                MatheuristicPoint candidate =
                    exactResult.Point ??
                    throw new InvalidOperationException(
                        "General Feasibility Pump exact finish returned no point.");

                if (ExactRepairToolkit.Better(candidate, best, domain.Sense))
                    best = candidate;
            }
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static void PerturbTarget(
        IExactRepairMatheuristicDomain domain,
        double[] target,
        double fraction,
        IRandomSource random)
    {
        int[] integerIndices =
            Enumerable.Range(0, domain.VariableKinds.Count)
                .Where(
                    index =>
                        domain.VariableKinds[index] !=
                        MatheuristicVariableKind.Continuous)
                .ToArray();

        if (integerIndices.Length == 0)
            return;

        int changes =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    fraction *
                    integerIndices.Length));

        for (int change = 0; change < changes; change++)
        {
            int index =
                integerIndices[
                    random.NextInt32(
                        integerIndices.Length)];

            if (domain.VariableKinds[index] ==
                MatheuristicVariableKind.Binary)
                target[index] =
                    target[index] >= 0.5
                        ? 0.0
                        : 1.0;
            else
                target[index] +=
                    random.NextDouble() < 0.5
                        ? -1.0
                        : 1.0;
        }
    }

    private static bool EqualTargets(
        IReadOnlyList<double> left,
        IReadOnlyList<double> right) =>
        left.Count == right.Count &&
        !left.Where(
            (value, index) =>
                value != right[index]).Any();
}
