using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.FeasibilityPump;

public sealed class FeasibilityPumpMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<FeasibilityPumpMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.FeasibilityPumpMatheuristic,
            Name = "Feasibility Pump",
            Acronym = "FP",
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
                    FeasibilityPumpMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        FeasibilityPumpMatheuristicParameters parameters,
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

        if (!relaxationResult.HasSolution)
            return ExactRepairToolkit.Result(
                best, trace, exactSolves, relaxationSolves, 0, seed);

        MatheuristicPoint relaxation =
            relaxationResult.Point ??
            throw new InvalidOperationException(
                "Feasibility Pump relaxation returned no point.");

        double[]? previousTarget = null;

        int iteration;
        for (iteration = 0; iteration < parameters.MaximumIterations; iteration++)
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

                trace.Add("feasibility-pump-feasible");
                iteration++;
                break;
            }

            double[] target =
                rounded.Values.ToArray();

            if (previousTarget is not null &&
                SameVector(previousTarget, target))
                PerturbBinaryTarget(
                    domain,
                    target,
                    random);

            ExactRepairRequest request =
                PumpStep(
                    target,
                    parameters.NodeLimit);

            relaxationResult =
                ExactRepairToolkit.SolveRelaxation(
                    domain,
                    request,
                    cancellationToken,
                    ref relaxationSolves);

            trace.Add("feasibility-pump-projection");

            if (!relaxationResult.HasSolution)
                break;

            relaxation =
                relaxationResult.Point ??
                throw new InvalidOperationException(
                    "Feasibility Pump projection returned no point.");

            previousTarget = target;
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            iteration,
            seed);
    }

    private static ExactRepairRequest PumpStep(
        IReadOnlyList<double> target,
        int nodeLimit) =>
        new()
        {
            Mode = MatheuristicSolveMode.DistanceToTarget,
            TargetValues = target,
            NodeLimit = nodeLimit
        };

    private static bool SameVector(
        IReadOnlyList<double> left,
        IReadOnlyList<double> right)
    {
        if (left.Count != right.Count)
            return false;

        for (int index = 0; index < left.Count; index++)
            if (left[index] != right[index])
                return false;

        return true;
    }

    private static void PerturbBinaryTarget(
        IExactRepairMatheuristicDomain domain,
        double[] target,
        IRandomSource random)
    {
        int[] binary =
            ExactRepairToolkit.BinaryIndices(domain);

        if (binary.Length == 0)
            return;

        int index =
            binary[random.NextInt32(binary.Length)];

        target[index] =
            target[index] >= 0.5
                ? 0.0
                : 1.0;
    }
}
