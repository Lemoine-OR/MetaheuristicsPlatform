using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class BenchmarkProtocolTests
{
    [Fact]
    public void Protocol_AndResultEnvelope_ValidateReferenceGradeFields()
    {
        var protocol = new BenchmarkProtocol(
            "protocol-v1", "dataset-sha", new ulong[] { 1, 2, 3 }, 1, 5, TimeSpan.FromSeconds(30));
        protocol.Validate();

        var result = new BenchmarkResultEnvelope(
            protocol.ProtocolId,
            "algorithm-id",
            "0.169.0",
            1UL,
            new[] { new BenchmarkMetric("objective", 12.5, "cost") },
            "manifest-sha");
        result.Validate();

        Assert.Equal(5, protocol.MeasuredRuns);
        Assert.Single(result.Metrics);
    }
}
