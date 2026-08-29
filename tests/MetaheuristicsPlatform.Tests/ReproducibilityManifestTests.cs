using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class ReproducibilityManifestTests
{
    [Fact]
    public void CanonicalFingerprint_IsStable()
    {
        var manifest = new ReproducibilityManifest(
            "algorithm-id", "0.167.0", 42UL,
            "params-sha", "dataset-sha", "provenance-sha", "net10.0");

        string first = manifest.CanonicalFingerprint();
        string second = manifest.CanonicalFingerprint();

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
    }
}
