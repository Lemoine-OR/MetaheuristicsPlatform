namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record ReferenceGradeGateInput(
    ScientificProvenanceRecord Provenance,
    ReproducibilityManifest Reproducibility,
    ParameterSchemaRegistry ParameterSchemas,
    BenchmarkProtocol BenchmarkProtocol,
    CrossFamilyCompositionContract Composition,
    ReferenceRandomStreamTrace RandomTrace,
    IReadOnlyList<LockedScientificReference> References);

public sealed record ReferenceGradeGateReport(
    bool IsGreen,
    IReadOnlyList<string> Checks,
    string ProvenanceFingerprint,
    string ReproducibilityFingerprint,
    string ReferenceFingerprint);

public static class ReferenceGradeStabilityGate
{
    public static ReferenceGradeGateReport Evaluate(
        ReferenceGradeGateInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Provenance);
        ArgumentNullException.ThrowIfNull(input.Reproducibility);
        ArgumentNullException.ThrowIfNull(input.ParameterSchemas);
        ArgumentNullException.ThrowIfNull(input.BenchmarkProtocol);
        ArgumentNullException.ThrowIfNull(input.Composition);
        ArgumentNullException.ThrowIfNull(input.RandomTrace);
        ArgumentNullException.ThrowIfNull(input.References);

        input.Provenance.Validate();
        input.Reproducibility.Validate();
        input.BenchmarkProtocol.Validate();
        input.Composition.ValidateAcyclic();

        if (input.ParameterSchemas.AlgorithmIds.Count == 0)
            throw new InvalidOperationException(
                "Reference-grade gate requires at least one parameter schema.");
        if (input.RandomTrace.Entries.Count == 0)
            throw new InvalidOperationException(
                "Reference-grade gate requires at least one derived random stream.");
        if (input.References.Count == 0)
            throw new InvalidOperationException(
                "Reference-grade gate requires at least one locked scientific reference.");

        string provenance = input.Provenance.ComputeFingerprint();
        string reproducibility = input.Reproducibility.CanonicalFingerprint();
        string references = ScientificReferenceIntegrity.ComputeReferenceSetFingerprint(input.References);

        string[] checks =
        {
            "scientific-provenance",
            "reproducibility-manifest",
            "parameter-schema-registry",
            "benchmark-protocol",
            "cross-family-composition",
            "random-stream-trace",
            "scientific-reference-integrity"
        };

        return new ReferenceGradeGateReport(
            true,
            checks,
            provenance,
            reproducibility,
            references);
    }
}
