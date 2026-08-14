using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.SearchSpaces.Continuous;

/// <summary>
/// High-performance bounded continuous problem supporting allocation-free span evaluation.
/// </summary>
public interface ISpanContinuousOptimizationProblem :
    IOptimizationProblem<double[]>,
    IEvaluationCharacteristicsProvider
{
    IBoundedContinuousSearchSpace SearchSpace { get; }

    /// <summary>
    /// Compatibility convenience property.
    /// </summary>
    bool SupportsParallelEvaluation { get; }

    double Evaluate(ReadOnlySpan<double> solution);
}