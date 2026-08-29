using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record ReferenceRandomStreamEntry(
    string StreamName,
    int Ordinal,
    ulong DerivedSeed);

public sealed class ReferenceRandomStreamTrace
{
    private readonly List<ReferenceRandomStreamEntry> _entries = new();

    public ReferenceRandomStreamTrace(ulong masterSeed)
    {
        MasterSeed = masterSeed;
    }

    public ulong MasterSeed { get; }
    public IReadOnlyList<ReferenceRandomStreamEntry> Entries => _entries;

    public ulong DeriveSeed(
        string streamName,
        int ordinal = 0)
    {
        if (string.IsNullOrWhiteSpace(streamName))
            throw new ArgumentException(
                "Random stream name must not be empty.",
                nameof(streamName));
        if (ordinal < 0)
            throw new ArgumentOutOfRangeException(nameof(ordinal));
        if (_entries.Any(x => x.StreamName == streamName && x.Ordinal == ordinal))
            throw new InvalidOperationException(
                "The same named random stream ordinal cannot be registered twice.");

        byte[] nameBytes = Encoding.UTF8.GetBytes(streamName.Trim());
        byte[] payload = new byte[12 + nameBytes.Length];
        BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(0, 8), MasterSeed);
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(8, 4), ordinal);
        nameBytes.AsSpan().CopyTo(payload.AsSpan(12));

        byte[] hash = SHA256.HashData(payload);
        ulong seed = BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(0, 8));
        _entries.Add(new ReferenceRandomStreamEntry(streamName.Trim(), ordinal, seed));
        return seed;
    }
}
