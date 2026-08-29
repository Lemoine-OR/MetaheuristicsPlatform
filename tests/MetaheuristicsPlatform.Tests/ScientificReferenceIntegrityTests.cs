using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class ScientificReferenceIntegrityTests
{
    [Fact]
    public void NormalizeDoi_ProducesCanonicalIdentity_AndFingerprintIsStable()
    {
        Assert.Equal(
            "10.1007/s10107-003-0395-5",
            ScientificReferenceIntegrity.NormalizeDoi("https://doi.org/10.1007/s10107-003-0395-5"));

        var refs = new[]
        {
            new LockedScientificReference("lb", "10.1007/s10107-003-0395-5", "Fischetti and Lodi (2003).")
        };

        string first = ScientificReferenceIntegrity.ComputeReferenceSetFingerprint(refs);
        string second = ScientificReferenceIntegrity.ComputeReferenceSetFingerprint(refs);
        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
    }
}
