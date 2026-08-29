namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record BenchmarkProtocol(
    string ProtocolId,
    string DatasetId,
    IReadOnlyList<ulong> Seeds,
    int WarmupRuns,
    int MeasuredRuns,
    TimeSpan PerRunTimeout)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProtocolId) ||
            string.IsNullOrWhiteSpace(DatasetId))
            throw new ArgumentException(
                "Benchmark protocol identifiers must not be empty.");

        if (Seeds is null || Seeds.Count == 0 ||
            Seeds.Distinct().Count() != Seeds.Count)
            throw new ArgumentException(
                "Benchmark seeds must be non-empty and unique.",
                nameof(Seeds));

        if (WarmupRuns < 0 ||
            MeasuredRuns <= 0 ||
            PerRunTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(MeasuredRuns));
    }
}

public sealed record BenchmarkMetric(
    string Name,
    double Value,
    string Unit)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Unit) ||
            !double.IsFinite(Value))
            throw new ArgumentException(
                "Benchmark metrics require a name, unit and finite value.");
    }
}

public sealed record BenchmarkResultEnvelope(
    string ProtocolId,
    string AlgorithmId,
    string LibraryVersion,
    ulong Seed,
    IReadOnlyList<BenchmarkMetric> Metrics,
    string ReproducibilityFingerprint)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProtocolId) ||
            string.IsNullOrWhiteSpace(AlgorithmId) ||
            string.IsNullOrWhiteSpace(LibraryVersion) ||
            string.IsNullOrWhiteSpace(ReproducibilityFingerprint))
            throw new ArgumentException(
                "Benchmark result identity fields must not be empty.");

        if (Metrics is null || Metrics.Count == 0)
            throw new ArgumentException(
                "Benchmark result must expose at least one metric.",
                nameof(Metrics));

        foreach (BenchmarkMetric metric in Metrics)
            metric.Validate();
    }
}
