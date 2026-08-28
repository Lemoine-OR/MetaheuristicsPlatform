using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.HyperHeuristics.IlsBanditHyperHeuristic;

public sealed class IlsBanditHyperHeuristicParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; init; } = 400;
    public double LearningRate { get; init; } = 0.2;
    public double Exploration { get; init; } = 1.0;
    public int SubsetSize { get; init; } = 4;
    public int PerturbationPeriod { get; init; } = 25;

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

        if (SubsetSize <= 0) throw new ArgumentOutOfRangeException(nameof(SubsetSize));
        if (PerturbationPeriod <= 0) throw new ArgumentOutOfRangeException(nameof(PerturbationPeriod));
    }
}
