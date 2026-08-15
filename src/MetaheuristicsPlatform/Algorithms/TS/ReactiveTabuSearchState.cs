namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Reactive Tabu Search counters exposed through OptimizationContext.
/// </summary>
public readonly struct ReactiveTabuSearchState
{
    public ReactiveTabuSearchState(
        double currentObjective,
        double bestObjective,
        long movesExamined,
        long applicableMoves,
        long candidateEvaluations,
        long deltaEvaluations,
        long fullEvaluations,
        long tabuRejections,
        long aspirationOverrides,
        long selectedMoves,
        int activeTabuAttributes,
        int currentTabuTenure,
        int trackedConfigurations,
        long repeatedConfigurations,
        long lastCycleLength,
        double movingAverageCycleLength,
        long tenureChanges,
        int frequencyTrackedAttributes,
        long intensificationRestarts,
        long diversificationPhases,
        long diversificationMoves,
        int diversificationMovesRemaining,
        long iterationsSinceBestImprovement)
    {
        CurrentObjective = currentObjective;
        BestObjective = bestObjective;
        MovesExamined = movesExamined;
        ApplicableMoves = applicableMoves;
        CandidateEvaluations = candidateEvaluations;
        DeltaEvaluations = deltaEvaluations;
        FullEvaluations = fullEvaluations;
        TabuRejections = tabuRejections;
        AspirationOverrides = aspirationOverrides;
        SelectedMoves = selectedMoves;
        ActiveTabuAttributes = activeTabuAttributes;
        CurrentTabuTenure = currentTabuTenure;
        TrackedConfigurations = trackedConfigurations;
        RepeatedConfigurations = repeatedConfigurations;
        LastCycleLength = lastCycleLength;
        MovingAverageCycleLength = movingAverageCycleLength;
        TenureChanges = tenureChanges;
        FrequencyTrackedAttributes = frequencyTrackedAttributes;
        IntensificationRestarts = intensificationRestarts;
        DiversificationPhases = diversificationPhases;
        DiversificationMoves = diversificationMoves;
        DiversificationMovesRemaining = diversificationMovesRemaining;
        IterationsSinceBestImprovement = iterationsSinceBestImprovement;
    }

    public double CurrentObjective { get; }
    public double BestObjective { get; }
    public long MovesExamined { get; }
    public long ApplicableMoves { get; }
    public long CandidateEvaluations { get; }
    public long DeltaEvaluations { get; }
    public long FullEvaluations { get; }
    public long TabuRejections { get; }
    public long AspirationOverrides { get; }
    public long SelectedMoves { get; }
    public int ActiveTabuAttributes { get; }
    public int CurrentTabuTenure { get; }
    public int TrackedConfigurations { get; }
    public long RepeatedConfigurations { get; }
    public long LastCycleLength { get; }
    public double MovingAverageCycleLength { get; }
    public long TenureChanges { get; }
    public int FrequencyTrackedAttributes { get; }
    public long IntensificationRestarts { get; }
    public long DiversificationPhases { get; }
    public long DiversificationMoves { get; }
    public int DiversificationMovesRemaining { get; }
    public long IterationsSinceBestImprovement { get; }
}
