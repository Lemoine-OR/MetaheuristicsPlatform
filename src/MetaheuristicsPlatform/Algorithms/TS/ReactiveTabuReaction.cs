namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// One feedback reaction produced by the reactive tenure controller.
/// </summary>
public readonly struct ReactiveTabuReaction
{
    public ReactiveTabuReaction(
        int tabuTenure,
        bool tenureChanged,
        bool diversificationRequested,
        int diversificationMoves)
    {
        TabuTenure = tabuTenure;
        TenureChanged = tenureChanged;
        DiversificationRequested = diversificationRequested;
        DiversificationMoves = diversificationMoves;
    }

    public int TabuTenure { get; }
    public bool TenureChanged { get; }
    public bool DiversificationRequested { get; }
    public int DiversificationMoves { get; }
}
