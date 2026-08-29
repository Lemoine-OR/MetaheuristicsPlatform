using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.FuzzyAdaptiveLateAcceptanceHyperHeuristic;

public sealed class FuzzyAdaptiveLateAcceptanceHyperHeuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 400;
    public double LearningRate { get; init; } = 0.2;
    public double Exploration { get; init; } = 1.0;
    public int MinimumHistoryLength { get; init; } = 10;
    public int MaximumHistoryLength { get; init; } = 100;

    public void Validate()
    {
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));

        if (!double.IsFinite(LearningRate) ||
            LearningRate <= 0.0 ||
            LearningRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(LearningRate));

        if (!double.IsFinite(Exploration) ||
            Exploration < 0.0)
            throw new ArgumentOutOfRangeException(nameof(Exploration));

        if (MinimumHistoryLength <= 0) throw new ArgumentOutOfRangeException(nameof(MinimumHistoryLength));
        if (MaximumHistoryLength < MinimumHistoryLength) throw new ArgumentOutOfRangeException(nameof(MaximumHistoryLength));
    }
}
