using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.ReinforcementLearningGreatDelugeHyperHeuristic;

public sealed class ReinforcementLearningGreatDelugeHyperHeuristicOptimizer :
    IHyperHeuristicOptimizer<ReinforcementLearningGreatDelugeHyperHeuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ReinforcementLearningGreatDelugeHyperHeuristic,
            Name = "Reinforcement Learning Great-Deluge Hyper-Heuristic",
            Acronym = "RL-GD-HH",
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
                    ReinforcementLearningGreatDelugeHyperHeuristicOptimizerReferences.Primary
                }
        };

public HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        ReinforcementLearningGreatDelugeHyperHeuristicParameters parameters,
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
        double[] utility = new double[count];
        double waterLevel = HyperHeuristicToolkit.Key(current.Objective, domain.Sense);
        double rain = parameters.RainSpeed * Math.Max(1.0, Math.Abs(waterLevel));

        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int selected = SelectReinforcementHeuristic(utility, parameters.Exploration, random);

            HyperHeuristicCandidate candidate =
                HyperHeuristicToolkit.CloneAndApply(
                    domain, current, selected, random, ref evaluations);

            double signed = HyperHeuristicToolkit.SignedImprovement(current.Objective, candidate.Objective, domain.Sense);
            utility[selected] = (1.0 - parameters.LearningRate) * utility[selected] + parameters.LearningRate * signed;
            double candidateKey = HyperHeuristicToolkit.Key(candidate.Objective, domain.Sense);
            double currentKey = HyperHeuristicToolkit.Key(current.Objective, domain.Sense);
            if (GreatDelugeAccept(candidateKey, currentKey, waterLevel)) current = candidate;
            waterLevel -= rain;

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

    private static int SelectReinforcementHeuristic(IReadOnlyList<double> utility, double exploration, IRandomSource random)
    {
        if (random.NextDouble() < Math.Min(1.0, exploration)) return random.NextInt32(utility.Count);
        return HyperHeuristicToolkit.BestScoreIndex(utility, random);
    }

    private static bool GreatDelugeAccept(double candidate, double current, double level)
    {
        return candidate <= current || candidate <= level;
    }
}
