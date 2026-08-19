using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>
/// Result of the generational evolutionary path-relinking intensification phase.
/// </summary>
public readonly record struct EvolutionaryPathRelinkingResult<TSolution>(
    TSolution BestSolution,
    double BestFitness,
    int GenerationsCompleted,
    long PairRelinkings,
    long PathSteps,
    long CandidateEvaluations,
    long AcceptedLocalMoves,
    long ElitePoolUpdates,
    StoppingDecision StoppingDecision);

/// <summary>
/// Generational evolutionary path relinking over a bounded quality/diversity elite population.
/// </summary>
/// <remarks>
/// Generation zero is the input elite set. At generation k, every unordered pair is
/// relinked and the resulting offspring compete for membership in a fresh generation.
/// Evolution stops when the new generation best does not strictly improve the previous
/// generation best, or when the configured safety cap/global stopping criterion is reached.
/// </remarks>
public sealed class EvolutionaryPathRelinkingProcedure<TSolution>
{
    private readonly IPathRelinkingProcedure<TSolution> _pathRelinking;
    private readonly ILocalSearchProcedure<TSolution> _localSearch;

    /// <summary>Creates a generational evolutionary path-relinking procedure.</summary>
    public EvolutionaryPathRelinkingProcedure(
        IPathRelinkingProcedure<TSolution> pathRelinking,
        ILocalSearchProcedure<TSolution> localSearch)
    {
        _pathRelinking =
            pathRelinking ?? throw new ArgumentNullException(nameof(pathRelinking));
        _localSearch =
            localSearch ?? throw new ArgumentNullException(nameof(localSearch));
    }

    /// <summary>
    /// Evolves the supplied elite population through exhaustive unordered-pair relinking.
    /// </summary>
    public EvolutionaryPathRelinkingResult<TSolution> Evolve(
        EliteSolutionPool<TSolution> initialPopulation,
        PathRelinkingExecutionOptions pathRelinkingOptions,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        int maximumGenerations,
        int maximumPathSteps,
        bool improveOffspring,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(initialPopulation);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        if (maximumGenerations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumGenerations));
        }

        if (maximumPathSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPathSteps));
        }

        pathRelinkingOptions.Validate();

        if (!pathRelinkingOptions.IsCanonicalGreedyForward &&
            _pathRelinking is not IAdvancedPathRelinkingProcedure<TSolution>)
        {
            throw new InvalidOperationException(
                "Advanced evolutionary path-relinking options require an " +
                "IAdvancedPathRelinkingProcedure<TSolution> implementation.");
        }

        if (!initialPopulation.TryGetBest(
                out TSolution bestSolution,
                out double bestFitness))
        {
            throw new InvalidOperationException(
                "Evolutionary path relinking requires a non-empty elite population.");
        }

        bestSolution =
            solutionCloner.Clone(bestSolution);

        if (initialPopulation.Count < 2)
        {
            return new EvolutionaryPathRelinkingResult<TSolution>(
                bestSolution,
                bestFitness,
                GenerationsCompleted: 0,
                PairRelinkings: 0,
                PathSteps: 0,
                CandidateEvaluations: 0,
                AcceptedLocalMoves: 0,
                ElitePoolUpdates: 0,
                StoppingDecision.Continue(
                    "EvolutionaryPathRelinkingInsufficientPopulation"));
        }

        EliteSolutionPool<TSolution> population =
            initialPopulation;

        int generationsCompleted = 0;
        long pairRelinkings = 0;
        long pathSteps = 0;
        long candidateEvaluations = 0;
        long acceptedLocalMoves = 0;
        long elitePoolUpdates = 0;

        for (int generation = 1;
             generation <= maximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!population.TryGetBest(
                    out _,
                    out double previousGenerationBest))
            {
                throw new InvalidOperationException(
                    "Evolutionary path-relinking population unexpectedly became empty.");
            }

            int populationCount =
                population.Count;

            if (populationCount < 2)
            {
                return CreateResult(
                    bestSolution,
                    bestFitness,
                    generationsCompleted,
                    pairRelinkings,
                    pathSteps,
                    candidateEvaluations,
                    acceptedLocalMoves,
                    elitePoolUpdates,
                    "EvolutionaryPathRelinkingPopulationCollapsed");
            }

            EliteSolutionPool<TSolution> nextPopulation =
                population.CreateEmptySibling();

            for (int firstIndex = 0;
                 firstIndex < populationCount - 1;
                 firstIndex++)
            {
                population.GetAt(
                    firstIndex,
                    out TSolution first,
                    out double firstFitness);

                for (int secondIndex = firstIndex + 1;
                     secondIndex < populationCount;
                     secondIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    population.GetAt(
                        secondIndex,
                        out TSolution second,
                        out double secondFitness);

                    pairRelinkings++;

                    PathRelinkingProcedureResult<TSolution> relinkingResult;

                    if (_pathRelinking is
                        IAdvancedPathRelinkingProcedure<TSolution> advancedPathRelinking)
                    {
                        relinkingResult =
                            advancedPathRelinking.RelinkAdvanced(
                                in first,
                                firstFitness,
                                in second,
                                secondFitness,
                                pathRelinkingOptions,
                                context,
                                solutionCloner,
                                maximumPathSteps,
                                cancellationToken);
                    }
                    else
                    {
                        relinkingResult =
                            _pathRelinking.Relink(
                                in first,
                                firstFitness,
                                in second,
                                context,
                                solutionCloner,
                                maximumPathSteps,
                                cancellationToken);
                    }

                    pathSteps +=
                        relinkingResult.PathSteps;
                    candidateEvaluations +=
                        relinkingResult.CandidateEvaluations;

                    if (relinkingResult.StoppingDecision.ShouldStop)
                    {
                        return new EvolutionaryPathRelinkingResult<TSolution>(
                            bestSolution,
                            bestFitness,
                            generationsCompleted,
                            pairRelinkings,
                            pathSteps,
                            candidateEvaluations,
                            acceptedLocalMoves,
                            elitePoolUpdates,
                            relinkingResult.StoppingDecision);
                    }

                    TSolution offspring =
                        relinkingResult.BestSolution;
                    double offspringFitness =
                        relinkingResult.BestFitness;

                    if (improveOffspring)
                    {
                        LocalSearchProcedureResult localResult =
                            _localSearch.Improve(
                                ref offspring,
                                offspringFitness,
                                context,
                                solutionCloner,
                                cancellationToken);

                        offspringFitness =
                            localResult.Fitness;
                        acceptedLocalMoves +=
                            localResult.AcceptedMoves;

                        if (localResult.StoppingDecision.ShouldStop)
                        {
                            return new EvolutionaryPathRelinkingResult<TSolution>(
                                bestSolution,
                                bestFitness,
                                generationsCompleted,
                                pairRelinkings,
                                pathSteps,
                                candidateEvaluations,
                                acceptedLocalMoves,
                                elitePoolUpdates,
                                localResult.StoppingDecision);
                        }
                    }

                    if (context.Problem.Sense.IsBetter(
                            offspringFitness,
                            bestFitness))
                    {
                        bestSolution =
                            solutionCloner.Clone(offspring);
                        bestFitness =
                            offspringFitness;
                    }

                    if (nextPopulation.TryAddEvolutionary(
                            in offspring,
                            offspringFitness,
                            out _))
                    {
                        elitePoolUpdates++;
                    }

                    StoppingDecision stop =
                        context.EvaluateStopping();

                    if (stop.ShouldStop)
                    {
                        return new EvolutionaryPathRelinkingResult<TSolution>(
                            bestSolution,
                            bestFitness,
                            generationsCompleted,
                            pairRelinkings,
                            pathSteps,
                            candidateEvaluations,
                            acceptedLocalMoves,
                            elitePoolUpdates,
                            stop);
                    }
                }
            }

            if (!nextPopulation.TryGetBest(
                    out _,
                    out double nextGenerationBest))
            {
                return CreateResult(
                    bestSolution,
                    bestFitness,
                    generationsCompleted,
                    pairRelinkings,
                    pathSteps,
                    candidateEvaluations,
                    acceptedLocalMoves,
                    elitePoolUpdates,
                    "EvolutionaryPathRelinkingNoAdmissibleOffspring");
            }

            generationsCompleted++;

            if (!context.Problem.Sense.IsBetter(
                    nextGenerationBest,
                    previousGenerationBest))
            {
                return CreateResult(
                    bestSolution,
                    bestFitness,
                    generationsCompleted,
                    pairRelinkings,
                    pathSteps,
                    candidateEvaluations,
                    acceptedLocalMoves,
                    elitePoolUpdates,
                    "EvolutionaryPathRelinkingConverged");
            }

            population =
                nextPopulation;
        }

        return CreateResult(
            bestSolution,
            bestFitness,
            generationsCompleted,
            pairRelinkings,
            pathSteps,
            candidateEvaluations,
            acceptedLocalMoves,
            elitePoolUpdates,
            "MaximumEvolutionaryPathRelinkingGenerations");
    }

    private static EvolutionaryPathRelinkingResult<TSolution> CreateResult(
        TSolution bestSolution,
        double bestFitness,
        int generationsCompleted,
        long pairRelinkings,
        long pathSteps,
        long candidateEvaluations,
        long acceptedLocalMoves,
        long elitePoolUpdates,
        string criterion) =>
        new(
            bestSolution,
            bestFitness,
            generationsCompleted,
            pairRelinkings,
            pathSteps,
            candidateEvaluations,
            acceptedLocalMoves,
            elitePoolUpdates,
            StoppingDecision.Continue(criterion));
}