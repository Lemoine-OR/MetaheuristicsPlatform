namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Result of observing one configuration signature in repetition memory.
/// </summary>
public readonly struct TabuSearchRepetitionObservation
{
    public TabuSearchRepetitionObservation(
        bool isRepetition,
        long previousIteration,
        long cycleLength,
        long visitCount)
    {
        IsRepetition = isRepetition;
        PreviousIteration = previousIteration;
        CycleLength = cycleLength;
        VisitCount = visitCount;
    }

    public bool IsRepetition { get; }
    public long PreviousIteration { get; }
    public long CycleLength { get; }
    public long VisitCount { get; }
}
