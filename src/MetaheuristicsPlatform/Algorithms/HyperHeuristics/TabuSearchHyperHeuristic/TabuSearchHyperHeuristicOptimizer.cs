using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.TabuSearchHyperHeuristic;

public sealed class TabuSearchHyperHeuristicOptimizer :
    IHyperHeuristicOptimizer<TabuSearchHyperHeuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.TabuSearchHyperHeuristic,
            Name = "Tabu-Search Hyper-Heuristic",
            Acronym = "TS-HH",
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
                    TabuSearchHyperHeuristicOptimizerReferences.Primary
                }
        };

public HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        TabuSearchHyperHeuristicParameters parameters,
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
        double[] scores = new double[count];
        int[] tabuUntil = new int[count];

        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int selected = SelectNonTabuHeuristic(scores, tabuUntil, iteration, parameters.Exploration, random);

            HyperHeuristicCandidate candidate =
                HyperHeuristicToolkit.CloneAndApply(
                    domain, current, selected, random, ref evaluations);

            double signed = HyperHeuristicToolkit.SignedImprovement(current.Objective, candidate.Objective, domain.Sense);
            scores[selected] = (1.0 - parameters.LearningRate) * scores[selected] + parameters.LearningRate * Math.Max(0.0, signed);
            if (signed > 0.0) current = candidate;
            else tabuUntil[selected] = iteration + parameters.TabuTenure;

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

    private static int SelectNonTabuHeuristic(IReadOnlyList<double> scores, IReadOnlyList<int> tabuUntil, int iteration, double exploration, IRandomSource random)
    {
        int best = -1; double bestValue = double.NegativeInfinity;
        for (int i = 0; i < scores.Count; i++)
        {
            if (tabuUntil[i] > iteration)
                continue;
            double value = scores[i] + exploration * 1e-9 * random.NextDouble();
            if (value > bestValue) { bestValue = value; best = i; }
        }
        return best >= 0 ? best : random.NextInt32(scores.Count);
    }
}
