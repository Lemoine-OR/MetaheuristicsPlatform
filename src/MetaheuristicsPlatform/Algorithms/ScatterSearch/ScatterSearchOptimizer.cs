using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.ScatterSearch;

/// <summary>
/// Generic Scatter Search foundation implementing the five-method template:
/// diversification generation, improvement, reference-set update,
/// subset generation and solution combination.
/// </summary>
public sealed class ScatterSearchOptimizer<TSolution> :
    IMetaheuristic<TSolution,ScatterSearchParameters>
{
    private readonly IScatterSearchDiversificationGenerationMethod<TSolution> _diversification;
    private readonly IScatterSearchImprovementMethod<TSolution>? _improvement;
    private readonly IScatterSearchReferenceSetUpdateMethod<TSolution> _referenceSetUpdate;
    private readonly IScatterSearchSubsetGenerationMethod<TSolution> _subsetGeneration;
    private readonly IScatterSearchSolutionCombinationMethod<TSolution> _combination;
    private readonly IScatterSearchDistance<TSolution> _distance;
    private readonly IScatterSearchReferenceSetRebuildingMethod<TSolution>? _referenceSetRebuilding;

    public ScatterSearchOptimizer(
        IScatterSearchDiversificationGenerationMethod<TSolution> diversification,
        IScatterSearchSolutionCombinationMethod<TSolution> combination,
        IScatterSearchDistance<TSolution> distance,
        IScatterSearchImprovementMethod<TSolution>? improvement = null)
        : this(
            diversification,
            improvement,
            new ClassicalScatterSearchReferenceSetUpdateMethod<TSolution>(),
            new PairwiseNewScatterSearchSubsetGenerationMethod<TSolution>(),
            combination,
            distance)
    {
    }

    public ScatterSearchOptimizer(
        IScatterSearchDiversificationGenerationMethod<TSolution> diversification,
        IScatterSearchImprovementMethod<TSolution>? improvement,
        IScatterSearchReferenceSetUpdateMethod<TSolution> referenceSetUpdate,
        IScatterSearchSubsetGenerationMethod<TSolution> subsetGeneration,
        IScatterSearchSolutionCombinationMethod<TSolution> combination,
        IScatterSearchDistance<TSolution> distance)
        : this(
            diversification,
            improvement,
            referenceSetUpdate,
            subsetGeneration,
            combination,
            distance,
            referenceSetRebuilding: null)
    {
    }

    public ScatterSearchOptimizer(
        IScatterSearchDiversificationGenerationMethod<TSolution> diversification,
        IScatterSearchImprovementMethod<TSolution>? improvement,
        IScatterSearchReferenceSetUpdateMethod<TSolution> referenceSetUpdate,
        IScatterSearchSubsetGenerationMethod<TSolution> subsetGeneration,
        IScatterSearchSolutionCombinationMethod<TSolution> combination,
        IScatterSearchDistance<TSolution> distance,
        IScatterSearchReferenceSetRebuildingMethod<TSolution>? referenceSetRebuilding)
    {
        _diversification =
            diversification ??
            throw new ArgumentNullException(nameof(diversification));

        _improvement =
            improvement;

        _referenceSetUpdate =
            referenceSetUpdate ??
            throw new ArgumentNullException(nameof(referenceSetUpdate));

        _subsetGeneration =
            subsetGeneration ??
            throw new ArgumentNullException(nameof(subsetGeneration));

        _combination =
            combination ??
            throw new ArgumentNullException(nameof(combination));

        _distance =
            distance ??
            throw new ArgumentNullException(nameof(distance));

        _referenceSetRebuilding =
            referenceSetRebuilding;
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = "scatter-search-marti-laguna-glover-2006",
        Name = "Scatter Search",
        Acronym = "SS",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary,
        Mechanisms =
            MetaheuristicMechanism.EvolutionaryOperators |
            MetaheuristicMechanism.MemoryBased |
            MetaheuristicMechanism.Constructive,
        SearchSpaces =
            SearchSpaceKind.Continuous |
            SearchSpaceKind.Binary |
            SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial |
            SearchSpaceKind.Mixed,
        IsStochastic = false,
        References =
        [
            ScatterSearchReferences.MartiLagunaGlover2006,
            ScatterSearchReferences.LagunaMarti2003,
            ScatterSearchReferences.GloverLagunaMarti2004
        ]
    };

    public ScatterSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        ScatterSearchParameters parameters,
        ISolutionCloner<TSolution> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<TSolution>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);

        parameters.Validate();

        var context =
            new OptimizationContext<TSolution>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        int diversificationEvaluated = 0;
        long subsetsGenerated = 0;
        long combinedEvaluated = 0;
        long referenceSetUpdates = 0;
        long improvementInvocations = 0;
        int referenceSetRebuilds = 0;

        var state =
            new ScatterSearchState(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0);

        context.Start(state);

        var population =
            new List<ScatterSearchReferencePoint<TSolution>>(
                parameters.DiversificationPopulationSize);

        for (int i = 0; i < parameters.DiversificationPopulationSize; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TSolution solution =
                _diversification.Generate(
                    problem,
                    context.Random);

            if (_improvement is not null)
            {
                _improvement.Improve(
                    ref solution,
                    problem,
                    context.Random,
                    cancellationToken);

                improvementInvocations++;
            }

            double objective =
                context.Evaluate(
                    solution,
                    state);

            diversificationEvaluated++;

            population.Add(
                new ScatterSearchReferencePoint<TSolution>(
                    solutionCloner.Clone(solution),
                    objective,
                    isNew: true));

            state =
                new ScatterSearchState(
                    0,
                    diversificationEvaluated,
                    0,
                    0,
                    subsetsGenerated,
                    combinedEvaluated,
                    referenceSetUpdates,
                    improvementInvocations);

            StoppingDecision initialStop =
                context.EvaluateStopping(state);

            if (initialStop.ShouldStop)
                return context.Complete(initialStop, state);
        }

        var referenceSet =
            new List<ScatterSearchReferencePoint<TSolution>>(
                parameters.ReferenceSetSize);

        _referenceSetUpdate.Initialize(
            referenceSet,
            population,
            parameters.ReferenceSetSize,
            parameters.QualityReferenceSetSize,
            _distance,
            problem.Sense,
            solutionCloner);

        state =
            CreateState(
                0,
                diversificationEvaluated,
                referenceSet,
                subsetsGenerated,
                combinedEvaluated,
                referenceSetUpdates,
                improvementInvocations);

        StoppingDecision stop =
            context.EvaluateStopping(state);

        if (stop.ShouldStop)
            return context.Complete(stop, state);

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<ScatterSearchSubset<TSolution>> subsets =
                _subsetGeneration.Generate(referenceSet);

            subsetsGenerated +=
                subsets.Count;

            foreach (ScatterSearchReferencePoint<TSolution> point in referenceSet)
                point.IsNew = false;

            if (subsets.Count == 0)
            {
                state =
                    CreateState(
                        iteration - 1,
                        diversificationEvaluated,
                        referenceSet,
                        subsetsGenerated,
                        combinedEvaluated,
                        referenceSetUpdates,
                        improvementInvocations);

                return context.Complete(
                    StoppingDecision.Stop(
                        "ReferenceSetStable",
                        "No new reference subset remains to be combined."),
                    state);
            }

            long updatesBeforeIteration =
                referenceSetUpdates;

            bool refreshImmediately = false;

            foreach (ScatterSearchSubset<TSolution> subset in subsets)
            {
                IEnumerable<TSolution> combined =
                    _combination.Combine(
                        subset,
                        problem,
                        context.Random) ??
                    throw new InvalidOperationException(
                        "The Scatter Search combination method returned null.");

                foreach (TSolution rawCandidate in combined)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // A combination method may return a RefSet-owned object.
                    // Clone before optional improvement so domain code cannot mutate
                    // a reference solution through aliasing.
                    TSolution candidate =
                        solutionCloner.Clone(rawCandidate);

                    if (_improvement is not null)
                    {
                        _improvement.Improve(
                            ref candidate,
                            problem,
                            context.Random,
                            cancellationToken);

                        improvementInvocations++;
                    }

                    state =
                        CreateState(
                            iteration - 1,
                            diversificationEvaluated,
                            referenceSet,
                            subsetsGenerated,
                            combinedEvaluated,
                            referenceSetUpdates,
                            improvementInvocations);

                    double objective =
                        context.Evaluate(
                            candidate,
                            state);

                    combinedEvaluated++;

                    var candidatePoint =
                        new ScatterSearchReferencePoint<TSolution>(
                            candidate,
                            objective,
                            isNew: true);

                    if (_referenceSetUpdate.TryUpdate(
                            referenceSet,
                            candidatePoint,
                            _distance,
                            problem.Sense,
                            solutionCloner))
                    {
                        referenceSetUpdates++;

                        if (parameters.ReferenceSetRefreshMode ==
                            ScatterSearchReferenceSetRefreshMode.DynamicImmediate)
                        {
                            refreshImmediately = true;
                        }
                    }

                    state =
                        CreateState(
                            iteration - 1,
                            diversificationEvaluated,
                            referenceSet,
                            subsetsGenerated,
                            combinedEvaluated,
                            referenceSetUpdates,
                            improvementInvocations);

                    stop =
                        context.EvaluateStopping(state);

                    if (stop.ShouldStop)
                        return context.Complete(stop, state);

                    if (refreshImmediately)
                        break;
                }

                if (refreshImmediately)
                    break;
            }

            state =
                CreateState(
                    iteration,
                    diversificationEvaluated,
                    referenceSet,
                    subsetsGenerated,
                    combinedEvaluated,
                    referenceSetUpdates,
                    improvementInvocations);

            context.CompleteIteration(
                context.State.BestFitness,
                state);

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);

            if (refreshImmediately)
                continue;

            if (referenceSetUpdates == updatesBeforeIteration)
            {
                if (_referenceSetRebuilding is not null &&
                    referenceSetRebuilds < parameters.MaximumReferenceSetRebuilds)
                {
                    var rebuildPopulation =
                        new List<ScatterSearchReferencePoint<TSolution>>(
                            parameters.RebuildDiversificationPopulationSize);

                    for (int rebuildIndex = 0;
                         rebuildIndex < parameters.RebuildDiversificationPopulationSize;
                         rebuildIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        TSolution rebuildSolution =
                            _diversification.Generate(
                                problem,
                                context.Random);

                        if (_improvement is not null)
                        {
                            _improvement.Improve(
                                ref rebuildSolution,
                                problem,
                                context.Random,
                                cancellationToken);

                            improvementInvocations++;
                        }

                        double rebuildObjective =
                            context.Evaluate(
                                rebuildSolution,
                                state);

                        diversificationEvaluated++;

                        rebuildPopulation.Add(
                            new ScatterSearchReferencePoint<TSolution>(
                                solutionCloner.Clone(rebuildSolution),
                                rebuildObjective,
                                isNew: true));

                        state =
                            CreateState(
                                iteration,
                                diversificationEvaluated,
                                referenceSet,
                                subsetsGenerated,
                                combinedEvaluated,
                                referenceSetUpdates,
                                improvementInvocations);

                        StoppingDecision rebuildStop =
                            context.EvaluateStopping(state);

                        if (rebuildStop.ShouldStop)
                            return context.Complete(rebuildStop, state);
                    }

                    if (_referenceSetRebuilding.TryRebuild(
                            referenceSet,
                            rebuildPopulation,
                            parameters.QualityReferenceSetSize,
                            _distance,
                            problem.Sense,
                            solutionCloner))
                    {
                        referenceSetRebuilds++;
                        continue;
                    }
                }

                return context.Complete(
                    StoppingDecision.Stop(
                        "ReferenceSetStable",
                        "The complete Scatter Search round produced no accepted RefSet update and no enabled RefSet rebuild refreshed the search."),
                    state);
            }
        }

        state =
            CreateState(
                parameters.MaximumIterations,
                diversificationEvaluated,
                referenceSet,
                subsetsGenerated,
                combinedEvaluated,
                referenceSetUpdates,
                improvementInvocations);

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumScatterSearchIterations",
                "The configured Scatter Search round limit was reached."),
            state);
    }

    private static ScatterSearchState CreateState(
        int iterationsCompleted,
        int diversificationEvaluated,
        IReadOnlyList<ScatterSearchReferencePoint<TSolution>> referenceSet,
        long subsetsGenerated,
        long combinedEvaluated,
        long referenceSetUpdates,
        long improvementInvocations)
    {
        int newReferenceSolutions =
            referenceSet.Count(
                static point => point.IsNew);

        return new ScatterSearchState(
            iterationsCompleted,
            diversificationEvaluated,
            referenceSet.Count,
            newReferenceSolutions,
            subsetsGenerated,
            combinedEvaluated,
            referenceSetUpdates,
            improvementInvocations);
    }
}
