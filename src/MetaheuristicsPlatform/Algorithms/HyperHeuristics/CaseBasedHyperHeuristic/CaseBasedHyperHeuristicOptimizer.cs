using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.CaseBasedHyperHeuristic;

public sealed class CaseBasedHyperHeuristicOptimizer :
    IHyperHeuristicOptimizer<CaseBasedHyperHeuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.CaseBasedHyperHeuristic,
            Name = "Case-Based Heuristic Selection",
            Acronym = "CB-HH",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families =
                MetaheuristicFamily.Other |
                MetaheuristicFamily.Hybrid,
            Mechanisms =
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.Hybrid,
            SearchSpaces =
                SearchSpaceKind.Continuous |
                SearchSpaceKind.Binary |
                SearchSpaceKind.Integer |
                SearchSpaceKind.Permutation |
                SearchSpaceKind.Combinatorial |
                SearchSpaceKind.Mixed,
            IsStochastic = true,
            References =
                new[]
                {
                    CaseBasedHyperHeuristicOptimizerReferences.Primary
                }
        };

public HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        CaseBasedHyperHeuristicParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        HyperHeuristicToolkit.ValidateDomain(domain);

        IRandomSource random =
            HyperHeuristicToolkit.CreateRandom(options, out ulong seed);

        int evaluations = 0;
        HyperHeuristicCandidate current =
            HyperHeuristicToolkit.Initialize(domain, random, ref evaluations);
        HyperHeuristicCandidate best =
            new(current.Solution.Clone(), current.Objective);
        List<string> trace = new(parameters.MaximumIterations);
        int count = domain.Heuristics.Count;
        List<CaseRecord> cases = new();

        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int selected = RetrieveCase(cases, domain.Describe(current.Solution), count, random);

            HyperHeuristicCandidate candidate =
                HyperHeuristicToolkit.CloneAndApply(
                    domain, current, selected, random, ref evaluations);

            double reward = HyperHeuristicToolkit.Improvement(current.Objective, candidate.Objective, domain.Sense);
            cases.Add(new CaseRecord(domain.Describe(current.Solution), selected, reward));
            if (cases.Count > parameters.MaximumCases) cases.RemoveAt(0);
            if (reward > 0.0) current = candidate;

            if (HyperHeuristicToolkit.Better(
                    current.Objective,
                    best.Objective,
                    domain.Sense))
                best =
                    new HyperHeuristicCandidate(
                        current.Solution.Clone(),
                        current.Objective);

            trace.Add(domain.Heuristics[selected].Id);
        }

        return HyperHeuristicToolkit.Result(
            best, trace, evaluations, parameters.MaximumIterations, seed);
    }

    private static int RetrieveCase(IReadOnlyList<CaseRecord> cases, IReadOnlyList<double> features, int count, IRandomSource random)
    {
        if (cases.Count == 0) return random.NextInt32(count);
        return cases.OrderBy(x => HyperHeuristicToolkit.FeatureDistance(x.Features, features)).ThenByDescending(x => x.Reward).First().HeuristicIndex;
    }

    private sealed record CaseRecord(double[] Features, int HeuristicIndex, double Reward);
}
