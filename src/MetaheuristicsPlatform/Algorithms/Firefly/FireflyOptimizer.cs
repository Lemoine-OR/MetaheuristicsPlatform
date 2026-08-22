using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Firefly;

/// <summary>
/// Canonical bounded-continuous Firefly Algorithm of Yang with distance-decaying
/// attractiveness and additive uniform randomization.
/// </summary>
public sealed class FireflyOptimizer :
    IMetaheuristic<double[], FireflyParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.Firefly,
            Name = "Firefly Algorithm",
            Acronym = "FA",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                FireflyReferences.Yang2009,
                FireflyReferences.Yang2010
            ]
        };

    public FireflyParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        FireflyParameters parameters,
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
        {
            throw new NotSupportedException(
                "Firefly Algorithm requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace =
            continuousProblem.SearchSpace;

        int dimension =
            searchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Firefly Algorithm requires a positive search-space dimension.");
        }

        int populationSize =
            parameters.PopulationSize;

        double[][] fireflies =
            new double[populationSize][];

        double[] objectiveValues =
            new double[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            fireflies[i] =
                new double[dimension];
        }

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        FireflyState state =
            new(
                Iteration: 0,
                Phase: FireflyPhase.Initialization,
                PopulationSize: populationSize,
                TotalMoves: 0,
                IterationMoves: 0,
                BaseAttractiveness: parameters.BaseAttractiveness,
                LightAbsorptionCoefficient: parameters.LightAbsorptionCoefficient,
                RandomizationAmplitude: parameters.RandomizationAmplitude,
                IterationBestFitness: null);

        context.Start(state);

        double? initializationBest =
            null;

        for (int i = 0; i < populationSize; i++)
        {
            searchSpace.Sample(
                context.Random,
                fireflies[i]);

            objectiveValues[i] =
                context.Evaluate(
                    fireflies[i],
                    state);

            RequireFiniteObjective(
                objectiveValues[i]);

            initializationBest =
                UpdateBest(
                    problem.Sense,
                    initializationBest,
                    objectiveValues[i]);

            StoppingDecision stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    state);
            }
        }

        int totalMoves = 0;

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int iterationMoves = 0;

            double? iterationBest =
                FindBestObjective(
                    objectiveValues,
                    problem.Sense);

            state =
                new FireflyState(
                    Iteration: iteration - 1,
                    Phase: FireflyPhase.AttractionMoves,
                    PopulationSize: populationSize,
                    TotalMoves: totalMoves,
                    IterationMoves: iterationMoves,
                    BaseAttractiveness: parameters.BaseAttractiveness,
                    LightAbsorptionCoefficient: parameters.LightAbsorptionCoefficient,
                    RandomizationAmplitude: parameters.RandomizationAmplitude,
                    IterationBestFitness: iterationBest);

            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < populationSize; j++)
                {
                    if (i == j ||
                        !problem.Sense.IsBetter(
                            objectiveValues[j],
                            objectiveValues[i]))
                    {
                        continue;
                    }

                    MoveTowardBrighterFirefly(
                        fireflies[i],
                        fireflies[j],
                        parameters,
                        searchSpace,
                        context.Random);

                    double movedObjective =
                        context.Evaluate(
                            fireflies[i],
                            state);

                    RequireFiniteObjective(
                        movedObjective);

                    objectiveValues[i] =
                        movedObjective;

                    totalMoves++;
                    iterationMoves++;

                    iterationBest =
                        UpdateBest(
                            problem.Sense,
                            iterationBest,
                            movedObjective);

                    state =
                        state with
                        {
                            TotalMoves = totalMoves,
                            IterationMoves = iterationMoves,
                            IterationBestFitness = iterationBest
                        };

                    StoppingDecision stop =
                        context.EvaluateStopping(
                            state);

                    if (stop.ShouldStop)
                    {
                        // A partial pairwise sweep is observable but is not a
                        // completed Firefly iteration.
                        return context.Complete(
                            stop,
                            state);
                    }
                }
            }

            state =
                new FireflyState(
                    Iteration: iteration,
                    Phase: FireflyPhase.CompletedIteration,
                    PopulationSize: populationSize,
                    TotalMoves: totalMoves,
                    IterationMoves: iterationMoves,
                    BaseAttractiveness: parameters.BaseAttractiveness,
                    LightAbsorptionCoefficient: parameters.LightAbsorptionCoefficient,
                    RandomizationAmplitude: parameters.RandomizationAmplitude,
                    IterationBestFitness: iterationBest);

            context.CompleteIteration(
                iterationBest,
                state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(
                    state);

            if (iterationStop.ShouldStop)
            {
                return context.Complete(
                    iterationStop,
                    state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumFireflyIterations",
                "The configured Firefly Algorithm iteration limit was reached."),
            state);
    }

    private static void MoveTowardBrighterFirefly(
        double[] current,
        ReadOnlySpan<double> brighter,
        FireflyParameters parameters,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random)
    {
        double distanceSquared =
            SquaredDistance(
                current,
                brighter);

        if (!double.IsFinite(distanceSquared))
        {
            throw new InvalidOperationException(
                "Firefly Algorithm requires finite pairwise squared distances.");
        }

        double exponent =
            parameters.LightAbsorptionCoefficient == 0.0
                ? 0.0
                : -parameters.LightAbsorptionCoefficient * distanceSquared;

        double attractiveness =
            parameters.BaseAttractiveness *
            Math.Exp(exponent);

        if (!double.IsFinite(attractiveness))
        {
            throw new InvalidOperationException(
                "Firefly Algorithm produced a non-finite attractiveness value.");
        }

        for (int coordinate = 0;
             coordinate < current.Length;
             coordinate++)
        {
            double delta =
                brighter[coordinate] -
                current[coordinate];

            double randomization =
                parameters.RandomizationAmplitude *
                (random.NextDouble() - 0.5);

            double updated =
                current[coordinate] +
                (attractiveness * delta) +
                randomization;

            if (!double.IsFinite(updated))
            {
                throw new InvalidOperationException(
                    "Firefly Algorithm produced a non-finite moved coordinate.");
            }

            current[coordinate] =
                updated;
        }

        searchSpace.Clamp(
            current.AsSpan());
    }

    private static double SquaredDistance(
        ReadOnlySpan<double> left,
        ReadOnlySpan<double> right)
    {
        double sum = 0.0;

        for (int i = 0; i < left.Length; i++)
        {
            double delta =
                left[i] -
                right[i];

            sum +=
                delta *
                delta;
        }

        return sum;
    }

    private static double? FindBestObjective(
        ReadOnlySpan<double> objectiveValues,
        OptimizationSense sense)
    {
        double? best = null;

        for (int i = 0; i < objectiveValues.Length; i++)
        {
            best =
                UpdateBest(
                    sense,
                    best,
                    objectiveValues[i]);
        }

        return best;
    }

    private static double? UpdateBest(
        OptimizationSense sense,
        double? current,
        double candidate)
    {
        if (!current.HasValue ||
            sense.IsBetter(
                candidate,
                current.Value))
        {
            return candidate;
        }

        return current;
    }

    private static void RequireFiniteObjective(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Firefly Algorithm requires finite objective values.");
        }
    }
}
