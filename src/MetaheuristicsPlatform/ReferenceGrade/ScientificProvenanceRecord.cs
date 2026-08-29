using System.Security.Cryptography;
using System.Text;

namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record ScientificProvenanceRecord(
    string StableId,
    string LibraryVersion,
    string PrimaryIdentifier,
    string ReproductionMode,
    string AdaptationBoundary)
{
    public void Validate()
    {
        Require(StableId, nameof(StableId));
        Require(LibraryVersion, nameof(LibraryVersion));
        Require(PrimaryIdentifier, nameof(PrimaryIdentifier));
        Require(ReproductionMode, nameof(ReproductionMode));
        Require(AdaptationBoundary, nameof(AdaptationBoundary));

        if (AdaptationBoundary.Trim().Length < 20)
            throw new ArgumentException(
                "Adaptation boundary is underspecified.",
                nameof(AdaptationBoundary));
    }

    public string ComputeFingerprint()
    {
        Validate();

        string canonical =
            string.Join(
                "\n",
                StableId.Trim(),
                LibraryVersion.Trim(),
                PrimaryIdentifier.Trim(),
                ReproductionMode.Trim(),
                AdaptationBoundary.Trim());

        return Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static void Require(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Reference-grade provenance fields must not be empty.",
                parameterName);
    }
}
