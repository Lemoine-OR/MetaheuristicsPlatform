namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Runtime information supplied to a tabu-tenure policy after a move is selected.
/// </summary>
public readonly struct TabuTenureContext
{
    public TabuTenureContext(
        long iteration,
        double previousObjective,
        double selectedObjective,
        double bestObjective,
        long movesExamined,
        long tabuRejections,
        long aspirationOverrides)
    {
        Iteration = iteration;
        PreviousObjective = previousObjective;
        SelectedObjective = selectedObjective;
        BestObjective = bestObjective;
        MovesExamined = movesExamined;
        TabuRejections = tabuRejections;
        AspirationOverrides = aspirationOverrides;
    }

    public long Iteration { get; }
    public double PreviousObjective { get; }
    public double SelectedObjective { get; }
    public double BestObjective { get; }
    public long MovesExamined { get; }
    public long TabuRejections { get; }
    public long AspirationOverrides { get; }
}
