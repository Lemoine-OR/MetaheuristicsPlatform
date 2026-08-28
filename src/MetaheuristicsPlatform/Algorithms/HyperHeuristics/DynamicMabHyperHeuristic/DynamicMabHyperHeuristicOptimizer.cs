using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.DynamicMabHyperHeuristic;

public sealed class DynamicMabHyperHeuristicOptimizer :
    IHyperHeuristicOptimizer<DynamicMabHyperHeuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.DynamicMabHyperHeuristic,
            Name = "Dynamic Multi-Armed Bandit Adaptive Operator Selection",
            Acronym = "DMAB-AOS",
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
                    DynamicMabHyperHeuristicOptimizerReferences.Primary
                }
        };

public HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        DynamicMabHyperHeuristicParameters parameters,
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
        int[] pulls = new int[count];
        double[] rewards = new double[count];
        Queue<double>[] rewardWindows = Enumerable.Range(0, count).Select(_ => new Queue<double>()).ToArray();

        for (int iteration = 0; iteration < parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int selected = DynamicUcbSelect(pulls, rewards, iteration, parameters.Exploration, random);

            HyperHeuristicCandidate candidate =
                HyperHeuristicToolkit.CloneAndApply(
                    domain, current, selected, random, ref evaluations);

            double reward = HyperHeuristicToolkit.Improvement(current.Objective, candidate.Objective, domain.Sense);
            pulls[selected]++;
            rewards[selected] += (reward - rewards[selected]) / pulls[selected];
            if (reward > 0.0) current = candidate;
            if (Math.Abs(reward - rewards[selected]) > parameters.ChangeThreshold) { Array.Clear(pulls,0,pulls.Length); Array.Clear(rewards,0,rewards.Length); }

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

    private static int DynamicUcbSelect(IReadOnlyList<int> pulls, IReadOnlyList<double> rewards, int iteration, double exploration, IRandomSource random)
    {
        for (int i=0;i<pulls.Count;i++) if (pulls[i]==0) return i;
        double[] score = new double[pulls.Count];
        for (int i=0;i<score.Length;i++) score[i]=rewards[i]+exploration*Math.Sqrt(Math.Log(Math.Max(2,iteration+1))/pulls[i]);
        return HyperHeuristicToolkit.BestScoreIndex(score, random);
    }
}
