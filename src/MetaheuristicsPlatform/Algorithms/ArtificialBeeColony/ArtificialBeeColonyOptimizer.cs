using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.ArtificialBeeColony;

/// <summary>
/// Canonical continuous Artificial Bee Colony algorithm of Karaboga and Basturk.
/// </summary>
public sealed class ArtificialBeeColonyOptimizer :
    IMetaheuristic<double[], ArtificialBeeColonyParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.ArtificialBeeColony,
            Name = "Artificial Bee Colony",
            Acronym = "ABC",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
            [
                ArtificialBeeColonyReferences.KarabogaBasturk2007,
                ArtificialBeeColonyReferences.KarabogaBasturk2008
            ]
        };

    public ArtificialBeeColonyParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        ArtificialBeeColonyParameters parameters,
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
                "Artificial Bee Colony requires ISpanContinuousOptimizationProblem.");
        }

        int dimension =
            continuousProblem.SearchSpace.Dimension;

        if (dimension <= 0)
        {
            throw new InvalidOperationException(
                "Artificial Bee Colony requires a positive search-space dimension.");
        }

        int sourceCount =
            parameters.FoodSourceCount;

        int abandonmentLimit =
            ResolveAbandonmentLimit(
                parameters,
                sourceCount,
                dimension);

        double[][] sources =
            new double[sourceCount][];

        double[] objectiveValues =
            new double[sourceCount];

        int[] trials =
            new int[sourceCount];

        double[] selectionWeights =
            new double[sourceCount];

        double[] candidate =
            new double[dimension];

        for (int i = 0; i < sourceCount; i++)
        {
            sources[i] =
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

        ArtificialBeeColonyState state =
            new(
                Cycle: 0,
                Phase: ArtificialBeeColonyPhase.Initialization,
                FoodSourceCount: sourceCount,
                AbandonmentLimit: abandonmentLimit,
                ScoutReinitializations: 0,
                CycleBestFitness: null);

        context.Start(state);

        for (int i = 0; i < sourceCount; i++)
        {
            continuousProblem.SearchSpace.Sample(
                context.Random,
                sources[i]);

            objectiveValues[i] =
                context.Evaluate(
                    sources[i],
                    state);

            RequireFinite(
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

        int scoutReinitializations = 0;

        for (int cycle = 1;
             cycle <= parameters.MaximumCycles;
             cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double? cycleBest = null;

            state =
                new ArtificialBeeColonyState(
                    Cycle: cycle - 1,
                    Phase: ArtificialBeeColonyPhase.EmployedBees,
                    FoodSourceCount: sourceCount,
                    AbandonmentLimit: abandonmentLimit,
                    ScoutReinitializations: scoutReinitializations,
                    CycleBestFitness: cycleBest);

            for (int source = 0;
                 source < sourceCount;
                 source++)
            {
                BuildNeighborCandidate(
                    sources,
                    source,
                    candidate,
                    continuousProblem.SearchSpace,
                    context.Random);

                double candidateObjective =
                    context.Evaluate(
                        candidate,
                        state);

                RequireFinite(
                    candidateObjective);

                cycleBest =
                    UpdateBest(
                        problem.Sense,
                        cycleBest,
                        candidateObjective);

                GreedySelection(
                    problem.Sense,
                    sources[source],
                    ref objectiveValues[source],
                    ref trials[source],
                    candidate,
                    candidateObjective);

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

            BuildSelectionWeights(
                objectiveValues,
                problem.Sense,
                selectionWeights);

            state =
                state with
                {
                    Phase =
                        ArtificialBeeColonyPhase.OnlookerBees,
                    CycleBestFitness =
                        cycleBest
                };

            for (int onlooker = 0;
                 onlooker < sourceCount;
                 onlooker++)
            {
                int source =
                    SelectSource(
                        selectionWeights,
                        context.Random);

                BuildNeighborCandidate(
                    sources,
                    source,
                    candidate,
                    continuousProblem.SearchSpace,
                    context.Random);

                double candidateObjective =
                    context.Evaluate(
                        candidate,
                        state);

                RequireFinite(
                    candidateObjective);

                cycleBest =
                    UpdateBest(
                        problem.Sense,
                        cycleBest,
                        candidateObjective);

                GreedySelection(
                    problem.Sense,
                    sources[source],
                    ref objectiveValues[source],
                    ref trials[source],
                    candidate,
                    candidateObjective);

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

            int scoutIndex =
                FindAbandonedSource(
                    trials,
                    abandonmentLimit);

            if (scoutIndex >= 0)
            {
                state =
                    state with
                    {
                        Phase =
                            ArtificialBeeColonyPhase.Scout,
                        CycleBestFitness =
                            cycleBest
                    };

                continuousProblem.SearchSpace.Sample(
                    context.Random,
                    sources[scoutIndex]);

                objectiveValues[scoutIndex] =
                    context.Evaluate(
                        sources[scoutIndex],
                        state);

                RequireFinite(
                    objectiveValues[scoutIndex]);

                trials[scoutIndex] = 0;
                scoutReinitializations++;

                cycleBest =
                    UpdateBest(
                        problem.Sense,
                        cycleBest,
                        objectiveValues[scoutIndex]);

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

            state =
                new ArtificialBeeColonyState(
                    Cycle: cycle,
                    Phase: ArtificialBeeColonyPhase.CompletedCycle,
                    FoodSourceCount: sourceCount,
                    AbandonmentLimit: abandonmentLimit,
                    ScoutReinitializations: scoutReinitializations,
                    CycleBestFitness: cycleBest);

            context.CompleteIteration(
                cycleBest,
                state);

            StoppingDecision cycleStop =
                context.EvaluateStopping(
                    state);

            if (cycleStop.ShouldStop)
            {
                return context.Complete(
                    cycleStop,
                    state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumArtificialBeeColonyCycles",
                "The configured Artificial Bee Colony cycle limit was reached."),
            state);
    }

    private static int ResolveAbandonmentLimit(
        ArtificialBeeColonyParameters parameters,
        int sourceCount,
        int dimension)
    {
        if (parameters.AbandonmentLimit > 0)
        {
            return parameters.AbandonmentLimit;
        }

        long derived =
            (long)sourceCount *
            dimension;

        if (derived > int.MaxValue)
        {
            throw new InvalidOperationException(
                "Derived ABC abandonment limit exceeds Int32 capacity.");
        }

        return checked((int)derived);
    }

    private static void BuildNeighborCandidate(
        double[][] sources,
        int sourceIndex,
        double[] candidate,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random)
    {
        int sourceCount =
            sources.Length;

        int dimension =
            searchSpace.Dimension;

        Array.Copy(
            sources[sourceIndex],
            candidate,
            dimension);

        int partner =
            random.NextInt32(
                sourceCount - 1);

        if (partner >= sourceIndex)
        {
            partner++;
        }

        int coordinate =
            random.NextInt32(
                dimension);

        double phi =
            (2.0 *
             random.NextDouble()) -
            1.0;

        candidate[coordinate] =
            sources[sourceIndex][coordinate] +
            (phi *
             (sources[sourceIndex][coordinate] -
              sources[partner][coordinate]));

        searchSpace.Clamp(
            candidate.AsSpan());
    }

    private static void GreedySelection(
        OptimizationSense sense,
        double[] source,
        ref double sourceObjective,
        ref int trialCount,
        double[] candidate,
        double candidateObjective)
    {
        if (sense.IsBetter(
            candidateObjective,
            sourceObjective))
        {
            Array.Copy(
                candidate,
                source,
                source.Length);

            sourceObjective =
                candidateObjective;

            trialCount = 0;
        }
        else
        {
            if (trialCount < int.MaxValue)
            {
                trialCount++;
            }
        }
    }

    private static void BuildSelectionWeights(
        ReadOnlySpan<double> objectiveValues,
        OptimizationSense sense,
        Span<double> weights)
    {
        double maxFitness = 0.0;

        for (int i = 0;
             i < objectiveValues.Length;
             i++)
        {
            double fitness =
                CanonicalFitness(
                    objectiveValues[i],
                    sense);

            weights[i] =
                fitness;

            maxFitness =
                Math.Max(
                    maxFitness,
                    fitness);
        }

        if (!double.IsFinite(maxFitness) ||
            maxFitness <= 0.0)
        {
            weights.Fill(1.0);
            return;
        }

        double scaledSum = 0.0;

        for (int i = 0;
             i < weights.Length;
             i++)
        {
            weights[i] /=
                maxFitness;

            scaledSum +=
                weights[i];
        }

        if (!double.IsFinite(scaledSum) ||
            scaledSum <= 0.0)
        {
            weights.Fill(1.0);
        }
    }

    private static double CanonicalFitness(
        double objective,
        OptimizationSense sense)
    {
        double cost =
            sense == OptimizationSense.Minimize
                ? objective
                : -objective;

        if (cost >= 0.0)
        {
            return
                1.0 /
                (1.0 + cost);
        }

        return
            1.0 +
            Math.Abs(cost);
    }

    private static int SelectSource(
        ReadOnlySpan<double> weights,
        IRandomSource random)
    {
        double sum = 0.0;

        for (int i = 0;
             i < weights.Length;
             i++)
        {
            sum +=
                weights[i];
        }

        if (!double.IsFinite(sum) ||
            sum <= 0.0)
        {
            return random.NextInt32(
                weights.Length);
        }

        double threshold =
            random.NextDouble() *
            sum;

        double cumulative = 0.0;

        for (int i = 0;
             i < weights.Length;
             i++)
        {
            cumulative +=
                weights[i];

            if (threshold < cumulative)
            {
                return i;
            }
        }

        return weights.Length - 1;
    }

    private static int FindAbandonedSource(
        ReadOnlySpan<int> trials,
        int limit)
    {
        int bestIndex = -1;
        int bestTrials = limit - 1;

        for (int i = 0;
             i < trials.Length;
             i++)
        {
            if (trials[i] > bestTrials)
            {
                bestTrials =
                    trials[i];

                bestIndex = i;
            }
        }

        return bestIndex;
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

    private static void RequireFinite(
        double objective)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException(
                "Artificial Bee Colony requires finite objective values.");
        }
    }
}
