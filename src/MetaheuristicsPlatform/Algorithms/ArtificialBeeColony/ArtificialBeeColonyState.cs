namespace MetaheuristicsPlatform.Algorithms.ArtificialBeeColony;

/// <summary>Canonical ABC lifecycle phase.</summary>
public enum ArtificialBeeColonyPhase
{
    Initialization = 0,
    EmployedBees = 1,
    OnlookerBees = 2,
    Scout = 3,
    CompletedCycle = 4
}

/// <summary>Observable state of the Artificial Bee Colony optimizer.</summary>
public sealed record ArtificialBeeColonyState(
    int Cycle,
    ArtificialBeeColonyPhase Phase,
    int FoodSourceCount,
    int AbandonmentLimit,
    int ScoutReinitializations,
    double? CycleBestFitness);
