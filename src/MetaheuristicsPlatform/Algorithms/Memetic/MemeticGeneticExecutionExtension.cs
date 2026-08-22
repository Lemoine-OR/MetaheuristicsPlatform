using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>
/// Generation-level memetic layer attached to the shared generational GA engine.
/// It never reimplements selection, crossover, mutation, elitism or common lifecycle logic.
/// </summary>
internal sealed class MemeticGeneticExecutionExtension<TSolution> :
    IGeneticAlgorithmExecutionExtension<TSolution>
{
    private readonly ILocalSearchProcedure<TSolution> _localSearch;
    private readonly IMemeticLocalSearchPolicy _localSearchPolicy;
    private readonly IMemeticLearningPolicy _learningPolicy;

    private long _localSearchInvocations;
    private long _successfulLocalSearches;
    private long _acceptedLocalSearchMoves;
    private double _cumulativeLocalSearchGain;
    private int _consecutiveNonImprovingGenerations;
    private double _lastLocalSearchProbability;

    public MemeticGeneticExecutionExtension(
        ILocalSearchProcedure<TSolution> localSearch,
        IMemeticLocalSearchPolicy localSearchPolicy,
        IMemeticLearningPolicy learningPolicy)
    {
        _localSearch =
            localSearch ??
            throw new ArgumentNullException(nameof(localSearch));

        _localSearchPolicy =
            localSearchPolicy ??
            throw new ArgumentNullException(nameof(localSearchPolicy));

        _learningPolicy =
            learningPolicy ??
            throw new ArgumentNullException(nameof(learningPolicy));
    }

    public void Reset()
    {
        _localSearchInvocations = 0;
        _successfulLocalSearches = 0;
        _acceptedLocalSearchMoves = 0;
        _cumulativeLocalSearchGain = 0.0;
        _consecutiveNonImprovingGenerations = 0;
        _lastLocalSearchProbability = 0.0;
    }

    public object CreateAlgorithmState(
        in GeneticAlgorithmState state) =>
        new MemeticAlgorithmState(
            state.Generation,
            state.PopulationCount,
            state.OffspringEvaluated,
            state.ParentSelections,
            state.CrossoverEvents,
            state.MutationEvents,
            state.EliteCount,
            _localSearchInvocations,
            _successfulLocalSearches,
            _acceptedLocalSearchMoves,
            _cumulativeLocalSearchGain,
            _consecutiveNonImprovingGenerations,
            _lastLocalSearchProbability,
            _learningPolicy.Mode);

    public StoppingDecision ProcessCompletedGeneration(
        List<GeneticPopulationMember<TSolution>> nextPopulation,
        in GeneticAlgorithmState state,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nextPopulation);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solutionCloner);

        int firstOffspringIndex =
            Math.Clamp(
                state.EliteCount,
                0,
                nextPopulation.Count);

        int offspringCount =
            nextPopulation.Count -
            firstOffspringIndex;

        if (offspringCount <= 0)
        {
            return StoppingDecision.Continue(
                "NoMemeticOffspring");
        }

        int[]? ranks = null;

        if (_localSearchPolicy.RequiresRanking)
        {
            ranks =
                BuildOffspringRanks(
                    nextPopulation,
                    firstOffspringIndex,
                    context.Problem.Sense);
        }

        double bestObjective =
            BestObjective(
                nextPopulation,
                context.Problem.Sense);

        for (int index = firstOffspringIndex;
             index < nextPopulation.Count;
             index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GeneticPopulationMember<TSolution> member =
                nextPopulation[index];

            int offspringIndex =
                index -
                firstOffspringIndex;

            int offspringRank =
                ranks is null
                    ? offspringIndex
                    : ranks[index];

            var policyContext =
                new MemeticLocalSearchCandidateContext(
                    state.Generation,
                    offspringIndex,
                    offspringRank,
                    offspringCount,
                    member.Objective,
                    bestObjective,
                    _consecutiveNonImprovingGenerations,
                    _localSearchInvocations,
                    _successfulLocalSearches);

            MemeticLocalSearchDecision localSearchDecision =
                _localSearchPolicy.Decide(
                    policyContext,
                    context.Random);

            if (!double.IsFinite(
                    localSearchDecision.Probability) ||
                localSearchDecision.Probability < 0.0 ||
                localSearchDecision.Probability > 1.0)
            {
                throw new InvalidOperationException(
                    "The memetic local-search policy returned an invalid probability.");
            }

            _lastLocalSearchProbability =
                localSearchDecision.Probability;

            if (!localSearchDecision.Apply)
                continue;

            TSolution phenotype =
                solutionCloner.Clone(
                    member.Solution);

            LocalSearchProcedureResult localResult =
                _localSearch.Improve(
                    ref phenotype,
                    member.Objective,
                    context,
                    solutionCloner,
                    cancellationToken);

            _localSearchInvocations++;
            _acceptedLocalSearchMoves +=
                localResult.AcceptedMoves;

            if (context.Problem.Sense.IsBetter(
                    member.Objective,
                    localResult.Fitness))
            {
                throw new InvalidOperationException(
                    "A memetic local-search procedure returned a solution worse than its starting objective.");
            }

            bool improved =
                context.Problem.Sense.IsBetter(
                    localResult.Fitness,
                    member.Objective);

            if (improved)
            {
                _successfulLocalSearches++;

                if (double.IsFinite(member.Objective) &&
                    double.IsFinite(localResult.Fitness))
                {
                    _cumulativeLocalSearchGain +=
                        context.Problem.Sense ==
                        OptimizationSense.Minimize
                            ? member.Objective -
                              localResult.Fitness
                            : localResult.Fitness -
                              member.Objective;
                }
            }

            var learningContext =
                new MemeticLearningContext(
                    member.Objective,
                    localResult.Fitness,
                    improved);

            MemeticLearningDecision learningDecision =
                _learningPolicy.Decide(
                    learningContext);

            if (double.IsNaN(
                    learningDecision.SelectionObjective))
            {
                throw new InvalidOperationException(
                    "The memetic learning policy returned NaN as its selection objective.");
            }

            nextPopulation[index] =
                new GeneticPopulationMember<TSolution>(
                    learningDecision.InheritImprovedPhenotype
                        ? phenotype
                        : member.Solution,
                    learningDecision.SelectionObjective);

            if (localResult.StoppingDecision.ShouldStop)
            {
                return localResult.StoppingDecision;
            }

            StoppingDecision stop =
                context.EvaluateStopping(
                    CreateAlgorithmState(state));

            if (stop.ShouldStop)
                return stop;
        }

        return StoppingDecision.Continue(
            "MemeticGenerationImproved");
    }

    public void CompleteGeneration(
        bool improvedGlobalBest)
    {
        if (improvedGlobalBest)
        {
            _consecutiveNonImprovingGenerations = 0;
        }
        else
        {
            _consecutiveNonImprovingGenerations++;
        }
    }

    private static int[] BuildOffspringRanks(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        int firstOffspringIndex,
        OptimizationSense sense)
    {
        int[] indices =
            Enumerable
                .Range(
                    firstOffspringIndex,
                    population.Count -
                    firstOffspringIndex)
                .ToArray();

        Array.Sort(
            indices,
            (left, right) =>
                CompareObjectives(
                    population[left].Objective,
                    population[right].Objective,
                    sense));

        var ranks =
            new int[population.Count];

        for (int rank = 0;
             rank < indices.Length;
             rank++)
        {
            ranks[indices[rank]] =
                rank;
        }

        return ranks;
    }

    private static double BestObjective(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense)
    {
        double best =
            population[0].Objective;

        for (int index = 1;
             index < population.Count;
             index++)
        {
            if (sense.IsBetter(
                    population[index].Objective,
                    best))
            {
                best =
                    population[index].Objective;
            }
        }

        return best;
    }

    private static int CompareObjectives(
        double left,
        double right,
        OptimizationSense sense)
    {
        if (sense.IsBetter(left, right))
            return -1;

        if (sense.IsBetter(right, left))
            return 1;

        return 0;
    }
}
