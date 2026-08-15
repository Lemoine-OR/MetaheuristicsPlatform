using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Generic high-performance short-term-memory Tabu Search optimizer.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
/// <typeparam name="TMove">Move description, preferably a compact value type.</typeparam>
/// <typeparam name="TUndo">Compact undo token for reversible candidate evaluation.</typeparam>
/// <typeparam name="TAttribute">Hashable tabu attribute.</typeparam>
/// <typeparam name="TEnumerator">Allocation-free neighborhood cursor.</typeparam>
/// <remarks>
/// The engine performs best-admissible neighborhood selection, attribute-based tabu memory,
/// aspiration, exact-delta evaluation when available, and reversible evaluate/undo otherwise.
/// Intermediate/long-term memory and Reactive Tabu Search are intentionally not approximated
/// by this foundation; their additional state belongs in later dedicated controllers.
///
/// Scientific basis: Glover (1986), Glover (1989), Glover (1990), and Glover &amp; Laguna (1997).
/// </remarks>
public sealed class TabuSearchOptimizer<
    TSolution,
    TMove,
    TUndo,
    TAttribute,
    TEnumerator> :
    IMetaheuristic<TSolution, TabuSearchParameters>
    where TAttribute : notnull
    where TEnumerator : struct, INeighborhoodEnumerator<TMove>
{
    private readonly ITabuSearchInitialSolutionGenerator<TSolution> _initialSolutionGenerator;
    private readonly IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> _neighborhood;
    private readonly IReversibleMoveOperator<TSolution, TMove, TUndo> _moveOperator;
    private readonly ITabuAttributeProvider<TSolution, TMove, TAttribute> _attributeProvider;
    private readonly IMoveObjectiveDeltaEvaluator<TSolution, TMove>? _deltaEvaluator;
    private readonly IMoveApplicability<TSolution, TMove>? _moveApplicability;
    private readonly Func<int, ITabuMemory<TAttribute>> _memoryFactory;

    public TabuSearchOptimizer(
        ITabuSearchInitialSolutionGenerator<TSolution> initialSolutionGenerator,
        IEnumeratedNeighborhood<TSolution, TMove, TEnumerator> neighborhood,
        IReversibleMoveOperator<TSolution, TMove, TUndo> moveOperator,
        ITabuAttributeProvider<TSolution, TMove, TAttribute> attributeProvider,
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
        _deltaEvaluator = deltaEvaluator;
        _moveApplicability = moveApplicability;
        _memoryFactory = memoryFactory ??
            (static capacity => new ExpirationTabuMemory<TAttribute>(capacity));
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "tabu-search-glover",
            Name = "Tabu Search",
            Acronym = "TS",
            SolutionModel = MetaheuristicSolutionModel.SingleSolution,
            Families = MetaheuristicFamily.TrajectoryBased | MetaheuristicFamily.LocalSearch,
            Mechanisms = MetaheuristicMechanism.Neighborhood |
                         MetaheuristicMechanism.Trajectory |
                         MetaheuristicMechanism.MemoryBased,
            SearchSpaces = SearchSpaceKind.Continuous |
                           SearchSpaceKind.Binary |
                           SearchSpaceKind.Integer |
                           SearchSpaceKind.Permutation |
                           SearchSpaceKind.Combinatorial |
                           SearchSpaceKind.Mixed,
            IsStochastic = false,
            References = new[]
            {
                TabuSearchReferences.Glover1986,
                TabuSearchReferences.Glover1989,
                TabuSearchReferences.Glover1990,
                TabuSearchReferences.GloverLaguna1997
            }
        };

    public TabuSearchParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        TabuSearchParameters parameters,
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

        TSolution solution = _initialSolutionGenerator.Create(problem, context.Random);
        ITabuTenurePolicy tenurePolicy = parameters.CreateTenurePolicy();
        ITabuAspirationCriterion aspirationCriterion = parameters.CreateAspirationCriterion();
        ITabuMemory<TAttribute> tabuMemory =
            _memoryFactory(parameters.MemoryInitialCapacity) ??
            throw new InvalidOperationException("The tabu-memory factory returned null.");

        long movesExamined = 0;
        long applicableMoves = 0;
        long candidateEvaluations = 0;
        long deltaEvaluations = 0;
        long fullEvaluations = 0;
        long tabuRejections = 0;
        long aspirationOverrides = 0;
        long selectedMoves = 0;
        long improvingMoves = 0;
        long equalMoves = 0;
        long worseningMoves = 0;
        int lastTabuTenure = 0;

        var initialState = CreateState(
            double.NaN,
            problem.Sense.WorstValue(),
            movesExamined,
            applicableMoves,
            candidateEvaluations,
            deltaEvaluations,
            fullEvaluations,
            tabuRejections,
            aspirationOverrides,
            selectedMoves,
            improvingMoves,
            equalMoves,
            worseningMoves,
            tabuMemory.Count,
            lastTabuTenure);

        context.Start(initialState);

        double currentObjective = context.Evaluate(solution, initialState);

        TabuSearchState state = CreateState(
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
            improvingMoves,
            equalMoves,
            worseningMoves,
            tabuMemory.Count,
            lastTabuTenure);

        StoppingDecision stop = context.EvaluateStopping(state);
        if (stop.ShouldStop)
        {
            return context.Complete(stop, state);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long iteration = context.State.Iteration + 1;
            tabuMemory.Advance(iteration);

            TEnumerator enumerator = _neighborhood.GetEnumerator(in solution);

            bool selected = false;
            TMove selectedMove = default!;
            TAttribute selectedAttributeToForbid = default!;
            double selectedObjective = problem.Sense.WorstValue();
            long selectedEvaluationIndex = 0;
            int applicableThisIteration = 0;
            int admissibleThisIteration = 0;

            while (enumerator.MoveNext(out TMove move))
            {
                cancellationToken.ThrowIfCancellationRequested();
                movesExamined++;

                if (_moveApplicability is not null &&
                    !_moveApplicability.IsApplicable(in solution, in move))
                {
                    continue;
                }

                applicableMoves++;
                applicableThisIteration++;

                TAttribute candidateAttribute =
                    _attributeProvider.GetCandidateAttribute(in solution, in move);

                bool isTabu = tabuMemory.IsTabu(in candidateAttribute, iteration);
                bool aspirationGrantedBeforeEvaluation = false;

                if (isTabu && !aspirationCriterion.RequiresCandidateObjective)
                {
                    var preEvaluationAspirationContext = new TabuAspirationContext(
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

                double candidateObjective = EvaluateCandidateObjective(
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
                    context.RegisterExternalProbeEvaluation(candidateObjective);

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
                    improvingMoves,
                    equalMoves,
                    worseningMoves,
                    tabuMemory.Count,
                    lastTabuTenure);

                stop = context.EvaluateStopping(state);
                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }

                if (isTabu && !aspirationGrantedBeforeEvaluation)
                {
                    var aspirationContext = new TabuAspirationContext(
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

                if (!selected ||
                    problem.Sense.IsBetter(candidateObjective, selectedObjective))
                {
                    selected = true;
                    selectedMove = move;
                    selectedObjective = candidateObjective;
                    selectedEvaluationIndex = evaluationIndex;
                    selectedAttributeToForbid =
                        _attributeProvider.GetAttributeToForbid(in solution, in move);
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
                    improvingMoves,
                    equalMoves,
                    worseningMoves,
                    tabuMemory.Count,
                    lastTabuTenure);

                string code = applicableThisIteration == 0
                    ? "NeighborhoodExhausted"
                    : "NoAdmissibleMove";

                string message = applicableThisIteration == 0
                    ? "The enumerated neighborhood contained no applicable move."
                    : "Every applicable move was tabu and no aspiration criterion released a candidate.";

                return context.Complete(StoppingDecision.Stop(code, message), state);
            }

            if (admissibleThisIteration <= 0)
            {
                throw new InvalidOperationException(
                    "A selected Tabu Search move requires at least one admissible candidate.");
            }

            double previousObjective = currentObjective;

            _moveOperator.Apply(ref solution, in selectedMove);
            currentObjective = selectedObjective;
            selectedMoves++;

            if (problem.Sense.IsBetter(currentObjective, previousObjective))
            {
                improvingMoves++;
            }
            else if (problem.Sense.IsBetter(previousObjective, currentObjective))
            {
                worseningMoves++;
            }
            else
            {
                equalMoves++;
            }

            if (context.WouldImprove(currentObjective))
            {
                TSolution ownedSnapshot = solutionCloner.Clone(solution);
                context.PromoteOwnedExternalProbeSnapshot(
                    ownedSnapshot,
                    currentObjective,
                    selectedEvaluationIndex);
            }

            var tenureContext = new TabuTenureContext(
                iteration,
                previousObjective,
                currentObjective,
                context.State.BestFitness,
                movesExamined,
                tabuRejections,
                aspirationOverrides);

            int tenure = tenurePolicy.GetTenure(in tenureContext, context.Random);
            if (tenure <= 0)
            {
                throw new InvalidOperationException(
                    "The Tabu Search tenure policy returned a non-positive tenure.");
            }

            lastTabuTenure = tenure;
            long tabuUntilIteration = checked(iteration + tenure);
            tabuMemory.Register(in selectedAttributeToForbid, tabuUntilIteration);

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
                improvingMoves,
                equalMoves,
                worseningMoves,
                tabuMemory.Count,
                lastTabuTenure);

            context.CompleteIteration(currentObjective, state);

            stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }
        }
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
        TUndo undo = _moveOperator.CaptureUndo(in solution, in move);
        _moveOperator.Apply(ref solution, in move);

        try
        {
            return problem.Evaluate(solution);
        }
        finally
        {
            _moveOperator.Undo(ref solution, in move, in undo);
        }
    }

    private static TabuSearchState CreateState(
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
        long improvingMoves,
        long equalMoves,
        long worseningMoves,
        int activeTabuAttributes,
        int lastTabuTenure) =>
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
            improvingMoves,
            equalMoves,
            worseningMoves,
            activeTabuAttributes,
            lastTabuTenure);
}
