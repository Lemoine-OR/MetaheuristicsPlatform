using System.Diagnostics;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Core;

/// <summary>
/// Implements the common runtime lifecycle used by every metaheuristic.
/// Algorithms keep their specific search logic but delegate common accounting,
/// random-source ownership, best-so-far management, callbacks and stopping to this context.
/// </summary>
/// <typeparam name="TSolution">Solution representation.</typeparam>
public sealed class OptimizationContext<TSolution>
{
    private readonly MetaheuristicDescriptor _descriptor;
    private readonly IOptimizationProblem<TSolution> _problem;
    private readonly ISolutionCloner<TSolution> _solutionCloner;
    private readonly IStoppingCriterion _stoppingCriterion;
    private readonly OptimizationOptions _options;
    private readonly IOptimizationCallback<TSolution>? _callback;
    private readonly CancellationToken _cancellationToken;
    private readonly Stopwatch _stopwatch = new();

    private long _iteration;
    private long _evaluations;
    private long _lastImprovementIteration;
    private long _lastImprovementEvaluation;
    private long _improvementCount;
    private TimeSpan _lastImprovementElapsed;
    private bool _hasBestSolution;
    private bool _started;
    private bool _completed;
    private double _bestFitness;
    private TSolution? _bestSolution;
    private object? _algorithmState;

    /// <summary>Initializes a common runtime context.</summary>
    public OptimizationContext(
        MetaheuristicDescriptor descriptor,
        IOptimizationProblem<TSolution> problem,
        ISolutionCloner<TSolution> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<TSolution>? callback = null,
        CancellationToken cancellationToken = default)
    {
        _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _problem = problem ?? throw new ArgumentNullException(nameof(problem));
        _solutionCloner = solutionCloner ?? throw new ArgumentNullException(nameof(solutionCloner));
        _stoppingCriterion = stoppingCriterion ?? throw new ArgumentNullException(nameof(stoppingCriterion));
        _options = options ?? new OptimizationOptions();
        _options.Validate();
        _callback = callback;
        _cancellationToken = cancellationToken;
        _bestFitness = _problem.Sense.WorstValue();
        Random = _options.RandomSourceFactory.Create(_options.Seed)
            ?? throw new InvalidOperationException("The random-source factory returned null.");
    }

    /// <summary>Gets the optimization problem.</summary>
    public IOptimizationProblem<TSolution> Problem => _problem;

    /// <summary>Gets the common options.</summary>
    public OptimizationOptions Options => _options;

    /// <summary>Gets the deterministic random source owned by this optimization run.</summary>
    public IRandomSource Random { get; }

    /// <summary>Gets the current common state.</summary>
    public OptimizationState State => new(
        _iteration,
        _evaluations,
        _stopwatch.Elapsed,
        _hasBestSolution,
        _bestFitness,
        _lastImprovementIteration,
        _lastImprovementEvaluation,
        _improvementCount,
        _algorithmState,
        _lastImprovementElapsed);

    /// <summary>Starts the common lifecycle.</summary>
    public void Start(object? algorithmState = null)
    {
        if (_started)
        {
            throw new InvalidOperationException("Optimization context has already been started.");
        }

        _started = true;
        _algorithmState = algorithmState;
        _stopwatch.Start();

        Emit(
            OptimizationCallbackEvents.Started,
            OptimizationEventKind.Started,
            currentFitness: null,
            algorithmData: algorithmState);
    }

    /// <summary>
    /// Evaluates a solution, updates common counters and best-so-far state,
    /// then returns the objective value.
    /// </summary>
    public double Evaluate(TSolution solution, object? algorithmData = null)
    {
        EnsureRunning();

        double fitness = _problem.Evaluate(solution);
        _evaluations++;

        Emit(
            OptimizationCallbackEvents.EvaluationCompleted,
            OptimizationEventKind.EvaluationCompleted,
            fitness,
            algorithmData);

        if (!_hasBestSolution || _problem.Sense.IsBetter(fitness, _bestFitness))
        {
            _bestFitness = fitness;
            _bestSolution = _solutionCloner.Clone(solution);
            _hasBestSolution = true;
            _lastImprovementIteration = _iteration;
            _lastImprovementEvaluation = _evaluations;
            _lastImprovementElapsed = _stopwatch.Elapsed;
            _improvementCount++;

            Emit(
                OptimizationCallbackEvents.BestImproved,
                OptimizationEventKind.BestImproved,
                fitness,
                algorithmData);
        }

        return fitness;
    }

    /// <summary>
    /// Returns whether <paramref name="fitness"/> would strictly improve the
    /// current best-so-far value.
    /// </summary>
    public bool WouldImprove(double fitness)
    {
        EnsureRunning();

        return !_hasBestSolution ||
            _problem.Sense.IsBetter(
                fitness,
                _bestFitness);
    }

    /// <summary>
    /// Registers a fitness value evaluated externally, without supplying a
    /// candidate solution. This overload is valid only when the value cannot
    /// improve the current best; it therefore avoids creating a candidate
    /// snapshot for non-improving batched evaluations.
    /// </summary>
    public void RegisterExternalEvaluation(
        double fitness,
        object? algorithmData = null)
    {
        EnsureRunning();

        if (WouldImprove(fitness))
        {
            throw new InvalidOperationException(
                "An improving external evaluation must provide its candidate solution.");
        }

        _evaluations++;

        Emit(
            OptimizationCallbackEvents.EvaluationCompleted,
            OptimizationEventKind.EvaluationCompleted,
            fitness,
            algorithmData);
    }

    /// <summary>
    /// Registers a candidate and its externally computed fitness.
    /// The candidate is cloned only when it strictly improves the best-so-far.
    /// </summary>
    public void RegisterExternalEvaluation(
        TSolution solution,
        double fitness,
        object? algorithmData = null)
    {
        EnsureRunning();

        _evaluations++;

        Emit(
            OptimizationCallbackEvents.EvaluationCompleted,
            OptimizationEventKind.EvaluationCompleted,
            fitness,
            algorithmData);

        if (!_hasBestSolution ||
            _problem.Sense.IsBetter(
                fitness,
                _bestFitness))
        {
            _bestFitness = fitness;
            _bestSolution =
                _solutionCloner.Clone(solution);
            _hasBestSolution = true;
            _lastImprovementIteration =
                _iteration;
            _lastImprovementEvaluation =
                _evaluations;
            _lastImprovementElapsed =
                _stopwatch.Elapsed;
            _improvementCount++;

            Emit(
                OptimizationCallbackEvents.BestImproved,
                OptimizationEventKind.BestImproved,
                fitness,
                algorithmData);
        }
    }
    /// <summary>
    /// Registers an externally evaluated candidate whose solution object is already
    /// an owned immutable snapshot for this optimization context. No second clone
    /// is performed.
    /// </summary>
    public void RegisterOwnedExternalEvaluationSnapshot(
        TSolution ownedSnapshot,
        double fitness,
        object? algorithmData = null)
    {
        EnsureRunning();

        _evaluations++;

        Emit(
            OptimizationCallbackEvents.EvaluationCompleted,
            OptimizationEventKind.EvaluationCompleted,
            fitness,
            algorithmData);

        if (!_hasBestSolution ||
            _problem.Sense.IsBetter(
                fitness,
                _bestFitness))
        {
            _bestFitness = fitness;
            _bestSolution = ownedSnapshot;
            _hasBestSolution = true;
            _lastImprovementIteration =
                _iteration;
            _lastImprovementEvaluation =
                _evaluations;
            _lastImprovementElapsed =
                _stopwatch.Elapsed;
            _improvementCount++;

            Emit(
                OptimizationCallbackEvents.BestImproved,
                OptimizationEventKind.BestImproved,
                fitness,
                algorithmData);
        }
    }
    /// <summary>
    /// Registers an externally computed candidate objective as a probe evaluation without
    /// promoting the candidate to best-so-far. This supports best-candidate selection
    /// algorithms that evaluate several neighbors before deciding which solution is visited.
    /// Returns the common evaluation index assigned to the probe.
    /// </summary>
    public long RegisterExternalProbeEvaluation(
        double fitness,
        object? algorithmData = null)
    {
        EnsureRunning();

        _evaluations++;

        Emit(
            OptimizationCallbackEvents.EvaluationCompleted,
            OptimizationEventKind.EvaluationCompleted,
            fitness,
            algorithmData);

        return _evaluations;
    }

    /// <summary>
    /// Promotes a previously registered probe evaluation to best-so-far without counting a
    /// second objective evaluation. The supplied solution must already be an owned snapshot.
    /// </summary>
    public void PromoteOwnedExternalProbeSnapshot(
        TSolution ownedSnapshot,
        double fitness,
        long evaluationIndex,
        object? algorithmData = null)
    {
        EnsureRunning();

        if (evaluationIndex <= 0 || evaluationIndex > _evaluations)
        {
            throw new ArgumentOutOfRangeException(nameof(evaluationIndex));
        }

        if (_hasBestSolution &&
            !_problem.Sense.IsBetter(fitness, _bestFitness))
        {
            return;
        }

        _bestFitness = fitness;
        _bestSolution = ownedSnapshot;
        _hasBestSolution = true;
        _lastImprovementIteration = _iteration;
        _lastImprovementEvaluation = evaluationIndex;
        _lastImprovementElapsed = _stopwatch.Elapsed;
        _improvementCount++;

        Emit(
            OptimizationCallbackEvents.BestImproved,
            OptimizationEventKind.BestImproved,
            fitness,
            algorithmData);
    }
    /// <summary>Marks one algorithm iteration as completed.</summary>
    public void CompleteIteration(double? currentFitness = null, object? algorithmState = null)
    {
        EnsureRunning();

        _iteration++;
        _algorithmState = algorithmState;

        if ((_iteration % _options.IterationCallbackFrequency) == 0)
        {
            Emit(
                OptimizationCallbackEvents.IterationCompleted,
                OptimizationEventKind.IterationCompleted,
                currentFitness,
                algorithmState);
        }
    }

    /// <summary>
    /// Evaluates cancellation and the configured stopping criterion.
    /// Algorithm-specific criteria use the same interface and may inspect State.AlgorithmState.
    /// </summary>
    public StoppingDecision EvaluateStopping(object? algorithmState = null)
    {
        EnsureRunning();
        _algorithmState = algorithmState;

        if (_cancellationToken.IsCancellationRequested)
        {
            return StoppingDecision.Stop("Cancellation", "Cancellation was requested.");
        }

        OptimizationState state = State;
        return _stoppingCriterion.Evaluate(in state, _problem.Sense);
    }

    /// <summary>Completes the lifecycle and creates the common result.</summary>
    public OptimizationResult<TSolution> Complete(StoppingDecision stopDecision, object? algorithmData = null)
    {
        EnsureRunning();

        if (!stopDecision.ShouldStop)
        {
            throw new ArgumentException("A completed optimization requires a stopping decision.", nameof(stopDecision));
        }

        if (!_hasBestSolution || _bestSolution is null)
        {
            throw new InvalidOperationException("The optimization completed without evaluating a valid best solution.");
        }

        _completed = true;
        _stopwatch.Stop();

        Emit(
            OptimizationCallbackEvents.Completed,
            OptimizationEventKind.Completed,
            _bestFitness,
            algorithmData);

        return new OptimizationResult<TSolution>
        {
            Algorithm = _descriptor,
            BestSolution = _bestSolution,
            BestFitness = _bestFitness,
            StopDecision = stopDecision,
            Seed = _options.Seed,
            RandomSourceId = _options.RandomSourceFactory.Id,
            Statistics = new OptimizationRunStatistics(
                _iteration,
                _evaluations,
                _improvementCount,
                _stopwatch.Elapsed,
                _lastImprovementIteration,
                _lastImprovementEvaluation,
                _lastImprovementElapsed)
        };
    }

    private void Emit(
        OptimizationCallbackEvents requiredFlag,
        OptimizationEventKind kind,
        double? currentFitness,
        object? algorithmData)
    {
        if (_callback is null ||
            (_options.CallbackEvents & requiredFlag) == 0 ||
            (_callback.Events & requiredFlag) == 0)
        {
            return;
        }

        OptimizationEvent<TSolution> optimizationEvent = new(
            kind,
            State,
            _hasBestSolution ? _bestSolution : default,
            currentFitness,
            algorithmData);

        _callback.OnEvent(in optimizationEvent);
    }

    private void EnsureRunning()
    {
        if (!_started)
        {
            throw new InvalidOperationException("Optimization context has not been started.");
        }

        if (_completed)
        {
            throw new InvalidOperationException("Optimization context has already been completed.");
        }
    }
}