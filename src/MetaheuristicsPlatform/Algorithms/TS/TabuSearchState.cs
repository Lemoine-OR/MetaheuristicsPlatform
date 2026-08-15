namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Algorithm-specific counters exposed through the common OptimizationContext state.
/// </summary>
public readonly struct TabuSearchState
{
    public TabuSearchState(
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
        long improvingMoves,
        long equalMoves,
        long worseningMoves,
        int activeTabuAttributes,
        int lastTabuTenure)
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
        ImprovingMoves = improvingMoves;
        EqualMoves = equalMoves;
        WorseningMoves = worseningMoves;
        ActiveTabuAttributes = activeTabuAttributes;
        LastTabuTenure = lastTabuTenure;
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
    public long ImprovingMoves { get; }
    public long EqualMoves { get; }
    public long WorseningMoves { get; }
    public int ActiveTabuAttributes { get; }
    public int LastTabuTenure { get; }
}
