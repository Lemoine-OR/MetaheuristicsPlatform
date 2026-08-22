using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>Canonical Ant Colony System parameters.</summary>
public sealed class AntColonySystemParameters : IMetaheuristicParameters
{
    public int AntCount { get; init; } = 10;
    public int MaximumIterations { get; init; } = 100;
    public double Beta { get; init; } = 2.0;
    public double ExploitationProbability { get; init; } = 0.9;
    public double GlobalEvaporationRate { get; init; } = 0.1;
    public double LocalUpdateRate { get; init; } = 0.1;
    public double InitialPheromone { get; init; } = 0.1;
    public int MaximumConstructionSteps { get; init; } = 100000;

    public void Validate()
    {
        if (AntCount <= 0) throw new ArgumentOutOfRangeException(nameof(AntCount));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(Beta) || Beta < 0.0) throw new ArgumentOutOfRangeException(nameof(Beta));
        if (!double.IsFinite(ExploitationProbability) ||
            ExploitationProbability < 0.0 ||
            ExploitationProbability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(ExploitationProbability));
        if (!double.IsFinite(GlobalEvaporationRate) ||
            GlobalEvaporationRate <= 0.0 ||
            GlobalEvaporationRate >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(GlobalEvaporationRate));
        if (!double.IsFinite(LocalUpdateRate) ||
            LocalUpdateRate <= 0.0 ||
            LocalUpdateRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(LocalUpdateRate));
        if (!double.IsFinite(InitialPheromone) || InitialPheromone <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialPheromone));
        if (MaximumConstructionSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumConstructionSteps));
    }
}

/// <summary>Best source used by the MMAS selective global update.</summary>
public enum MaxMinAntSystemBestSource
{
    IterationBest = 0,
    BestSoFar = 1
}

/// <summary>Canonical MAX-MIN Ant System parameters.</summary>
public sealed class MaxMinAntSystemParameters : IMetaheuristicParameters
{
    public int AntCount { get; init; } = 20;
    public int MaximumIterations { get; init; } = 100;
    public double Alpha { get; init; } = 1.0;
    public double Beta { get; init; } = 2.0;
    public double EvaporationRate { get; init; } = 0.2;
    public double InitialPheromone { get; init; } = 1.0;
    public double MinimumPheromone { get; init; } = 0.01;
    public double MaximumPheromone { get; init; } = 1.0;
    public MaxMinAntSystemBestSource BestSource { get; init; } =
        MaxMinAntSystemBestSource.BestSoFar;
    public int RestartAfterNonImprovingIterations { get; init; } = 0;
    public int MaximumConstructionSteps { get; init; } = 100000;

    public void Validate()
    {
        if (AntCount <= 0) throw new ArgumentOutOfRangeException(nameof(AntCount));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(Alpha) || Alpha < 0.0) throw new ArgumentOutOfRangeException(nameof(Alpha));
        if (!double.IsFinite(Beta) || Beta < 0.0) throw new ArgumentOutOfRangeException(nameof(Beta));
        if (!double.IsFinite(EvaporationRate) ||
            EvaporationRate <= 0.0 ||
            EvaporationRate >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(EvaporationRate));
        if (!double.IsFinite(MinimumPheromone) || MinimumPheromone <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(MinimumPheromone));
        if (!double.IsFinite(MaximumPheromone) ||
            MaximumPheromone < MinimumPheromone)
            throw new ArgumentOutOfRangeException(nameof(MaximumPheromone));
        if (!double.IsFinite(InitialPheromone) ||
            InitialPheromone < MinimumPheromone ||
            InitialPheromone > MaximumPheromone)
            throw new ArgumentOutOfRangeException(nameof(InitialPheromone));
        if (!Enum.IsDefined(BestSource))
            throw new ArgumentOutOfRangeException(nameof(BestSource));
        if (RestartAfterNonImprovingIterations < 0)
            throw new ArgumentOutOfRangeException(nameof(RestartAfterNonImprovingIterations));
        if (MaximumConstructionSteps <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumConstructionSteps));
    }
}
