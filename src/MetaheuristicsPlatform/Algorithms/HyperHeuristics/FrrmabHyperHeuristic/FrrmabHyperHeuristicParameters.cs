using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.FrrmabHyperHeuristic;

public sealed class FrrmabHyperHeuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 400;
    public double LearningRate { get; init; } = 0.2;
    public double Exploration { get; init; } = 1.0;
    public int SlidingWindow { get; init; } = 50;
    public double Decay { get; init; } = 0.8;

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

        if (SlidingWindow <= 0) throw new ArgumentOutOfRangeException(nameof(SlidingWindow));
        if (!double.IsFinite(Decay) || Decay <= 0.0 || Decay > 1.0) throw new ArgumentOutOfRangeException(nameof(Decay));
    }
}
