using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Evaluates a candidate through a problem-specific decode/repair/improve/evaluate pipeline.
/// </summary>
public interface IEvaluationPipeline<TCandidate, TSolution> :
    IEvaluationCharacteristicsProvider
{
    ImprovementFeedbackMode FeedbackMode { get; }

    EvaluationPipelineResult<TSolution> Evaluate(
        ref TCandidate candidate,
        CancellationToken cancellationToken = default);
}