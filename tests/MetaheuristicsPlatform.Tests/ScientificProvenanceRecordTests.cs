using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class ScientificProvenanceRecordTests
{
    [Fact]
    public void Fingerprint_IsDeterministic_AndSensitiveToBoundary()
    {
        var first = new ScientificProvenanceRecord(
            "algorithm-id",
            "0.166.0",
            "10.1000/example",
            "exact-literature-reproduction",
            "The platform only adapts generic lifecycle and cancellation integration.");
        var second = first with { };
        var changed = first with { AdaptationBoundary = "A materially different platform adaptation boundary is declared here." };

        Assert.Equal(first.ComputeFingerprint(), second.ComputeFingerprint());
        Assert.NotEqual(first.ComputeFingerprint(), changed.ComputeFingerprint());
        Assert.Equal(64, first.ComputeFingerprint().Length);
    }
}
