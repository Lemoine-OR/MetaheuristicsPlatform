using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Matheuristics.DINS;

public sealed class DinsMatheuristicOptimizer :
    IExactRepairMatheuristicOptimizer<DinsMatheuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.DinsMatheuristic,
            Name = "Distance Induced Neighborhood Search",
            Acronym = "DINS",
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
                    DinsMatheuristicOptimizerReferences.Primary
                }
        };

    public MatheuristicOptimizationResult Optimize(
        IExactRepairMatheuristicDomain domain,
        DinsMatheuristicParameters parameters,
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
                    "DINS relaxation returned no point.");

            ExactRepairRequest request =
                BuildDistanceInducedRequest(
                    domain,
                    best,
                    relaxation,
                    parameters.AgreementTolerance,
                    parameters.NodeLimit);

            MatheuristicSolveResult exactResult =
                ExactRepairToolkit.SolveExact(
                    domain,
                    request,
                    cancellationToken,
                    ref exactSolves);

            trace.Add("dins");

            if (!exactResult.HasSolution)
                break;

            MatheuristicPoint candidate =
                exactResult.Point ??
                throw new InvalidOperationException(
                    "DINS exact solve returned no point.");

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

    private static ExactRepairRequest BuildDistanceInducedRequest(
        IExactRepairMatheuristicDomain domain,
        MatheuristicPoint incumbent,
        MatheuristicPoint relaxation,
        double tolerance,
        int nodeLimit)
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
                fixings[index] =
                    incumbent.Values[index];
        }

        return new ExactRepairRequest
        {
            FixedValues = fixings,
            ReferenceValues = relaxation.Values,
            DistanceLimit =
                ExactRepairToolkit.AbsoluteDistance(
                    incumbent.Values,
                    relaxation.Values),
            NodeLimit = nodeLimit
        };
    }
}
