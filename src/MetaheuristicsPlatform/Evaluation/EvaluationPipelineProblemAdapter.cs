using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Evaluation;

/// <summary>
/// Adapts a non-Lamarckian evaluation pipeline to the standard optimization-problem API.
/// </summary>
public sealed class EvaluationPipelineProblemAdapter<TCandidate, TSolution> :
    IOptimizationProblem<TCandidate>,
    IEvaluationCharacteristicsProvider
{
    private readonly IEvaluationPipeline<TCandidate, TSolution> _pipeline;

    public EvaluationPipelineProblemAdapter(
        OptimizationSense sense,
        IEvaluationPipeline<TCandidate, TSolution> pipeline)
    {
        _pipeline =
            pipeline ??
            throw new ArgumentNullException(
                nameof(pipeline));

        if (pipeline.FeedbackMode ==
            ImprovementFeedbackMode.Lamarckian)
        {
            throw new ArgumentException(
                "The standard IOptimizationProblem<T> API cannot expose Lamarckian " +
                "candidate replacement. Use the evaluation pipeline directly or an " +
                "algorithm that consumes IEvaluationPipeline.",
                nameof(pipeline));
        }

        Sense = sense;
    }

    public OptimizationSense Sense { get; }

    public EvaluationCharacteristics
        EvaluationCharacteristics =>
        _pipeline.EvaluationCharacteristics;

    public double Evaluate(TCandidate solution)
    {
        TCandidate candidate = solution;

        return _pipeline
            .Evaluate(
                ref candidate)
            .Fitness;
    }
}