using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class ReferenceGradeStabilityGateTests
{
    [Fact]
    public void Evaluate_ReturnsGreen_WhenAllReferenceContractsAreSatisfied()
    {
        var provenance = new ScientificProvenanceRecord(
            "algorithm-id", "0.173.0", "10.1000/example",
            "mechanism-preserving-platform-adaptation",
            "The literature mechanism is preserved while lifecycle integration is adapted explicitly.");
        var reproducibility = new ReproducibilityManifest(
            "algorithm-id", "0.173.0", 7UL,
            "params", "dataset", provenance.ComputeFingerprint(), "net10.0");
        ParameterSchemaRegistry registry = new();
        registry.Register(
            "algorithm-id",
            new[] { new ReferenceParameterDescriptor("iterations", "System.Int32", "10", "Maximum iterations.", 1, 100) });
        var protocol = new BenchmarkProtocol(
            "protocol", "dataset", new ulong[] { 7 }, 0, 1, TimeSpan.FromSeconds(10));
        var composition = new CrossFamilyCompositionContract(
            new[] { new ReferenceCompositionNode("search", "algorithm-id", ReferenceCompositionRole.PrimarySearch) },
            Array.Empty<ReferenceCompositionEdge>());
        ReferenceRandomStreamTrace trace = new(7UL);
        trace.DeriveSeed("main");
        var references = new[]
        {
            new LockedScientificReference("algorithm-id", "10.1000/example", "Example reference.")
        };

        ReferenceGradeGateReport report = ReferenceGradeStabilityGate.Evaluate(
            new ReferenceGradeGateInput(
                provenance, reproducibility, registry, protocol,
                composition, trace, references));

        Assert.True(report.IsGreen);
        Assert.Equal(7, report.Checks.Count);
        Assert.Equal(64, report.ProvenanceFingerprint.Length);
        Assert.Equal(64, report.ReproducibilityFingerprint.Length);
        Assert.Equal(64, report.ReferenceFingerprint.Length);
    }
}
