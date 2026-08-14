using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Flat deterministic external archive.
///
/// When full, adding a vector is equivalent to appending it to Capacity+1
/// candidates and uniformly removing one candidate. If the removed candidate is
/// the newly added vector, the archive remains unchanged; otherwise the selected
/// existing slot is replaced.
/// </summary>
public sealed class DeExternalArchive :
    IDeExternalArchive
{
    private readonly double[] _storage;
    private int _count;

    public DeExternalArchive(
        int capacity,
        int dimension)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity));
        }

        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        Capacity = capacity;
        Dimension = dimension;

        _storage =
            GC.AllocateUninitializedArray<double>(
                checked(
                    capacity *
                    dimension));
    }

    public int Count =>
        _count;

    public int Capacity { get; }

    public int Dimension { get; }

    public void Clear() =>
        _count = 0;

    public void Add(
        ReadOnlySpan<double> vector,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (vector.Length != Dimension)
        {
            throw new ArgumentException(
                "Archive vector dimension mismatch.",
                nameof(vector));
        }

        if (_count < Capacity)
        {
            vector.CopyTo(
                GetSlot(_count));

            _count++;
            return;
        }

        int removedIndex =
            random.NextInt32(
                Capacity + 1);

        if (removedIndex == Capacity)
        {
            // The newly appended vector is the one removed.
            return;
        }

        vector.CopyTo(
            GetSlot(removedIndex));
    }

    /// <summary>
    /// Zero-allocation indexed read access used by archive-aware DE mutation.
    /// </summary>
    public ReadOnlySpan<double> GetVectorReadOnly(
        int index)
    {
        if ((uint)index >= (uint)_count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index));
        }

        return GetSlotReadOnly(index);
    }

    /// <summary>
    /// Randomly removes owned archive vectors until Count is at most maxCount.
    /// Storage capacity is unchanged.
    /// </summary>
    public void TrimToCount(
        int maxCount,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (maxCount < 0 ||
            maxCount > Capacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxCount));
        }

        while (_count > maxCount)
        {
            int removedIndex =
                random.NextInt32(
                    _count);

            int lastIndex =
                _count - 1;

            if (removedIndex != lastIndex)
            {
                GetSlotReadOnly(lastIndex)
                    .CopyTo(
                        GetSlot(removedIndex));
            }

            _count--;
        }
    }

    public void CopyRandomTo(
        IRandomSource random,
        Span<double> destination)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (_count == 0)
        {
            throw new InvalidOperationException(
                "Cannot sample an empty DE archive.");
        }

        if (destination.Length != Dimension)
        {
            throw new ArgumentException(
                "Destination dimension mismatch.",
                nameof(destination));
        }

        int index =
            random.NextInt32(
                _count);

        GetSlotReadOnly(index)
            .CopyTo(destination);
    }

    private Span<double> GetSlot(int index) =>
        _storage.AsSpan(
            index * Dimension,
            Dimension);

    private ReadOnlySpan<double> GetSlotReadOnly(int index) =>
        _storage.AsSpan(
            index * Dimension,
            Dimension);
}