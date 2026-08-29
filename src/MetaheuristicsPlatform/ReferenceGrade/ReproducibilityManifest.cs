using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record ReproducibilityManifest(
    string AlgorithmId,
    string LibraryVersion,
    ulong Seed,
    string ParameterFingerprint,
    string DatasetFingerprint,
    string ProvenanceFingerprint,
    string RuntimeId)
{
    public void Validate()
    {
        Require(AlgorithmId, nameof(AlgorithmId));
        Require(LibraryVersion, nameof(LibraryVersion));
        Require(ParameterFingerprint, nameof(ParameterFingerprint));
        Require(DatasetFingerprint, nameof(DatasetFingerprint));
        Require(ProvenanceFingerprint, nameof(ProvenanceFingerprint));
        Require(RuntimeId, nameof(RuntimeId));
    }

    public string CanonicalFingerprint()
    {
        Validate();

        var canonical = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["algorithmId"] = AlgorithmId.Trim(),
            ["datasetFingerprint"] = DatasetFingerprint.Trim(),
            ["libraryVersion"] = LibraryVersion.Trim(),
            ["parameterFingerprint"] = ParameterFingerprint.Trim(),
            ["provenanceFingerprint"] = ProvenanceFingerprint.Trim(),
            ["runtimeId"] = RuntimeId.Trim(),
            ["seed"] = Seed.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        byte[] payload =
            JsonSerializer.SerializeToUtf8Bytes(canonical);

        return Convert.ToHexString(
                SHA256.HashData(payload))
            .ToLowerInvariant();
    }

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Reproducibility manifest fields must not be empty.",
                name);
    }
}
