using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.SearchSpaces.Continuous;

/// <summary>
/// Standard bounded continuous optimization problem backed by a span-based objective.
/// </summary>
public sealed class ContinuousOptimizationProblem :
    ISpanContinuousOptimizationProblem
{
    private readonly ContinuousObjective _objective;

    public ContinuousOptimizationProblem(
        IBoundedContinuousSearchSpace searchSpace,
        OptimizationSense sense,
        ContinuousObjective objective,
        bool supportsParallelEvaluation = false,
        EvaluationCostHint evaluationCostHint =
            EvaluationCostHint.Light,
        EvaluationVariabilityHint evaluationVariabilityHint =
            EvaluationVariabilityHint.Uniform)
    {
        SearchSpace =
            searchSpace ??
            throw new ArgumentNullException(
                nameof(searchSpace));

        Sense = sense;

        _objective =
            objective ??
            throw new ArgumentNullException(
                nameof(objective));

        EvaluationCharacteristics =
            new EvaluationCharacteristics(
                supportsParallelEvaluation,
                evaluationCostHint,
                evaluationVariabilityHint);
    }

    public IBoundedContinuousSearchSpace SearchSpace { get; }

    public OptimizationSense Sense { get; }

    public EvaluationCharacteristics
        EvaluationCharacteristics { get; }

    public bool SupportsParallelEvaluation =>
        EvaluationCharacteristics
            .SupportsParallelEvaluation;

    public double Evaluate(double[] solution)
    {
        ArgumentNullException.ThrowIfNull(solution);
        return Evaluate(solution.AsSpan());
    }

    public double Evaluate(
        ReadOnlySpan<double> solution)
    {
        if (solution.Length !=
            SearchSpace.Dimension)
        {
            throw new ArgumentException(
                $"Expected solution dimension {SearchSpace.Dimension}, " +
                $"received {solution.Length}.",
                nameof(solution));
        }

        return _objective(solution);
    }
}