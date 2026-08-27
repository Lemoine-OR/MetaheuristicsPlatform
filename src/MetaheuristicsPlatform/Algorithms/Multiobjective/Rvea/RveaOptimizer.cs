using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaIII;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Rvea;
public sealed class RveaOptimizer : IMultiobjectiveOptimizer<RveaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.Rvea,
        Name = "Reference Vector Guided Evolutionary Algorithm",
        Acronym = "RVEA",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary | MetaheuristicFamily.DecompositionBased,
        Mechanisms = MetaheuristicMechanism.Adaptive,
        SearchSpaces = SearchSpaceKind.Continuous,
        IsStochastic = true,
        References = new[] { RveaReferences.ChengJinOlhoferSendhoff2016 }
    };
    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        RveaParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();
        IRandomSource random = MultiobjectiveToolkit.CreateRandom(options, out ulong seed);
        int evaluations = 0;
        List<MoCandidate> population = MultiobjectiveToolkit.Initialize(problem, parameters.PopulationSize, random, ref evaluations);
        double[][] baseReferences = ReferenceDirectionUtilities.DasDennis(problem.ObjectiveCount, parameters.ReferenceDivisions);
        double[][] references = baseReferences.Select(vector => (double[])vector.Clone()).ToArray();
        double pm = parameters.MutationProbability < 0 ? 1.0 / problem.SearchSpace.Dimension : parameters.MutationProbability;
        for (int generation = 0; generation < parameters.MaximumGenerations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MultiobjectiveToolkit.SortFronts(population, problem.ObjectiveSenses);
            List<MoCandidate> children = new(parameters.PopulationSize);
            while (children.Count < parameters.PopulationSize)
            {
                MoCandidate first = MultiobjectiveToolkit.Tournament(population, random);
                MoCandidate second = MultiobjectiveToolkit.Tournament(population, random);
                double[] child = MultiobjectiveToolkit.SbxChild(first.Position, second.Position, problem.SearchSpace, random, parameters.CrossoverProbability, parameters.DistributionIndex);
                MultiobjectiveToolkit.PolynomialMutate(child, problem.SearchSpace, random, pm, parameters.DistributionIndex);
                problem.SearchSpace.Clamp(child);
                children.Add(MultiobjectiveToolkit.Evaluate(problem, child, ref evaluations));
            }
            population.AddRange(children);
            population = Select(
                population, parameters.PopulationSize, problem.ObjectiveSenses,
                references, parameters.Alpha,
                (generation + 1.0) / parameters.MaximumGenerations);
            if ((generation + 1) % parameters.AdaptationFrequency == 0)
                references = Adapt(baseReferences, population, problem.ObjectiveSenses);
        }
        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(population, problem.ObjectiveSenses),
            evaluations, parameters.MaximumGenerations, seed);
    }
    private static List<MoCandidate> Select(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        IReadOnlyList<OptimizationSense> senses,
        double[][] references,
        double alpha,
        double progress)
    {
        double[][] normalized = ReferenceDirectionUtilities.NormalizeObjectives(candidates, senses);
        Dictionary<int, (MoCandidate Candidate, double Apd)> best = new();
        for (int i = 0; i < candidates.Count; i++)
        {
            var association = ReferenceDirectionUtilities.Associate(normalized[i], references);
            double magnitude = Math.Sqrt(normalized[i].Sum(value => value * value));
            double ratio = association.Distance / Math.Max(magnitude, 1e-12);
            double angle = Math.Asin(Math.Clamp(ratio, 0.0, 1.0));
            double gamma = ReferenceGamma(references, association.Reference);
            double apd = magnitude * (
                1.0 +
                senses.Count *
                Math.Pow(progress, alpha) *
                angle /
                Math.Max(gamma, 1e-12));
            if (!best.TryGetValue(association.Reference, out var current) ||
                apd < current.Apd)
                best[association.Reference] = (candidates[i], apd);
        }
        List<MoCandidate> selected = best.Values
            .OrderBy(item => item.Apd)
            .Select(item => item.Candidate)
            .Take(size)
            .ToList();
        if (selected.Count < size)
        {
            foreach (MoCandidate candidate in
                MultiobjectiveToolkit.NsgaEnvironmentalSelection(candidates, size, senses))
            {
                if (!selected.Contains(candidate) && selected.Count < size)
                    selected.Add(candidate);
            }
        }
        return selected;
    }
    private static double ReferenceGamma(double[][] references, int index)
    {
        double[] first = references[index];
        double firstNorm = Math.Sqrt(first.Sum(value => value * value));
        double best = double.PositiveInfinity;
        for (int j = 0; j < references.Length; j++)
        {
            if (j == index) continue;
            double[] second = references[j];
            double secondNorm = Math.Sqrt(second.Sum(value => value * value));
            double dot = 0.0;
            for (int k = 0; k < first.Length; k++)
                dot += first[k] * second[k];
            double cosine = dot / Math.Max(firstNorm * secondNorm, 1e-12);
            double angle = Math.Acos(Math.Clamp(cosine, -1.0, 1.0));
            if (angle < best) best = angle;
        }
        return double.IsFinite(best) ? best : Math.PI / 2.0;
    }
    private static double[][] Adapt(
        double[][] baseReferences,
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses)
    {
        int objectives = senses.Count;
        double[] ranges = new double[objectives];
        for (int objective = 0; objective < objectives; objective++)
        {
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            for (int i = 0; i < population.Count; i++)
            {
                double value = MultiobjectiveToolkit.Normalize(
                    population[i].Objectives[objective], senses[objective]);
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
            ranges[objective] = Math.Max(max - min, 1e-12);
        }
        double[][] adapted = new double[baseReferences.Length][];
        for (int i = 0; i < baseReferences.Length; i++)
        {
            adapted[i] = new double[objectives];
            double norm = 0.0;
            for (int objective = 0; objective < objectives; objective++)
            {
                adapted[i][objective] = baseReferences[i][objective] * ranges[objective];
                norm += adapted[i][objective] * adapted[i][objective];
            }
            norm = Math.Sqrt(norm);
            if (norm > 0.0)
                for (int objective = 0; objective < objectives; objective++)
                    adapted[i][objective] /= norm;
        }
        return adapted;
    }
}
