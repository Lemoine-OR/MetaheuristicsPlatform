using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.RENS;

public sealed class RensMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<RensMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.RensMatheuristic,
            Name = "Relaxation Enforced Neighborhood Search",
            Acronym = "RENS",
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
                    RensMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        RensMatheuristicParameters parameters,
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
        List<string> trace = new(1);

        MatheuristicSolveResult relaxationResult =
            ExactRepairToolkit.SolveRelaxation(
                domain,
                new ExactRepairRequest(),
                cancellationToken,
                ref relaxationSolves);

        if (relaxationResult.HasSolution)
        {
            MatheuristicPoint relaxation =
                relaxationResult.Point ??
                throw new InvalidOperationException(
                    "RENS relaxation returned no point.");

            ExactRepairRequest request =
                BuildRensBounds(
                    domain,
                    relaxation,
                    parameters.IntegralityTolerance,
                    parameters.NodeLimit);

            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    request,
                    cancellationToken,
                    ref exactSolves);

            trace.Add("rens-optimal-rounding");

            if (exactResult.HasSolution)
            {
                MatheuristicPoint candidate =
                    exactResult.Point ??
                    throw new InvalidOperationException(
                        "RENS exact rounding returned no point.");

                if (ExactRepairToolkit.Better(candidate, best, domain.Sense))
                    best = candidate;
            }
        }

        return ExactRepairToolkit.Result(
            best,
            trace,
            exactSolves,
            relaxationSolves,
            1,
            seed);
    }

    private static ExactRepairRequest BuildRensBounds(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint relaxation,
        double tolerance,
        int nodeLimit)
    {
        Dictionary<int, double> fixings = new();
        Dictionary<int, MatheuristicVariableBound> bounds = new();

        for (int index = 0; index < domain.VariableKinds.Count; index++)
        {
            if (domain.VariableKinds[index] ==
                    MatheuristicVariableKind.Continuous)
                continue;

            double value =
                relaxation.Values[index];

            double rounded =
                Math.Round(
                    value,
                    MidpointRounding.AwayFromZero);

            if (Math.Abs(value - rounded) <= tolerance)
                fixings[index] = rounded;
            else
                bounds[index] =
                    new MatheuristicVariableBound(
                        Math.Floor(value),
                        Math.Ceiling(value));
        }

        return new ExactRepairRequest
        {
            FixedValues = fixings,
            Bounds = bounds,
            NodeLimit = nodeLimit
        };
    }
}
