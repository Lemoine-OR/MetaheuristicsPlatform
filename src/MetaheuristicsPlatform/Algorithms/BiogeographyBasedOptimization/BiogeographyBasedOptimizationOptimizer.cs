using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.BiogeographyBasedOptimization;

public sealed class BiogeographyBasedOptimizationOptimizer :
    IMetaheuristic<double[], BiogeographyBasedOptimizationParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.BiogeographyBasedOptimization,
            Name = "Biogeography-Based Optimization",
            Acronym = "BBO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Evolutionary,
            Mechanisms = MetaheuristicMechanism.EvolutionaryOperators,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [BiogeographyBasedOptimizationReferences.Simon2008]
        };

    public BiogeographyBasedOptimizationParameters CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        BiogeographyBasedOptimizationParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);
        parameters.Validate();

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
            throw new NotSupportedException("BBO requires ISpanContinuousOptimizationProblem.");

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        if (dimension <= 0)
            throw new InvalidOperationException("BBO requires a positive dimension.");

        int n = parameters.PopulationSize;
        double[][] population = new double[n][];
        double[] objectives = new double[n];
        for (int i = 0; i < n; i++)
            population[i] = new double[dimension];

        var context = new OptimizationContext<double[]>(
            Descriptor, problem, solutionCloner, stoppingCriterion,
            options, callback, cancellationToken);

        var state = new BiogeographyBasedOptimizationState(
            0, BiogeographyBasedOptimizationPhase.Initialization,
            n, 0, 0, null);

        context.Start(state);

        for (int i = 0; i < n; i++)
        {
            searchSpace.Sample(context.Random, population[i]);
            objectives[i] = context.Evaluate(population[i], state);
            RequireFinite(objectives[i]);

            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
                return context.Complete(stop, state);
        }

        double[] speciesProbabilities =
            BuildEquilibriumSpeciesProbabilities(
                n,
                parameters.MaximumImmigrationRate,
                parameters.MaximumEmigrationRate);

        int migrationEvents = 0;
        int mutationEvents = 0;
        double[] mutationScratch = new double[dimension];

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int[] order = Rank(objectives, problem.Sense);
            int[] speciesByHabitat = new int[n];

            for (int rank = 0; rank < n; rank++)
                speciesByHabitat[order[rank]] = n - 1 - rank;

            double[][] sourcePopulation = ClonePopulation(population);
            double[] emigrationWeights = new double[n];

            for (int i = 0; i < n; i++)
            {
                int species = speciesByHabitat[i];
                emigrationWeights[i] =
                    parameters.MaximumEmigrationRate *
                    species /
                    (n - 1.0);
            }

            double pMax = speciesProbabilities.Max();

            state = new BiogeographyBasedOptimizationState(
                iteration - 1,
                BiogeographyBasedOptimizationPhase.Search,
                n,
                migrationEvents,
                mutationEvents,
                BestObjective(objectives, problem.Sense));

            for (int i = 0; i < n; i++)
            {
                bool elite = false;

                for (int rank = 0;
                     rank < parameters.EliteCount;
                     rank++)
                {
                    if (order[rank] == i)
                    {
                        elite = true;
                        break;
                    }
                }

                if (elite)
                    continue;

                int species = speciesByHabitat[i];

                double immigrationRate =
                    parameters.MaximumImmigrationRate *
                    (1.0 - species / (n - 1.0));

                for (int d = 0; d < dimension; d++)
                {
                    if (context.Random.NextDouble() < immigrationRate)
                    {
                        int source =
                            Roulette(
                                emigrationWeights,
                                context.Random,
                                i);

                        population[i][d] =
                            sourcePopulation[source][d];

                        migrationEvents++;
                    }

                    double mutationRate =
                        parameters.MaximumMutationRate *
                        (1.0 - speciesProbabilities[species] / pMax);

                    if (context.Random.NextDouble() < mutationRate)
                    {
                        searchSpace.Sample(
                            context.Random,
                            mutationScratch);

                        population[i][d] =
                            mutationScratch[d];

                        mutationEvents++;
                    }
                }

                searchSpace.Clamp(population[i].AsSpan());

                objectives[i] =
                    context.Evaluate(
                        population[i],
                        state);

                RequireFinite(objectives[i]);

                state = state with
                {
                    MigrationEvents = migrationEvents,
                    MutationEvents = mutationEvents
                };

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            state =
                new BiogeographyBasedOptimizationState(
                    iteration,
                    BiogeographyBasedOptimizationPhase.CompletedIteration,
                    n,
                    migrationEvents,
                    mutationEvents,
                    BestObjective(objectives, problem.Sense));

            context.CompleteIteration(
                state.IterationBestFitness,
                state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(state);

            if (iterationStop.ShouldStop)
                return context.Complete(iterationStop, state);
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumBboIterations",
                "The configured BBO iteration limit was reached."),
            state);
    }

    private static int[] Rank(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        int[] order =
            Enumerable.Range(0, values.Length).ToArray();

        double[] snapshot =
            values.ToArray();

        Array.Sort(
            order,
            (left, right) =>
            {
                if (snapshot[left] == snapshot[right])
                    return left.CompareTo(right);

                return sense.IsBetter(
                    snapshot[left],
                    snapshot[right])
                    ? -1
                    : 1;
            });

        return order;
    }

    private static double[][] ClonePopulation(
        double[][] population)
    {
        var clone =
            new double[population.Length][];

        for (int i = 0; i < population.Length; i++)
            clone[i] = (double[])population[i].Clone();

        return clone;
    }

    private static int Roulette(
        ReadOnlySpan<double> weights,
        IRandomSource random,
        int excludedIndex)
    {
        double total = 0.0;

        for (int i = 0; i < weights.Length; i++)
        {
            if (i != excludedIndex)
                total += weights[i];
        }

        if (!(total > 0.0) || !double.IsFinite(total))
        {
            int fallback;

            do
            {
                fallback =
                    random.NextInt32(weights.Length);
            }
            while (fallback == excludedIndex);

            return fallback;
        }

        double threshold =
            random.NextDouble() * total;

        double cumulative = 0.0;

        for (int i = 0; i < weights.Length; i++)
        {
            if (i == excludedIndex)
                continue;

            cumulative += weights[i];

            if (threshold <= cumulative)
                return i;
        }

        for (int i = weights.Length - 1; i >= 0; i--)
        {
            if (i != excludedIndex)
                return i;
        }

        throw new InvalidOperationException(
            "BBO source selection failed.");
    }

    private static double[] BuildEquilibriumSpeciesProbabilities(
        int populationSize,
        double maximumImmigration,
        double maximumEmigration)
    {
        int maximumSpecies =
            populationSize - 1;

        double[] probabilities =
            new double[populationSize];

        probabilities[0] = 1.0;

        for (int species = 1;
             species <= maximumSpecies;
             species++)
        {
            double lambdaPrevious =
                maximumImmigration *
                (1.0 - (species - 1.0) / maximumSpecies);

            double muCurrent =
                maximumEmigration *
                species /
                maximumSpecies;

            probabilities[species] =
                probabilities[species - 1] *
                lambdaPrevious /
                muCurrent;
        }

        double sum =
            probabilities.Sum();

        if (!(sum > 0.0) || !double.IsFinite(sum))
        {
            throw new InvalidOperationException(
                "BBO species-probability normalization failed.");
        }

        for (int species = 0;
             species < probabilities.Length;
             species++)
        {
            probabilities[species] /= sum;
        }

        return probabilities;
    }

    private static double BestObjective(
        ReadOnlySpan<double> values,
        OptimizationSense sense)
    {
        double best = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (sense.IsBetter(values[i], best))
                best = values[i];
        }

        return best;
    }

    private static void RequireFinite(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "BBO requires finite objective values.");
        }
    }
}
