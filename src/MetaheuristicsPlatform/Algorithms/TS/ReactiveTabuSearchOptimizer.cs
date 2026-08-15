using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Generic Reactive Tabu Search optimizer following Battiti and Tecchiolli (1994).
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
/// <typeparam name="TMove">Move description, preferably a compact value type.</typeparam>
/// <typeparam name="TUndo">Compact undo token for reversible candidate evaluation.</typeparam>
/// <typeparam name="TAttribute">Hashable tabu attribute.</typeparam>
/// <typeparam name="TEnumerator">Allocation-free neighborhood cursor.</typeparam>
/// <remarks>
/// The implementation adds explicit configuration-repetition memory to the Glover-style
/// short-term Tabu Search foundation. The prohibition period reacts to detected cycles and
/// decreases when repetition evidence disappears. Persistent repetition activates an escape
/// phase composed of uniformly sampled applicable moves, with requested length proportional
/// to the moving average of observed cycle lengths.
///
/// Optional long-term frequency bias and elite intensification are generic Glover-style
/// components layered on the same trajectory. They are disabled by default so the canonical
/// reactive mechanism remains independently observable.
///
/// Scientific basis:
/// Battiti and Tecchiolli (1994), DOI 10.1287/ijoc.6.2.126;
/// Glover (1989), DOI 10.1287/ijoc.1.3.190;
/// Glover (1990), DOI 10.1287/ijoc.2.1.4.
/// </remarks>
public sealed class ReactiveTabuSearchOptimizer<
    TSolution,
    TMove,
    TUndo,
    TAttribute,
    TEnumerator> :
    IMetaheuristic<TSolution, ReactiveTabuSearchParameters>
    where TAttribute : notnull
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    private readonly ITabuSearchInitialSolutionGenerator<TSolution>
        _initialSolutionGenerator;
    private readonly IEnumeratedNeighborhood<TSolution, TMove, TEnumerator>
        _neighborhood;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo>
        _moveOperator;
    private readonly ITabuAttributeProvider<TSolution, TMove, TAttribute>
        _attributeProvider;
    private readonly ITabuSearchSolutionSignatureProvider<TSolution>
        _signatureProvider;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>?
        _deltaEvaluator;
    private readonly IMoveApplicability<TSolution, TMove>?
        _moveApplicability;
    private readonly Func<int, ITabuMemory<TAttribute>>
        _memoryFactory;

    public ReactiveTabuSearchOptimizer(
        ITabuSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        ITabuAttributeProvider<TSolution, TMove, TAttribute> attributeProvider,
        ITabuSearchSolutionSignatureProvider<TSolution> signatureProvider,
        IMoveObjectiveDeltaEvaluator<TSolution, TMove>? deltaEvaluator = null,
        IMoveApplicability<TSolution, TMove>? moveApplicability = null,
        Func<int, ITabuMemory<TAttribute>>? memoryFactory = null)
    {
        _initialSolutionGenerator = initialSolutionGenerator ??
            throw new ArgumentNullException(nameof(initialSolutionGenerator));
        _neighborhood = neighborhood ??
            throw new ArgumentNullException(nameof(neighborhood));
        _moveOperator = moveOperator ??
            throw new ArgumentNullException(nameof(moveOperator));
        _attributeProvider = attributeProvider ??
            throw new ArgumentNullException(nameof(attributeProvider));
        _signatureProvider = signatureProvider ??
            throw new ArgumentNullException(nameof(signatureProvider));
        _deltaEvaluator = deltaEvaluator;
        _moveApplicability = moveApplicability;
        _memoryFactory = memoryFactory ??
            (static capacity => new ExpirationTabuMemory<TAttribute>(capacity));
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "reactive-tabu-search-battiti-tecchiolli-1994",
            Name = "Reactive Tabu Search",
            Acronym = "RTS",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families =
                MetaheuristicFamily.TrajectoryBased |
                MetaheuristicFamily.LocalSearch,
            Mechanisms =
                MetaheuristicMechanism.Neighborhood |
                MetaheuristicMechanism.Trajectory |
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces =
                SearchSpaceKind.Continuous |
                SearchSpaceKind.Binary |
                SearchSpaceKind.Integer |
                SearchSpaceKind.Permutation |
                SearchSpaceKind.Combinatorial |
                SearchSpaceKind.Mixed,
            IsStochastic = true,
            References =
                new[]
                {
                    TabuSearchReferences.BattitiTecchiolli1994,
                    TabuSearchReferences.Glover1989,
                    TabuSearchReferences.Glover1990,
                    TabuSearchReferences.GloverLaguna1997
                }
        };

    public ReactiveTabuSearchParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        ReactiveTabuSearchParameters parameters,
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

        var context = new OptimizationContext<TSolution>(
            Descriptor,
            problem,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);

        TSolution solution =
            _initialSolutionGenerator.Create(problem, context.Random);

        IReactiveTabuTenurePolicy reactivePolicy =
            parameters.CreateReactiveTenurePolicy();

        ITabuAspirationCriterion aspirationCriterion =
            parameters.CreateAspirationCriterion();

        ITabuMemory<TAttribute> tabuMemory =
            CreateMemory(parameters.MemoryInitialCapacity);

        var repetitionMemory =
            new ConfigurationRepetitionMemory(
                parameters.MemoryInitialCapacity);

        AttributeFrequencyMemory<TAttribute>? frequencyMemory =
            parameters.FrequencyPenaltyWeight > 0.0
                ? new AttributeFrequencyMemory<TAttribute>(
                    parameters.MemoryInitialCapacity)
                : null;

        bool intensificationEnabled =
            parameters.IntensificationAfterIterationsWithoutImprovement > 0;

        long movesExamined = 0;
        long applicableMoves = 0;
        long candidateEvaluations = 0;
        long deltaEvaluations = 0;
        long fullEvaluations = 0;
        long tabuRejections = 0;
        long aspirationOverrides = 0;
        long selectedMoves = 0;
        long repeatedConfigurations = 0;
        long lastCycleLength = 0;
        long tenureChanges = 0;
        long intensificationRestarts = 0;
        long diversificationPhases = 0;
        long diversificationMoves = 0;
        int diversificationMovesRemaining = 0;
        long iterationsSinceBestImprovement = 0;

        var initialState = CreateState(
            currentObjective: double.NaN,
            bestObjective: problem.Sense.WorstValue(),
            movesExamined,
            applicableMoves,
            candidateEvaluations,
            deltaEvaluations,
            fullEvaluations,
            tabuRejections,
            aspirationOverrides,
            selectedMoves,
            activeTabuAttributes: tabuMemory.Count,
            currentTabuTenure: reactivePolicy.CurrentTenure,
            trackedConfigurations: repetitionMemory.Count,
            repeatedConfigurations,
            lastCycleLength,
            movingAverageCycleLength: reactivePolicy.MovingAverageCycleLength,
            tenureChanges,
            frequencyTrackedAttributes: frequencyMemory?.Count ?? 0,
            intensificationRestarts,
            diversificationPhases,
            diversificationMoves,
            diversificationMovesRemaining,
            iterationsSinceBestImprovement);

        context.Start(initialState);

        double currentObjective =
            context.Evaluate(solution, initialState);

        TSolution eliteSolution =
            intensificationEnabled
                ? solutionCloner.Clone(solution)
                : solution;
        double eliteObjective =
            currentObjective;

        ulong initialSignature =
            _signatureProvider.GetSignature(in solution);
        repetitionMemory.Observe(initialSignature, iteration: 0);

        ReactiveTabuSearchState state = CreateState(
            currentObjective,
            context.State.BestFitness,
            movesExamined,
            applicableMoves,
            candidateEvaluations,
            deltaEvaluations,
            fullEvaluations,
            tabuRejections,
            aspirationOverrides,
            selectedMoves,
            tabuMemory.Count,
            reactivePolicy.CurrentTenure,
            repetitionMemory.Count,
            repeatedConfigurations,
            lastCycleLength,
            reactivePolicy.MovingAverageCycleLength,
            tenureChanges,
            frequencyMemory?.Count ?? 0,
            intensificationRestarts,
            diversificationPhases,
            diversificationMoves,
            diversificationMovesRemaining,
            iterationsSinceBestImprovement);

        StoppingDecision stop =
            context.EvaluateStopping(state);

        if (stop.ShouldStop)
        {
            return context.Complete(stop, state);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long iteration =
                context.State.Iteration + 1;

            tabuMemory.Advance(iteration);

            if (diversificationMovesRemaining > 0)
            {
                bool moved = TryExecuteDiversificationMove(
                    problem,
                    ref solution,
                    currentObjective,
                    iteration,
                    context,
                    solutionCloner,
                    ref eliteSolution,
                    ref eliteObjective,
                    reactivePolicy,
                    repetitionMemory,
                    frequencyMemory,
                    intensificationEnabled,
                    ref tabuMemory,
                    ref movesExamined,
                    ref applicableMoves,
                    ref candidateEvaluations,
                    ref deltaEvaluations,
                    ref fullEvaluations,
                    ref selectedMoves,
                    ref repeatedConfigurations,
                    ref lastCycleLength,
                    ref tenureChanges,
                    ref diversificationMoves,
                    ref diversificationMovesRemaining,
                    ref iterationsSinceBestImprovement,
                    cancellationToken,
                    out currentObjective);

                if (!moved)
                {
                    state = CreateState(
                        currentObjective,
                        context.State.BestFitness,
                        movesExamined,
                        applicableMoves,
                        candidateEvaluations,
                        deltaEvaluations,
                        fullEvaluations,
                        tabuRejections,
                        aspirationOverrides,
                        selectedMoves,
                        tabuMemory.Count,
                        reactivePolicy.CurrentTenure,
                        repetitionMemory.Count,
                        repeatedConfigurations,
                        lastCycleLength,
                        reactivePolicy.MovingAverageCycleLength,
                        tenureChanges,
                        frequencyMemory?.Count ?? 0,
                        intensificationRestarts,
                        diversificationPhases,
                        diversificationMoves,
                        diversificationMovesRemaining,
                        iterationsSinceBestImprovement);

                    return context.Complete(
                        StoppingDecision.Stop(
                            "NeighborhoodExhausted",
                            "The diversification neighborhood contained no applicable move."),
                        state);
                }

                state = CreateState(
                    currentObjective,
                    context.State.BestFitness,
                    movesExamined,
                    applicableMoves,
                    candidateEvaluations,
                    deltaEvaluations,
                    fullEvaluations,
                    tabuRejections,
                    aspirationOverrides,
                    selectedMoves,
                    tabuMemory.Count,
                    reactivePolicy.CurrentTenure,
                    repetitionMemory.Count,
                    repeatedConfigurations,
                    lastCycleLength,
                    reactivePolicy.MovingAverageCycleLength,
                    tenureChanges,
                    frequencyMemory?.Count ?? 0,
                    intensificationRestarts,
                    diversificationPhases,
                    diversificationMoves,
                    diversificationMovesRemaining,
                    iterationsSinceBestImprovement);

                context.CompleteIteration(
                    currentObjective,
                    state);

                stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }

                continue;
            }

            TEnumerator enumerator =
                _neighborhood.GetEnumerator(in solution);

            bool selected = false;
            TMove selectedMove = default!;
            TAttribute selectedCandidateAttribute = default!;
            TAttribute selectedAttributeToForbid = default!;
            double selectedObjective =
                problem.Sense.WorstValue();
            double selectedScore =
                problem.Sense.WorstValue();
            long selectedEvaluationIndex = 0;
            int applicableThisIteration = 0;
            int admissibleThisIteration = 0;

            while (enumerator.MoveNext(out TMove move))
            {
                cancellationToken.ThrowIfCancellationRequested();
                movesExamined++;

                if (_moveApplicability is not null &&
                    !_moveApplicability.IsApplicable(
                        in solution,
                        in move))
                {
                    continue;
                }

                applicableMoves++;
                applicableThisIteration++;

                TAttribute candidateAttribute =
                    _attributeProvider.GetCandidateAttribute(
                        in solution,
                        in move);

                bool isTabu =
                    tabuMemory.IsTabu(
                        in candidateAttribute,
                        iteration);

                bool aspirationGrantedBeforeEvaluation = false;

                if (isTabu &&
                    !aspirationCriterion.RequiresCandidateObjective)
                {
                    var preEvaluationAspirationContext =
                        new TabuAspirationContext(
                            iteration,
                            evaluationIndex: 0,
                            currentObjective,
                            context.State.BestFitness,
                            candidateObjective: double.NaN);

                    if (!aspirationCriterion.IsAspirational(
                            in preEvaluationAspirationContext,
                            problem.Sense))
                    {
                        tabuRejections++;
                        continue;
                    }

                    aspirationGrantedBeforeEvaluation = true;
                    aspirationOverrides++;
                }

                double candidateObjective =
                    EvaluateCandidateObjective(
                        problem,
                        ref solution,
                        currentObjective,
                        in move,
                        out bool usedDelta);

                if (usedDelta)
                {
                    deltaEvaluations++;
                }
                else
                {
                    fullEvaluations++;
                }

                candidateEvaluations++;

                long evaluationIndex =
                    context.RegisterExternalProbeEvaluation(
                        candidateObjective);

                state = CreateState(
                    currentObjective,
                    context.State.BestFitness,
                    movesExamined,
                    applicableMoves,
                    candidateEvaluations,
                    deltaEvaluations,
                    fullEvaluations,
                    tabuRejections,
                    aspirationOverrides,
                    selectedMoves,
                    tabuMemory.Count,
                    reactivePolicy.CurrentTenure,
                    repetitionMemory.Count,
                    repeatedConfigurations,
                    lastCycleLength,
                    reactivePolicy.MovingAverageCycleLength,
                    tenureChanges,
                    frequencyMemory?.Count ?? 0,
                    intensificationRestarts,
                    diversificationPhases,
                    diversificationMoves,
                    diversificationMovesRemaining,
                    iterationsSinceBestImprovement);

                stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }

                if (isTabu &&
                    !aspirationGrantedBeforeEvaluation)
                {
                    var aspirationContext =
                        new TabuAspirationContext(
                            iteration,
                            evaluationIndex,
                            currentObjective,
                            context.State.BestFitness,
                            candidateObjective);

                    if (!aspirationCriterion.IsAspirational(
                            in aspirationContext,
                            problem.Sense))
                    {
                        tabuRejections++;
                        continue;
                    }

                    aspirationOverrides++;
                }

                admissibleThisIteration++;

                long candidateFrequency =
                    frequencyMemory is null
                        ? 0L
                        : frequencyMemory.GetFrequency(
                            in candidateAttribute);

                double candidateScore =
                    ApplyFrequencyPenalty(
                        candidateObjective,
                        candidateFrequency,
                        parameters.FrequencyPenaltyWeight,
                        problem.Sense);

                if (!selected ||
                    problem.Sense.IsBetter(
                        candidateScore,
                        selectedScore) ||
                    (candidateScore == selectedScore &&
                     problem.Sense.IsBetter(
                         candidateObjective,
                         selectedObjective)))
                {
                    selected = true;
                    selectedMove = move;
                    selectedCandidateAttribute =
                        candidateAttribute;
                    selectedObjective =
                        candidateObjective;
                    selectedScore =
                        candidateScore;
                    selectedEvaluationIndex =
                        evaluationIndex;
                    selectedAttributeToForbid =
                        _attributeProvider.GetAttributeToForbid(
                            in solution,
                            in move);
                }
            }

            if (!selected)
            {
                state = CreateState(
                    currentObjective,
                    context.State.BestFitness,
                    movesExamined,
                    applicableMoves,
                    candidateEvaluations,
                    deltaEvaluations,
                    fullEvaluations,
                    tabuRejections,
                    aspirationOverrides,
                    selectedMoves,
                    tabuMemory.Count,
                    reactivePolicy.CurrentTenure,
                    repetitionMemory.Count,
                    repeatedConfigurations,
                    lastCycleLength,
                    reactivePolicy.MovingAverageCycleLength,
                    tenureChanges,
                    frequencyMemory?.Count ?? 0,
                    intensificationRestarts,
                    diversificationPhases,
                    diversificationMoves,
                    diversificationMovesRemaining,
                    iterationsSinceBestImprovement);

                string code =
                    applicableThisIteration == 0
                        ? "NeighborhoodExhausted"
                        : "NoAdmissibleMove";

                string message =
                    applicableThisIteration == 0
                        ? "The enumerated neighborhood contained no applicable move."
                        : "Every applicable move was tabu and no aspiration criterion released a candidate.";

                return context.Complete(
                    StoppingDecision.Stop(
                        code,
                        message),
                    state);
            }

            if (admissibleThisIteration <= 0)
            {
                throw new InvalidOperationException(
                    "A selected Reactive Tabu Search move requires at least one admissible candidate.");
            }

            _moveOperator.Apply(
                ref solution,
                in selectedMove);

            currentObjective =
                selectedObjective;
            selectedMoves++;

            bool improvedGlobalBest =
                context.WouldImprove(currentObjective);

            if (improvedGlobalBest)
            {
                TSolution ownedSnapshot =
                    solutionCloner.Clone(solution);

                context.PromoteOwnedExternalProbeSnapshot(
                    ownedSnapshot,
                    currentObjective,
                    selectedEvaluationIndex);

                if (intensificationEnabled)
                {
                    eliteSolution =
                        solutionCloner.Clone(solution);
                    eliteObjective =
                        currentObjective;
                }
                iterationsSinceBestImprovement = 0;
            }
            else
            {
                iterationsSinceBestImprovement++;
            }

            if (frequencyMemory is not null)
            {
                frequencyMemory.Record(
                    in selectedCandidateAttribute);
            }

            TabuSearchRepetitionObservation repetition =
                ObserveConfiguration(
                    solution,
                    iteration,
                    repetitionMemory,
                    ref repeatedConfigurations,
                    ref lastCycleLength);

            ReactiveTabuReaction reaction =
                ObserveReactivePolicy(
                    reactivePolicy,
                    iteration,
                    in repetition,
                    currentObjective,
                    context.State.BestFitness,
                    ref tenureChanges);

            RegisterTabuAttribute(
                tabuMemory,
                in selectedAttributeToForbid,
                iteration,
                reaction.TabuTenure);

            if (reaction.DiversificationRequested)
            {
                diversificationMovesRemaining =
                    Math.Max(
                        diversificationMovesRemaining,
                        reaction.DiversificationMoves);

                diversificationPhases++;
                reactivePolicy.AcknowledgeDiversification();
            }

            if (intensificationEnabled &&
                diversificationMovesRemaining == 0 &&
                iterationsSinceBestImprovement >=
                    parameters.IntensificationAfterIterationsWithoutImprovement)
            {
                solution =
                    solutionCloner.Clone(eliteSolution);
                currentObjective =
                    eliteObjective;
                tabuMemory =
                    CreateMemory(parameters.MemoryInitialCapacity);
                intensificationRestarts++;
                iterationsSinceBestImprovement = 0;
            }

            state = CreateState(
                currentObjective,
                context.State.BestFitness,
                movesExamined,
                applicableMoves,
                candidateEvaluations,
                deltaEvaluations,
                fullEvaluations,
                tabuRejections,
                aspirationOverrides,
                selectedMoves,
                tabuMemory.Count,
                reactivePolicy.CurrentTenure,
                repetitionMemory.Count,
                repeatedConfigurations,
                lastCycleLength,
                reactivePolicy.MovingAverageCycleLength,
                tenureChanges,
                frequencyMemory?.Count ?? 0,
                intensificationRestarts,
                diversificationPhases,
                diversificationMoves,
                diversificationMovesRemaining,
                iterationsSinceBestImprovement);

            context.CompleteIteration(
                currentObjective,
                state);

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    state);
            }
        }
    }

    private bool TryExecuteDiversificationMove(
        IOptimizationProblem<TSolution> problem,
        ref TSolution solution,
        double currentObjective,
        long iteration,
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        ref TSolution eliteSolution,
        ref double eliteObjective,
        IReactiveTabuTenurePolicy reactivePolicy,
        ConfigurationRepetitionMemory repetitionMemory,
        AttributeFrequencyMemory<TAttribute>? frequencyMemory,
        bool intensificationEnabled,
        ref ITabuMemory<TAttribute> tabuMemory,
        ref long movesExamined,
        ref long applicableMoves,
        ref long candidateEvaluations,
        ref long deltaEvaluations,
        ref long fullEvaluations,
        ref long selectedMoves,
        ref long repeatedConfigurations,
        ref long lastCycleLength,
        ref long tenureChanges,
        ref long diversificationMoves,
        ref int diversificationMovesRemaining,
        ref long iterationsSinceBestImprovement,
        CancellationToken cancellationToken,
        out double resultingObjective)
    {
        TEnumerator enumerator =
            _neighborhood.GetEnumerator(in solution);

        bool selected = false;
        int applicableCount = 0;
        TMove selectedMove = default!;
        TAttribute selectedCandidateAttribute = default!;
        TAttribute selectedAttributeToForbid = default!;

        while (enumerator.MoveNext(out TMove move))
        {
            cancellationToken.ThrowIfCancellationRequested();
            movesExamined++;

            if (_moveApplicability is not null &&
                !_moveApplicability.IsApplicable(
                    in solution,
                    in move))
            {
                continue;
            }

            applicableMoves++;
            applicableCount++;

            if (context.Random.NextInt32(
                    applicableCount) != 0)
            {
                continue;
            }

            selected = true;
            selectedMove = move;
            selectedCandidateAttribute =
                _attributeProvider.GetCandidateAttribute(
                    in solution,
                    in move);
            selectedAttributeToForbid =
                _attributeProvider.GetAttributeToForbid(
                    in solution,
                    in move);
        }

        if (!selected)
        {
            resultingObjective = currentObjective;
            return false;
        }

        double candidateObjective;

        if (_deltaEvaluator is not null &&
            _deltaEvaluator.TryEvaluateCandidateObjective(
                in solution,
                currentObjective,
                in selectedMove,
                out candidateObjective))
        {
            deltaEvaluations++;
            _moveOperator.Apply(
                ref solution,
                in selectedMove);
        }
        else
        {
            _moveOperator.Apply(
                ref solution,
                in selectedMove);
            candidateObjective =
                problem.Evaluate(solution);
            fullEvaluations++;
        }

        candidateEvaluations++;
        selectedMoves++;
        diversificationMoves++;
        diversificationMovesRemaining--;

        bool improvedGlobalBest =
            context.WouldImprove(candidateObjective);

        context.RegisterExternalEvaluation(
            solution,
            candidateObjective);

        if (improvedGlobalBest)
        {
            if (intensificationEnabled)
            {
                eliteSolution =
                    solutionCloner.Clone(solution);
                eliteObjective =
                    candidateObjective;
            }
            iterationsSinceBestImprovement = 0;
        }
        else
        {
            iterationsSinceBestImprovement++;
        }

        if (frequencyMemory is not null)
        {
            frequencyMemory.Record(
                in selectedCandidateAttribute);
        }

        TabuSearchRepetitionObservation repetition =
            ObserveConfiguration(
                solution,
                iteration,
                repetitionMemory,
                ref repeatedConfigurations,
                ref lastCycleLength);

        ReactiveTabuReaction reaction =
            ObserveReactivePolicy(
                reactivePolicy,
                iteration,
                in repetition,
                candidateObjective,
                context.State.BestFitness,
                ref tenureChanges);

        RegisterTabuAttribute(
            tabuMemory,
            in selectedAttributeToForbid,
            iteration,
            reaction.TabuTenure);

        if (reaction.DiversificationRequested)
        {
            diversificationMovesRemaining =
                Math.Max(
                    diversificationMovesRemaining,
                    reaction.DiversificationMoves);
            reactivePolicy.AcknowledgeDiversification();
        }

        resultingObjective =
            candidateObjective;

        return true;
    }

    private TabuSearchRepetitionObservation ObserveConfiguration(
        in TSolution solution,
        long iteration,
        ConfigurationRepetitionMemory repetitionMemory,
        ref long repeatedConfigurations,
        ref long lastCycleLength)
    {
        ulong signature =
            _signatureProvider.GetSignature(
                in solution);

        TabuSearchRepetitionObservation observation =
            repetitionMemory.Observe(
                signature,
                iteration);

        if (observation.IsRepetition)
        {
            repeatedConfigurations++;
            lastCycleLength =
                observation.CycleLength;
        }

        return observation;
    }

    private static ReactiveTabuReaction ObserveReactivePolicy(
        IReactiveTabuTenurePolicy reactivePolicy,
        long iteration,
        in TabuSearchRepetitionObservation repetition,
        double currentObjective,
        double bestObjective,
        ref long tenureChanges)
    {
        var reactiveContext =
            new ReactiveTabuTenureContext(
                iteration,
                in repetition,
                currentObjective,
                bestObjective);

        ReactiveTabuReaction reaction =
            reactivePolicy.Observe(
                in reactiveContext);

        if (reaction.TabuTenure <= 0)
        {
            throw new InvalidOperationException(
                "The reactive tabu-tenure policy returned a non-positive tenure.");
        }

        if (reaction.DiversificationRequested &&
            reaction.DiversificationMoves <= 0)
        {
            throw new InvalidOperationException(
                "A reactive diversification request must contain a positive move count.");
        }

        if (reaction.TenureChanged)
        {
            tenureChanges++;
        }

        return reaction;
    }

    private static void RegisterTabuAttribute(
        ITabuMemory<TAttribute> tabuMemory,
        in TAttribute attribute,
        long iteration,
        int tenure)
    {
        long tabuUntilIteration =
            checked(iteration + tenure);

        tabuMemory.Register(
            in attribute,
            tabuUntilIteration);
    }

    private double EvaluateCandidateObjective(
        IOptimizationProblem<TSolution> problem,
        ref TSolution solution,
        double currentObjective,
        in TMove move,
        out bool usedDelta)
    {
        if (_deltaEvaluator is not null &&
            _deltaEvaluator.TryEvaluateCandidateObjective(
                in solution,
                currentObjective,
                in move,
                out double candidateObjective))
        {
            usedDelta = true;
            return candidateObjective;
        }

        usedDelta = false;

        TUndo undo =
            _moveOperator.CaptureUndo(
                in solution,
                in move);

        _moveOperator.Apply(
            ref solution,
            in move);

        try
        {
            return problem.Evaluate(solution);
        }
        finally
        {
            _moveOperator.Undo(
                ref solution,
                in move,
                in undo);
        }
    }

    private ITabuMemory<TAttribute> CreateMemory(
        int initialCapacity) =>
        _memoryFactory(initialCapacity) ??
        throw new InvalidOperationException(
            "The tabu-memory factory returned null.");

    private static double ApplyFrequencyPenalty(
        double candidateObjective,
        long frequency,
        double penaltyWeight,
        OptimizationSense sense)
    {
        if (penaltyWeight <= 0.0 ||
            frequency <= 0)
        {
            return candidateObjective;
        }

        double penalty =
            penaltyWeight * frequency;

        double score =
            sense == OptimizationSense.Minimize
                ? candidateObjective + penalty
                : candidateObjective - penalty;

        if (!double.IsFinite(score))
        {
            throw new InvalidOperationException(
                "The frequency-diversification score became non-finite.");
        }

        return score;
    }

    private static ReactiveTabuSearchState CreateState(
        double currentObjective,
        double bestObjective,
        long movesExamined,
        long applicableMoves,
        long candidateEvaluations,
        long deltaEvaluations,
        long fullEvaluations,
        long tabuRejections,
        long aspirationOverrides,
        long selectedMoves,
        int activeTabuAttributes,
        int currentTabuTenure,
        int trackedConfigurations,
        long repeatedConfigurations,
        long lastCycleLength,
        double movingAverageCycleLength,
        long tenureChanges,
        int frequencyTrackedAttributes,
        long intensificationRestarts,
        long diversificationPhases,
        long diversificationMoves,
        int diversificationMovesRemaining,
        long iterationsSinceBestImprovement) =>
        new(
            currentObjective,
            bestObjective,
            movesExamined,
            applicableMoves,
            candidateEvaluations,
            deltaEvaluations,
            fullEvaluations,
            tabuRejections,
            aspirationOverrides,
            selectedMoves,
            activeTabuAttributes,
            currentTabuTenure,
            trackedConfigurations,
            repeatedConfigurations,
            lastCycleLength,
            movingAverageCycleLength,
            tenureChanges,
            frequencyTrackedAttributes,
            intensificationRestarts,
            diversificationPhases,
            diversificationMoves,
            diversificationMovesRemaining,
            iterationsSinceBestImprovement);
}
