using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.LateAcceptanceHyperHeuristic;

public sealed class LateAcceptanceHyperHeuristicOptimizer :
    IHyperHeuristicOptimizer<LateAcceptanceHyperHeuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.LateAcceptanceHyperHeuristic,
            Name = "Late Acceptance Hyper-Heuristic",
            Acronym = "LA-HH",
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
                    LateAcceptanceHyperHeuristicOptimizerReferences.Primary
                }
        };

public HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        LateAcceptanceHyperHeuristicParameters parameters,
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
        double[] history = Enumerable.Repeat(HyperHeuristicToolkit.Key(current.Objective, domain.Sense), parameters.HistoryLength).ToArray();
        int activeHistoryLength = history.Length;
        double[] scores = new double[count];
        int[] lastUsed = Enumerable.Repeat(-1, count).ToArray();

        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int selected = HyperHeuristicToolkit.RandomHeuristic(domain, random);

            HyperHeuristicCandidate candidate =
                HyperHeuristicToolkit.CloneAndApply(
                    domain, current, selected, random, ref evaluations);

            double reward = HyperHeuristicToolkit.Improvement(current.Objective, candidate.Objective, domain.Sense);
            int slot = iteration % activeHistoryLength;
            double candidateKey = HyperHeuristicToolkit.Key(candidate.Objective, domain.Sense);
            double currentKey = HyperHeuristicToolkit.Key(current.Objective, domain.Sense);
            if (LateAcceptance(candidateKey, currentKey, history[slot])) current = candidate;
            history[slot] = HyperHeuristicToolkit.Key(current.Objective, domain.Sense);


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

    private static bool LateAcceptance(double candidate, double current, double historical)
    {
        return candidate <= current || candidate <= historical;
    }
}
