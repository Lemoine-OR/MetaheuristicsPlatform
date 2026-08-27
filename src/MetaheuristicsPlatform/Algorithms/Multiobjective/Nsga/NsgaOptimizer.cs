using MetaheuristicsPlatform.Algorithms.Multiobjective.Advanced;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.Multiobjective.Nsga;

public sealed class NsgaOptimizer :
    IMultiobjectiveOptimizer<NsgaParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Nsga,
            Name = "Nondominated Sorting Genetic Algorithm",
            Acronym = "NSGA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[] { NsgaReferences.SrinivasDeb1994 }
        };

    public MultiobjectiveOptimizationResult Optimize(
        IContinuousMultiobjectiveOptimizationProblem problem,
        NsgaParameters parameters,
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

        double mutationProbability =
            parameters.MutationProbability < 0
                ? 1.0 / problem.SearchSpace.Dimension
                : parameters.MutationProbability;

        for (int generation = 0;
             generation < parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AssignSharedRankFitness(
                population,
                problem.ObjectiveSenses,
                parameters.SharingRadius,
                parameters.SharingAlpha);

            List<MoCandidate> offspring =
                new(parameters.PopulationSize);

            while (offspring.Count <
                   parameters.PopulationSize)
            {
                MoCandidate first =
                    population[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            population,
                            random)];

                MoCandidate second =
                    population[
                        MultiobjectiveAdvancedToolkit.TournamentByFitness(
                            population,
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

        return new MultiobjectiveOptimizationResult(
            MultiobjectiveToolkit.ResultFront(
                population,
                problem.ObjectiveSenses),
            evaluations,
            parameters.MaximumGenerations,
            seed);
    }

    private static void AssignSharedRankFitness(
        IReadOnlyList<MoCandidate> population,
        IReadOnlyList<OptimizationSense> senses,
        double radius,
        double alpha)
    {
        List<List<MoCandidate>> fronts =
            MultiobjectiveToolkit.SortFronts(
                population,
                senses);

        double baseFitness = population.Count;

        for (int rank = 0; rank < fronts.Count; rank++)
        {
            List<MoCandidate> front = fronts[rank];

            foreach (MoCandidate candidate in front)
            {
                double nicheCount = 0.0;

                foreach (MoCandidate other in front)
                {
                    double distance =
                        MultiobjectiveAdvancedToolkit.ObjectiveDistance(
                            candidate,
                            other,
                            population,
                            senses);

                    if (distance < radius)
                        nicheCount +=
                            1.0 -
                            Math.Pow(
                                distance / radius,
                                alpha);
                }

                double raw =
                    Math.Max(
                        1.0,
                        baseFitness - rank);

                candidate.Fitness =
                    -raw /
                    Math.Max(
                        nicheCount,
                        1e-12);
            }
        }
    }
}
