namespace MetaheuristicsPlatform.Algorithms.CrossEntropy;

/// <summary>Lifecycle phase of the continuous Cross-Entropy Method.</summary>
public enum CrossEntropyPhase
{
    Sampling = 0,
    DistributionUpdate = 1,
    CompletedIteration = 2
}

/// <summary>Observable state of the continuous Cross-Entropy Method.</summary>
public readonly record struct ContinuousCrossEntropyState(
    int Iteration,
    CrossEntropyPhase Phase,
    int SampleCount,
    int EliteCount,
    double MeanSmoothing,
    double DynamicStandardDeviationSmoothing,
    double MinimumCoordinateStandardDeviation,
    double MaximumCoordinateStandardDeviation,
    double? IterationBestFitness);
