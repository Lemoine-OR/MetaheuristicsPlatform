using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Batch execution helper for independent candidates.
/// </summary>
public static class EvaluationPipelineBatchExecutor
{
    public static void Evaluate<TCandidate, TSolution>(
        TCandidate[] candidates,
        double[] fitness,
        int representationDimension,
        IEvaluationPipeline<TCandidate, TSolution> pipeline,
        EvaluationExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(fitness);
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(executionOptions);

        if (candidates.Length == 0)
        {
            throw new ArgumentException(
                "Candidate batch cannot be empty.",
                nameof(candidates));
        }

        if (fitness.Length != candidates.Length)
        {
            throw new ArgumentException(
                "Fitness buffer length must equal candidate count.",
                nameof(fitness));
        }

        EvaluationExecutor.ForCandidates(
            candidates.Length,
            representationDimension,
            pipeline.EvaluationCharacteristics,
            executionOptions,
            (start, end) =>
            {
                for (int candidateIndex = start;
                     candidateIndex < end;
                     candidateIndex++)
                {
                    ref TCandidate candidate =
                        ref candidates[candidateIndex];

                    EvaluationPipelineResult<TSolution> result =
                        pipeline.Evaluate(
                            ref candidate,
                            cancellationToken);

                    fitness[candidateIndex] =
                        result.Fitness;
                }
            },
            cancellationToken);
    }
}