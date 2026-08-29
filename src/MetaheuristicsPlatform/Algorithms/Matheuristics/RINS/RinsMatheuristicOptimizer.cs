using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.RINS;

public sealed class RinsMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<RinsMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.RinsMatheuristic,
            Name = "Relaxation Induced Neighborhood Search",
            Acronym = "RINS",
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
                    RinsMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        RinsMatheuristicParameters parameters,
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
            MatheuristicSolveResult relaxationResult =
                ExactRepairToolkit.SolveRelaxation(
                    domain,
                    new ExactRepairRequest(),
                    cancellationToken,
                    ref relaxationSolves);

            if (!relaxationResult.HasSolution)
                break;

            MatheuristicPoint relaxation =
                relaxationResult.Point ??
                throw new InvalidOperationException(
                    "RINS relaxation returned no point.");

            IReadOnlyDictionary<int, double> fixings =
                BuildRinsFixings(
                    domain,
                    best,
                    relaxation,
                    parameters.AgreementTolerance);

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

            trace.Add("rins");

            if (!exactResult.HasSolution)
                break;

            MatheuristicPoint candidate =
                exactResult.Point ??
                throw new InvalidOperationException(
                    "RINS exact solve returned no point.");

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

    private static IReadOnlyDictionary<int, double> BuildRinsFixings(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint incumbent,
        MatheuristicPoint relaxation,
        double tolerance)
    {
        Dictionary<int, double> fixings = new();

        for (int index = 0; index < domain.VariableKinds.Count; index++)
        {
            if (domain.VariableKinds[index] ==
                    MatheuristicVariableKind.Continuous)
                continue;

            if (Math.Abs(
                    incumbent.Values[index] -
                    relaxation.Values[index]) <= tolerance)
                fixings[index] = incumbent.Values[index];
        }

        return fixings;
    }
}
