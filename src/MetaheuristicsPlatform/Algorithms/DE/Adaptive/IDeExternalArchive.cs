using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// External vector archive contract for JADE/SHADE-family mutation.
/// </summary>
public interface IDeExternalArchive
{
    int Count { get; }

    int Capacity { get; }

    int Dimension { get; }

    void Clear();

    void Add(
        ReadOnlySpan<double> vector,
        IRandomSource random);

    void CopyRandomTo(
        IRandomSource random,
        Span<double> destination);
}