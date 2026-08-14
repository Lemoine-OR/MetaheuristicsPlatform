namespace MetaheuristicsPlatform.Evaluation.Instrumentation;

/// <summary>
/// Optional sink for evaluation-pipeline performance measurements.
/// </summary>
public interface IEvaluationPipelineMetricsSink
{
    void RecordEvaluation(
        in EvaluationPipelineMeasurement measurement);

    void RecordCacheHit();

    void RecordCacheMiss();
}