using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multimodal.BiobjectiveMultimodalEa;

public sealed class BiobjectiveMultimodalEaOptimizer :
    IMultimodalOptimizer<BiobjectiveMultimodalEaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.BiobjectiveMultimodalEa,
            Name = "Bi-Objective Evolutionary Multimodal Optimizer",
            Acronym = "BiOMEA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    BiobjectiveMultimodalEaOptimizerReferences.Primary
                }
        };

public MultimodalOptimizationResult Optimize(
        IContinuousMultimodalOptimizationProblem problem,
        BiobjectiveMultimodalEaParameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        IRandomSource random =
            MultimodalToolkit.CreateRandom(
                options,
                out ulong seed);

        int evaluations = 0;
        List<MultimodalCandidate> population =
            MultimodalToolkit.Initialize(
                problem,
                parameters.PopulationSize,
                random,
                ref evaluations);

        double mutationProbability =
            parameters.MutationProbability < 0.0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<MultimodalCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count < parameters.PopulationSize)
            {
                MultimodalCandidate first =
                    population[
                        random.NextInt32(
                            population.Count)];

                MultimodalCandidate second =
                    population[
                        random.NextInt32(
                            population.Count)];

                double[] child =
                    MultimodalToolkit.SbxChild(
                        first.Position,
                        second.Position,
                        problem.SearchSpace,
                        random,
                        parameters.CrossoverProbability,
                        parameters.DistributionIndex);

                MultimodalToolkit.PolynomialMutate(
                    child,
                    problem.SearchSpace,
                    random,
                    mutationProbability,
                    parameters.DistributionIndex);

                offspring.Add(
                    MultimodalToolkit.Evaluate(
                        problem,
                        child,
                        ref evaluations));
            }

            List<MultimodalCandidate> union =
                new(population.Count + offspring.Count);

            union.AddRange(population);
            union.AddRange(offspring);

            population =
                BuildBiobjectiveScores(
                    union,
                    parameters.PopulationSize,
                    problem,
                    parameters.GradientStep,
                    ref evaluations);
        }

        return new MultimodalOptimizationResult(
            MultimodalToolkit.ExtractDistinctOptima(
                population,
                problem.Sense,
                parameters.NicheRadius,
                parameters.MaximumOptima),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static List<MultimodalCandidate> BuildBiobjectiveScores(
        IReadOnlyList<MultimodalCandidate> candidates,
        int populationSize,
        IContinuousMultimodalOptimizationProblem problem,
        double gradientStep,
        ref int evaluations)
    {
        double[] gradientNorm =
            new double[candidates.Count];

        for (int i = 0; i < candidates.Count; i++)
            gradientNorm[i] =
                GradientNorm(
                    candidates[i].Position,
                    problem,
                    gradientStep,
                    ref evaluations);

        List<int> remaining =
            Enumerable.Range(0, candidates.Count)
                .ToList();

        List<MultimodalCandidate> selected =
            new(populationSize);

        while (remaining.Count > 0 &&
               selected.Count < populationSize)
        {
            List<int> front = new();

            foreach (int i in remaining)
            {
                bool dominated = false;

                foreach (int j in remaining)
                {
                    if (i == j)
                        continue;

                    double leftObjective =
                        MultimodalToolkit.Key(
                            candidates[j].Objective,
                            problem.Sense);

                    double rightObjective =
                        MultimodalToolkit.Key(
                            candidates[i].Objective,
                            problem.Sense);

                    bool noWorseObjective =
                        leftObjective <= rightObjective;

                    bool noWorseGradient =
                        gradientNorm[j] <=
                        gradientNorm[i];

                    bool strictlyBetter =
                        leftObjective < rightObjective ||
                        gradientNorm[j] <
                        gradientNorm[i];

                    if (noWorseObjective &&
                        noWorseGradient &&
                        strictlyBetter)
                    {
                        dominated = true;
                        break;
                    }
                }

                if (!dominated)
                    front.Add(i);
            }

            foreach (int i in
                front
                    .OrderBy(index =>
                        gradientNorm[index])
                    .ThenBy(index =>
                        MultimodalToolkit.Key(
                            candidates[index].Objective,
                            problem.Sense)))
            {
                if (selected.Count >= populationSize)
                    break;

                selected.Add(candidates[i]);
            }

            foreach (int i in front)
                remaining.Remove(i);
        }

        return selected;
    }

    private static double GradientNorm(
        ReadOnlySpan<double> position,
        IContinuousMultimodalOptimizationProblem problem,
        double relativeStep,
        ref int evaluations)
    {
        ReadOnlySpan<double> lower =
            problem.SearchSpace.LowerBounds;

        ReadOnlySpan<double> upper =
            problem.SearchSpace.UpperBounds;

        double squaredNorm = 0.0;

        for (int d = 0; d < position.Length; d++)
        {
            double width =
                upper[d] - lower[d];

            double step =
                Math.Max(
                    relativeStep * width,
                    1e-12);

            double[] plus = position.ToArray();
            double[] minus = position.ToArray();

            plus[d] =
                Math.Min(
                    upper[d],
                    position[d] + step);

            minus[d] =
                Math.Max(
                    lower[d],
                    position[d] - step);

            double denominator =
                plus[d] - minus[d];

            if (denominator <= 0.0)
                continue;

            double fPlus =
                problem.Evaluate(plus);

            double fMinus =
                problem.Evaluate(minus);

            evaluations += 2;

            double derivative =
                (fPlus - fMinus) /
                denominator;

            squaredNorm +=
                derivative * derivative;
        }

        return Math.Sqrt(squaredNorm);
    }

}
