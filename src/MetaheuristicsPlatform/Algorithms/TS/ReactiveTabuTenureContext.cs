namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Feedback supplied to a reactive tabu-tenure controller after a visited configuration
/// has been recorded.
/// </summary>
public readonly struct ReactiveTabuTenureContext
{
    public ReactiveTabuTenureContext(
        long iteration,
        in TabuSearchRepetitionObservation repetition,
        double currentObjective,
        double bestObjective)
    {
        Iteration = iteration;
        Repetition = repetition;
        CurrentObjective = currentObjective;
        BestObjective = bestObjective;
    }

    public long Iteration { get; }
    public TabuSearchRepetitionObservation Repetition { get; }
    public double CurrentObjective { get; }
    public double BestObjective { get; }
}
