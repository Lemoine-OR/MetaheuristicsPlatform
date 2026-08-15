using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Neighborhoods;
using MetaheuristicsPlatform.Stopping;
using MetaheuristicsPlatform.Trajectory;
using MetaheuristicsPlatform.Trajectory.Moves;

namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Generic high-performance Simulated Annealing optimizer.
/// </summary>
/// <typeparam name="TSolution">Arbitrary solution representation.</typeparam>
/// <typeparam name="TMove">Move description, typically a small readonly struct.</typeparam>
/// <typeparam name="TUndo">Compact reversible-move undo token.</typeparam>
/// <remarks>
/// The implementation uses the v0.17 reversible trajectory executor.
///
/// When an exact move objective evaluator is supplied:
/// - rejected moves are not applied;
/// - accepted moves are applied exactly once;
/// - no full solution clone is required per transition.
///
/// Without a delta evaluator:
/// - the move is applied;
/// - the objective is fully evaluated;
/// - rejected moves are undone.
///
/// Metropolis acceptance:
/// P(accept worsening delta) = exp(-delta/T).
///
/// Scientific references:
/// Metropolis et al. (1953), DOI 10.1063/1.1699114.
/// Kirkpatrick, Gelatt and Vecchi (1983), DOI 10.1126/science.220.4598.671.
/// </remarks>
public sealed class SimulatedAnnealingOptimizer<
    TSolution,
    TMove,
    TUndo> :
    IMetaheuristic<
        TSolution,
        SimulatedAnnealingParameters>
{
    private readonly ISimulatedAnnealingInitialSolutionGenerator<TSolution>
        _initialSolutionGenerator;

    private readonly IStochasticNeighborhood<
        TSolution,
        TMove> _neighborhood;

    private readonly IReversibleMoveOperator<
        TSolution,
        TMove,
        TUndo> _moveOperator;

    private readonly IMoveObjectiveDeltaEvaluator<
        TSolution,
        TMove>? _deltaEvaluator;

    public SimulatedAnnealingOptimizer(
        ISimulatedAnnealingInitialSolutionGenerator<TSolution>
            initialSolutionGenerator,
        IStochasticNeighborhood<TSolution, TMove>
            neighborhood,
        IReversibleMoveOperator<
            TSolution,
            TMove,
            TUndo> moveOperator,
        IMoveObjectiveDeltaEvaluator<
            TSolution,
            TMove>? deltaEvaluator = null)
    {
        _initialSolutionGenerator =
            initialSolutionGenerator ??
            throw new ArgumentNullException(
                nameof(initialSolutionGenerator));

        _neighborhood =
            neighborhood ??
            throw new ArgumentNullException(
                nameof(neighborhood));

        _moveOperator =
            moveOperator ??
            throw new ArgumentNullException(
                nameof(moveOperator));

        _deltaEvaluator =
            deltaEvaluator;
    }

    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "simulated-annealing-metropolis",
            Name = "Simulated Annealing",
            Acronym = "SA",
            SolutionModel =
                MetaheuristicSolutionModel.SingleSolution,
            Families =
                MetaheuristicFamily.TrajectoryBased |
                MetaheuristicFamily.LocalSearch,
            Mechanisms =
                MetaheuristicMechanism.Neighborhood |
                MetaheuristicMechanism.Trajectory,
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
                    SimulatedAnnealingReferences.MetropolisEtAl1953,
                    SimulatedAnnealingReferences.KirkpatrickGelattVecchi1983
                }
        };

    public SimulatedAnnealingParameters
        CreateDefaultParameters() =>
        new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        SimulatedAnnealingParameters parameters,
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

        TSolution solution =
            _initialSolutionGenerator.Create(
                problem,
                context.Random);

        double temperature =
            parameters.InitialTemperature;

        var acceptancePolicy =
            new MetropolisAcceptancePolicy(
                temperature);

        ISimulatedAnnealingCoolingSchedule coolingSchedule =
            parameters.CreateCoolingSchedule();

        bool collectCoolingLevelStatistics =
            coolingSchedule is ISimulatedAnnealingStatisticalCoolingSchedule;

        var coolingLevelStatistics =
            new SimulatedAnnealingLevelStatisticsAccumulator();

        long levelAttemptStart = 0;
        long levelAcceptedStart = 0;

        var executor =
            new ReversibleTrajectoryStepExecutor<
                TSolution,
                TMove,
                TUndo>(
                _moveOperator,
                (
                    in TSolution candidate) =>
                    problem.Evaluate(
                        candidate),
                acceptancePolicy,
                _deltaEvaluator);

        var statistics =
            new TrajectoryStatisticsAccumulator();

        long temperatureLevel = 0;
        int transitionsInLevel = 0;
        int consecutiveSamplingFailures = 0;

        var initialState =
            new SimulatedAnnealingState(
                CurrentObjective:
                    double.NaN,
                BestObjective:
                    problem.Sense.WorstValue(),
                Temperature:
                    temperature,
                TemperatureLevel:
                    temperatureLevel,
                TransitionsInCurrentLevel:
                    transitionsInLevel,
                AttemptedTransitions:
                    0,
                AcceptedTransitions:
                    0,
                ImprovingTransitions:
                    0,
                EqualTransitions:
                    0,
                WorseningTransitions:
                    0,
                DeltaEvaluations:
                    0,
                FullEvaluations:
                    0,
                ConsecutiveSamplingFailures:
                    0);

        context.Start(
            initialState);

        double currentObjective =
            context.Evaluate(
                solution,
                initialState);

        SimulatedAnnealingState state =
            CreateState(
                currentObjective,
                context.State.BestFitness,
                temperature,
                temperatureLevel,
                transitionsInLevel,
                in statistics,
                consecutiveSamplingFailures);

        StoppingDecision stop =
            context.EvaluateStopping(
                state);

        if (stop.ShouldStop)
        {
            return
                context.Complete(
                    stop,
                    state);
        }

        long transitionIndex = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_neighborhood.TrySampleMove(
                    in solution,
                    context.Random,
                    out TMove move))
            {
                consecutiveSamplingFailures++;

                state =
                    CreateState(
                        currentObjective,
                        context.State.BestFitness,
                        temperature,
                        temperatureLevel,
                        transitionsInLevel,
                        in statistics,
                        consecutiveSamplingFailures);

                if (consecutiveSamplingFailures >=
                    parameters.MaximumConsecutiveSamplingFailures)
                {
                    return
                        context.Complete(
                            StoppingDecision.Stop(
                                "NeighborhoodExhausted",
                                "The stochastic neighborhood could not provide a move within the configured consecutive-failure limit."),
                            state);
                }

                stop =
                    context.EvaluateStopping(
                        state);

                if (stop.ShouldStop)
                {
                    return
                        context.Complete(
                            stop,
                            state);
                }

                continue;
            }

            consecutiveSamplingFailures = 0;
            transitionIndex++;

            acceptancePolicy.SetTemperature(
                temperature);

            TrajectoryStepResult step =
                executor.Execute(
                    ref solution,
                    currentObjective,
                    context.State.BestFitness,
                    in move,
                    transitionIndex,
                    problem.Sense,
                    context.Random,
                    cancellationToken);

            RegisterCandidateEvaluation(
                context,
                solutionCloner,
                in solution,
                in step);

            currentObjective =
                step.ResultingObjective;

            statistics.Record(
                in step);

            if (collectCoolingLevelStatistics)
            {
                coolingLevelStatistics.Record(
                    currentObjective);
            }

            transitionsInLevel++;

            if (transitionsInLevel >=
                parameters.TransitionsPerTemperatureLevel)
            {
                temperatureLevel++;

                var coolingContext =
                    new SimulatedAnnealingCoolingContext(
                        CompletedTemperatureLevels:
                            temperatureLevel,
                        AttemptedTransitions:
                            statistics.Attempts,
                        AcceptedTransitions:
                            statistics.Accepted,
                        InitialTemperature:
                            parameters.InitialTemperature,
                        CurrentTemperature:
                            temperature)
                    {
                        LevelAttemptedTransitions =
                            statistics.Attempts -
                            levelAttemptStart,
                        LevelAcceptedTransitions =
                            statistics.Accepted -
                            levelAcceptedStart,
                        LevelObjectiveSamples =
                            collectCoolingLevelStatistics
                                ? coolingLevelStatistics.Count
                                : 0,
                        LevelObjectiveMean =
                            collectCoolingLevelStatistics
                                ? coolingLevelStatistics.Mean
                                : 0.0,
                        LevelObjectiveVariance =
                            collectCoolingLevelStatistics
                                ? coolingLevelStatistics.PopulationVariance
                                : 0.0
                    };

                double nextTemperature =
                    coolingSchedule.GetNextTemperature(
                        in coolingContext);

                if (!double.IsFinite(nextTemperature) ||
                    nextTemperature < 0.0)
                {
                    throw new InvalidOperationException(
                        "The simulated-annealing cooling schedule returned a non-finite or negative temperature.");
                }

                temperature =
                    Math.Max(
                        nextTemperature,
                        parameters.MinimumTemperature);

                transitionsInLevel = 0;
                coolingLevelStatistics.Reset();
                levelAttemptStart =
                    statistics.Attempts;
                levelAcceptedStart =
                    statistics.Accepted;
            }

            state =
                CreateState(
                    currentObjective,
                    context.State.BestFitness,
                    temperature,
                    temperatureLevel,
                    transitionsInLevel,
                    in statistics,
                    consecutiveSamplingFailures);

            context.CompleteIteration(
                currentObjective,
                state);

            if (parameters.StopAtMinimumTemperature &&
                temperature <=
                    parameters.MinimumTemperature)
            {
                return
                    context.Complete(
                        StoppingDecision.Stop(
                            "MinimumTemperature",
                            "The simulated-annealing temperature reached the configured minimum."),
                        state);
            }

            stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return
                    context.Complete(
                        stop,
                        state);
            }
        }
    }

    private static void RegisterCandidateEvaluation(
        OptimizationContext<TSolution> context,
        ISolutionCloner<TSolution> solutionCloner,
        in TSolution currentSolution,
        in TrajectoryStepResult step)
    {
        if (context.WouldImprove(
                step.CandidateObjective))
        {
            if (!step.Accepted)
            {
                throw new InvalidOperationException(
                    "A candidate that improves the global best must be accepted by the Metropolis rule.");
            }

            TSolution ownedSnapshot =
                solutionCloner.Clone(
                    currentSolution);

            context.RegisterOwnedExternalEvaluationSnapshot(
                ownedSnapshot,
                step.CandidateObjective,
                step);
        }
        else
        {
            context.RegisterExternalEvaluation(
                step.CandidateObjective,
                step);
        }
    }

    private static SimulatedAnnealingState CreateState(
        double currentObjective,
        double bestObjective,
        double temperature,
        long temperatureLevel,
        int transitionsInLevel,
        in TrajectoryStatisticsAccumulator statistics,
        int consecutiveSamplingFailures) =>
        new(
            currentObjective,
            bestObjective,
            temperature,
            temperatureLevel,
            transitionsInLevel,
            statistics.Attempts,
            statistics.Accepted,
            statistics.Improving,
            statistics.Equal,
            statistics.Worsening,
            statistics.DeltaEvaluations,
            statistics.FullEvaluations,
            consecutiveSamplingFailures);
}