namespace MetaheuristicsPlatform.Algorithms.Firefly;

/// <summary>Lifecycle phase of the Firefly Algorithm.</summary>
public enum FireflyPhase
{
    Initialization = 0,
    AttractionMoves = 1,
    CompletedIteration = 2
}

/// <summary>Observable state of the Firefly Algorithm.</summary>
public readonly record struct FireflyState(
    int Iteration,
    FireflyPhase Phase,
    int PopulationSize,
    int TotalMoves,
    int IterationMoves,
    double BaseAttractiveness,
    double LightAbsorptionCoefficient,
    double RandomizationAmplitude,
    double? IterationBestFitness);
