using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.TwoArch2;

public sealed class TwoArch2Optimizer :
    IMultiobjectiveOptimizer<TwoArch2Parameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.TwoArch2,
            Name = "Two_Arch2",
            Acronym = "Two_Arch2",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { TwoArch2References.WangJiaoYao2015 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        TwoArch2Parameters parameters,
        OptimizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        IRandomSource random =
            MultiobjectiveToolkit.CreateRandom(
                options,
                out ulong seed);

        int evaluations = 0;

        List<MoCandidate> population =
            MultiobjectiveToolkit.Initialize(
                problem,
                parameters.PopulationSize,
                random,
                ref evaluations);

        List<MoCandidate> convergenceArchive =
            UpdateConvergenceArchive(
                population,
                parameters.ConvergenceArchiveSize,
                problem.ObjectiveSenses);

        List<MoCandidate> diversityArchive =
            UpdateDiversityArchive(
                population,
                convergenceArchive,
                parameters.DiversityArchiveSize,
                parameters.DiversityNormExponent,
                problem.ObjectiveSenses);

        double mutationProbability =
            parameters.MutationProbability < 0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<MoCandidate> mating =
                new(
                    convergenceArchive.Count +
                    diversityArchive.Count);

            mating.AddRange(convergenceArchive);
            mating.AddRange(diversityArchive);

            MultiobjectiveToolkit.SortFronts(
                mating,
                problem.ObjectiveSenses);

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    MultiobjectiveToolkit.Tournament(
                        mating,
                        random);

                MoCandidate second =
                    MultiobjectiveToolkit.Tournament(
                        mating,
                        random);

                double[] child =
                    MultiobjectiveToolkit.SbxChild(
                        first.Position,
                        second.Position,
                        problem.SearchSpace,
                        random,
                        parameters.CrossoverProbability,
                        parameters.DistributionIndex);

                MultiobjectiveToolkit.PolynomialMutate(
                    child,
                    problem.SearchSpace,
                    random,
                    mutationProbability,
                    parameters.DistributionIndex);

                problem.SearchSpace.Clamp(child);

                offspring.Add(
                    MultiobjectiveToolkit.Evaluate(
                        problem,
                        child,
                        ref evaluations));
            }

            List<MoCandidate> union =
                new(
                    convergenceArchive.Count +
                    diversityArchive.Count +
                    offspring.Count);

            union.AddRange(convergenceArchive);
            union.AddRange(diversityArchive);
            union.AddRange(offspring);

            convergenceArchive =
                UpdateConvergenceArchive(
                    union,
                    parameters.ConvergenceArchiveSize,
                    problem.ObjectiveSenses);

            diversityArchive =
                UpdateDiversityArchive(
                    union,
                    convergenceArchive,
                    parameters.DiversityArchiveSize,
                    parameters.DiversityNormExponent,
                    problem.ObjectiveSenses);

            population = offspring;
        }

        List<MoCandidate> final =
            new(
                convergenceArchive.Count +
                diversityArchive.Count);

        final.AddRange(convergenceArchive);
        final.AddRange(diversityArchive);

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                final,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static List<MoCandidate> UpdateConvergenceArchive(
        IReadOnlyList<MoCandidate> candidates,
        int size,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> nondominated =
            MultiobjectiveAdvancedToolkit.Nondominated(
                candidates,
                senses);

        if (nondominated.Count <= size)
            return nondominated
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        List<MoCandidate> working =
            nondominated
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        while (working.Count > size)
        {
            double[] ideal =
                MultiobjectiveAdvancedToolkit.IdealPoint(
                    working,
                    senses);

            int remove =
                Enumerable.Range(0, working.Count)
                    .OrderByDescending(
                        index =>
                            MultiobjectiveAdvancedToolkit.RelativeObjectives(
                                working[index],
                                ideal,
                                senses)
                            .Sum())
                    .First();

            working.RemoveAt(remove);
        }

        return working;
    }

    private static List<MoCandidate> UpdateDiversityArchive(
        IReadOnlyList<MoCandidate> candidates,
        IReadOnlyList<MoCandidate> convergenceArchive,
        int size,
        double exponent,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> pool =
            MultiobjectiveAdvancedToolkit.Nondominated(
                candidates,
                senses)
            .Select(MultiobjectiveToolkit.Clone)
            .ToList();

        List<MoCandidate> selected = new();

        if (pool.Count == 0)
            return selected;

        while (selected.Count < size &&
               pool.Count > 0)
        {
            int bestIndex = 0;
            double bestDistance =
                double.NegativeInfinity;

            for (int i = 0; i < pool.Count; i++)
            {
                double[] direction =
                    MultiobjectiveAdvancedToolkit.UnitDirection(
                        pool[i],
                        candidates,
                        senses);

                double nearest =
                    double.PositiveInfinity;

                IEnumerable<MoCandidate> anchors =
                    convergenceArchive.Concat(selected);

                foreach (MoCandidate anchor in anchors)
                {
                    double[] other =
                        MultiobjectiveAdvancedToolkit.UnitDirection(
                            anchor,
                            candidates,
                            senses);

                    double lp = 0.0;

                    for (int objective = 0;
                         objective < direction.Length;
                         objective++)
                        lp +=
                            Math.Pow(
                                Math.Abs(
                                    direction[objective] -
                                    other[objective]),
                                exponent);

                    lp =
                        Math.Pow(
                            lp,
                            1.0 / exponent);

                    nearest =
                        Math.Min(
                            nearest,
                            lp);
                }

                if (double.IsPositiveInfinity(nearest))
                    nearest =
                        MultiobjectiveAdvancedToolkit.NormalizeObjectives(
                            pool[i],
                            candidates,
                            senses)
                        .Sum();

                if (nearest > bestDistance)
                {
                    bestDistance = nearest;
                    bestIndex = i;
                }
            }

            selected.Add(pool[bestIndex]);
            pool.RemoveAt(bestIndex);
        }

        return selected;
    }
}
