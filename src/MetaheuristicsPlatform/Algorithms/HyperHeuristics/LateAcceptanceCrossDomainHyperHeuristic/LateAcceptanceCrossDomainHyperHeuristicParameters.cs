using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.LateAcceptanceCrossDomainHyperHeuristic;

public sealed class LateAcceptanceCrossDomainHyperHeuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 400;
    public double LearningRate { get; init; } = 0.2;
    public double Exploration { get; init; } = 1.0;
    public int HistoryLength { get; init; } = 50;
    public double ChoiceRecencyWeight { get; init; } = 0.2;

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

        if (HistoryLength <= 0) throw new ArgumentOutOfRangeException(nameof(HistoryLength));
        if (!double.IsFinite(ChoiceRecencyWeight) || ChoiceRecencyWeight < 0.0) throw new ArgumentOutOfRangeException(nameof(ChoiceRecencyWeight));
    }
}
