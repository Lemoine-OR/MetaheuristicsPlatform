using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Spea;

public sealed class SpeaOptimizer :
    IMultiobjectiveOptimizer<SpeaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Spea,
            Name = "Strength Pareto Evolutionary Algorithm",
            Acronym = "SPEA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    SpeaReferences.ZitzlerThiele1999
                }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        SpeaParameters parameters,
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

        List<MoCandidate> archive = new();

        double mutationProbability =
            parameters.MutationProbability < 0.0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<MoCandidate> union =
                new(
                    population.Count +
                    archive.Count);

            union.AddRange(population);
            union.AddRange(archive);

            archive =
                EnvironmentalSelection(
                    union,
                    parameters.ArchiveSize,
                    problem.ObjectiveSenses);

            AssignStrengthFitness(
                union,
                archive,
                problem.ObjectiveSenses);

            IReadOnlyList<MoCandidate> mating =
                archive.Count >= 2
                    ? archive
                    : population;

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    mating[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            mating,
                            random)];

                MoCandidate second =
                    mating[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            mating,
                            random)];

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

            population = offspring;
        }

        List<MoCandidate> finalUnion =
            new(
                population.Count +
                archive.Count);

        finalUnion.AddRange(population);
        finalUnion.AddRange(archive);

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                finalUnion,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static void AssignStrengthFitness(
        IReadOnlyList<MoCandidate> union,
        IReadOnlyList<MoCandidate> archive,
        IReadOnlyList<OptimizationSense> senses)
    {
        Dictionary<MoCandidate, double> strength = new();

        foreach (MoCandidate elite in archive)
        {
            int dominated = 0;

            foreach (MoCandidate candidate in union)
                if (!ReferenceEquals(elite, candidate) &&
                    ParetoDominance.Compare(
                        elite.Objectives,
                        candidate.Objectives,
                        senses) < 0)
                    dominated++;

            strength[elite] =
                dominated /
                (double)Math.Max(
                    union.Count,
                    1);
        }

        foreach (MoCandidate candidate in union)
        {
            double fitness = 1.0;

            foreach (MoCandidate elite in archive)
                if (ParetoDominance.Compare(
                        elite.Objectives,
                        candidate.Objectives,
                        senses) < 0)
                    fitness += strength[elite];

            candidate.Fitness = fitness;
        }

        foreach (MoCandidate elite in archive)
            elite.Fitness =
                strength.TryGetValue(
                    elite,
                    out double value)
                    ? value
                    : 0.0;
    }

    private static List<MoCandidate> EnvironmentalSelection(
        IReadOnlyList<MoCandidate> union,
        int archiveSize,
        IReadOnlyList<OptimizationSense> senses)
    {
        List<MoCandidate> nondominated =
            MultiobjectiveAdvancedToolkit.Nondominated(
                union,
                senses);

        if (nondominated.Count <= archiveSize)
            return nondominated
                .Select(MultiobjectiveToolkit.Clone)
                .ToList();

        return MultiobjectiveAdvancedToolkit.TruncateByClustering(
            nondominated,
            archiveSize,
            senses);
    }
}
