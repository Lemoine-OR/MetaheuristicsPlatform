using MetaheuristicsPlatform.Evaluation.Instrumentation;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Generic composable candidate evaluation pipeline.
/// </summary>
public sealed class EvaluationPipeline<TCandidate, TSolution> :
    IEvaluationPipeline<TCandidate, TSolution>
{
    private readonly ISolutionDecoder<TCandidate, TSolution> _decoder;
    private readonly ISolutionEvaluator<TSolution> _evaluator;
    private readonly ISolutionRepair<TSolution>? _repair;
    private readonly ISolutionImprover<TSolution>? _improver;
    private readonly ILamarckianFeedback<TCandidate, TSolution>? _feedback;
    private readonly IEvaluationPipelineMetricsSink? _metricsSink;

    public EvaluationPipeline(
        ISolutionDecoder<TCandidate, TSolution> decoder,
        ISolutionEvaluator<TSolution> evaluator,
        EvaluationCharacteristics evaluationCharacteristics,
        ISolutionRepair<TSolution>? repair = null,
        ISolutionImprover<TSolution>? improver = null,
        ImprovementFeedbackMode feedbackMode =
            ImprovementFeedbackMode.None,
        ILamarckianFeedback<TCandidate, TSolution>? feedback = null,
        IEvaluationPipelineMetricsSink? metricsSink = null)
    {
        _decoder =
            decoder ??
            throw new ArgumentNullException(
                nameof(decoder));

        _evaluator =
            evaluator ??
            throw new ArgumentNullException(
                nameof(evaluator));

        _repair = repair;
        _improver = improver;
        _feedback = feedback;
        _metricsSink = metricsSink;

        if (feedbackMode != ImprovementFeedbackMode.None &&
            improver is null)
        {
            throw new ArgumentException(
                "Baldwinian or Lamarckian feedback requires an improvement stage.",
                nameof(improver));
        }

        if (feedbackMode == ImprovementFeedbackMode.Lamarckian &&
            feedback is null)
        {
            throw new ArgumentException(
                "Lamarckian feedback requires an ILamarckianFeedback implementation.",
                nameof(feedback));
        }

        FeedbackMode = feedbackMode;
        EvaluationCharacteristics = evaluationCharacteristics;
    }

    public ImprovementFeedbackMode FeedbackMode { get; }

    public EvaluationCharacteristics
        EvaluationCharacteristics { get; }

    public bool InstrumentationEnabled =>
        _metricsSink is not null;

    public EvaluationPipelineResult<TSolution> Evaluate(
        ref TCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        if (_metricsSink is null)
        {
            return EvaluateUninstrumented(
                ref candidate,
                cancellationToken);
        }

        return EvaluateInstrumented(
            ref candidate,
            cancellationToken);
    }

    private EvaluationPipelineResult<TSolution>
        EvaluateUninstrumented(
            ref TCandidate candidate,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TSolution solution =
            _decoder.Decode(
                candidate,
                cancellationToken);

        bool repaired = false;

        if (_repair is not null)
        {
            repaired =
                _repair.Repair(
                    ref solution,
                    cancellationToken);
        }

        bool improved = false;

        if (FeedbackMode !=
            ImprovementFeedbackMode.None)
        {
            improved =
                _improver!.Improve(
                    ref solution,
                    cancellationToken);
        }

        double fitness =
            _evaluator.Evaluate(
                solution,
                cancellationToken);

        bool feedbackApplied = false;

        if (FeedbackMode ==
            ImprovementFeedbackMode.Lamarckian)
        {
            _feedback!.Apply(
                solution,
                ref candidate,
                cancellationToken);

            feedbackApplied = true;
        }

        return new EvaluationPipelineResult<TSolution>(
            fitness,
            solution,
            repaired,
            improved,
            feedbackApplied);
    }

    private EvaluationPipelineResult<TSolution>
        EvaluateInstrumented(
            ref TCandidate candidate,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        long totalStart =
            System.Diagnostics.Stopwatch.GetTimestamp();

        long stageStart =
            System.Diagnostics.Stopwatch.GetTimestamp();

        TSolution solution =
            _decoder.Decode(
                candidate,
                cancellationToken);

        long decodeTicks =
            System.Diagnostics.Stopwatch.GetTimestamp() -
            stageStart;

        bool repaired = false;
        long repairTicks = 0;

        if (_repair is not null)
        {
            stageStart =
                System.Diagnostics.Stopwatch.GetTimestamp();

            repaired =
                _repair.Repair(
                    ref solution,
                    cancellationToken);

            repairTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                stageStart;
        }

        bool improved = false;
        long improveTicks = 0;

        if (FeedbackMode !=
            ImprovementFeedbackMode.None)
        {
            stageStart =
                System.Diagnostics.Stopwatch.GetTimestamp();

            improved =
                _improver!.Improve(
                    ref solution,
                    cancellationToken);

            improveTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                stageStart;
        }

        stageStart =
            System.Diagnostics.Stopwatch.GetTimestamp();

        double fitness =
            _evaluator.Evaluate(
                solution,
                cancellationToken);

        long evaluateTicks =
            System.Diagnostics.Stopwatch.GetTimestamp() -
            stageStart;

        bool feedbackApplied = false;
        long feedbackTicks = 0;

        if (FeedbackMode ==
            ImprovementFeedbackMode.Lamarckian)
        {
            stageStart =
                System.Diagnostics.Stopwatch.GetTimestamp();

            _feedback!.Apply(
                solution,
                ref candidate,
                cancellationToken);

            feedbackTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                stageStart;

            feedbackApplied = true;
        }

        long totalTicks =
            System.Diagnostics.Stopwatch.GetTimestamp() -
            totalStart;

        var measurement =
            new EvaluationPipelineMeasurement(
                decodeTicks,
                repairTicks,
                improveTicks,
                evaluateTicks,
                feedbackTicks,
                totalTicks,
                repaired,
                improved,
                feedbackApplied);

        _metricsSink!.RecordEvaluation(
            in measurement);

        return new EvaluationPipelineResult<TSolution>(
            fitness,
            solution,
            repaired,
            improved,
            feedbackApplied);
    }
}