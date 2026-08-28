using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.HyperHeuristics;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.GraphBasedHyperHeuristic;

public sealed class GraphBasedHyperHeuristicOptimizer :
    IHyperHeuristicOptimizer<GraphBasedHyperHeuristicParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.GraphBasedHyperHeuristic,
            Name = "Graph-Based Hyper-Heuristic",
            Acronym = "GB-HH",
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
                    GraphBasedHyperHeuristicOptimizerReferences.Primary
                }
        };

public HyperHeuristicOptimizationResult Optimize(
        IHyperHeuristicDomain domain,
        GraphBasedHyperHeuristicParameters parameters,
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
        int count = domain.Heuristics.Count;
        int[] permutation = Enumerable.Range(0, count).ToArray();
        Shuffle(permutation, random);
        SequenceEvaluation best =
            EvaluateHeuristicPermutation(domain, permutation, random, ref evaluations);
        SequenceEvaluation current = best;
        Dictionary<string,int> tabuUntil = new(StringComparer.Ordinal);
        List<string> trace = new();

        for (int iteration=0; iteration<parameters.MaximumIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SequenceEvaluation? chosen = null;
            string? chosenMove = null;

            for (int trial=0; trial<Math.Max(4,count); trial++)
            {
                int i=random.NextInt32(count);
                int j;
                do { j=random.NextInt32(count); } while(j==i);
                int[] candidatePermutation=(int[])permutation.Clone();
                (candidatePermutation[i],candidatePermutation[j])=(candidatePermutation[j],candidatePermutation[i]);
                string move=MoveKey(candidatePermutation[i],candidatePermutation[j]);
                SequenceEvaluation candidate=EvaluateHeuristicPermutation(domain,candidatePermutation,random,ref evaluations);
                bool tabu=tabuUntil.TryGetValue(move,out int until) && until>iteration;
                bool aspiration=HyperHeuristicToolkit.Better(candidate.Objective,best.Objective,domain.Sense);
                if(tabu && !aspiration) continue;
                if(chosen is null || HyperHeuristicToolkit.Better(candidate.Objective,chosen.Objective,domain.Sense))
                { chosen=candidate; chosenMove=move; }
            }

            if(chosen is null || chosenMove is null) continue;
            current=chosen;
            permutation=(int[])chosen.Permutation.Clone();
            tabuUntil[chosenMove]=iteration+parameters.TabuTenure;
            if(HyperHeuristicToolkit.Better(current.Objective,best.Objective,domain.Sense)) best=current;
            foreach(int index in permutation) trace.Add(domain.Heuristics[index].Id);
        }

        return new HyperHeuristicOptimizationResult(
            best.Solution,best.Objective,trace,evaluations,parameters.MaximumIterations,seed);
    }

    private static SequenceEvaluation EvaluateHeuristicPermutation(
        IHyperHeuristicDomain domain,
        IReadOnlyList<int> permutation,
        IRandomSource random,
        ref int evaluations)
    {
        IHyperHeuristicSolution solution=domain.CreateInitial(random);
        foreach(int index in permutation) domain.Heuristics[index].Apply(solution,random);
        double objective=domain.Evaluate(solution);
        if(!double.IsFinite(objective)) throw new InvalidOperationException("Graph-based hyper-heuristic evaluation must be finite.");
        evaluations++;
        return new SequenceEvaluation(permutation.ToArray(),solution,objective);
    }

    private static void Shuffle(Span<int> values, IRandomSource random)
    {
        for(int i=values.Length-1;i>0;i--)
        {
            int j=random.NextInt32(i+1);
            (values[i],values[j])=(values[j],values[i]);
        }
    }

    private static string MoveKey(int first,int second)
    {
        int low=Math.Min(first,second);
        int high=Math.Max(first,second);
        return low.ToString(System.Globalization.CultureInfo.InvariantCulture)+":"+high.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class SequenceEvaluation
    {
        public SequenceEvaluation(int[] permutation,IHyperHeuristicSolution solution,double objective)
        {
            Permutation=permutation; Solution=solution.Clone(); Objective=objective;
        }
        public int[] Permutation { get; }
        public IHyperHeuristicSolution Solution { get; }
        public double Objective { get; }
    }
}
