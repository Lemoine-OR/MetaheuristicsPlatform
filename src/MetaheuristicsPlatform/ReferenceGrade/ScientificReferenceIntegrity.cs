using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MetaheuristicsPlatform.ReferenceGrade;

public sealed record LockedScientificReference(
    string StableId,
    string PrimaryIdentifier,
    string Citation);

public static class ScientificReferenceIntegrity
{
    private static readonly Regex DoiPattern =
        new(
            @"^10\.\d{4,9}/\S+$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string NormalizeDoi(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "DOI must not be empty.",
                nameof(value));

        string normalized = value.Trim();
        normalized = Regex.Replace(
            normalized,
            @"^(https?://(dx\.)?doi\.org/|doi:\s*)",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        normalized = normalized.ToLowerInvariant();
        if (!DoiPattern.IsMatch(normalized))
            throw new FormatException(
                "Primary identifier is not a valid DOI.");

        return normalized;
    }

    public static string ComputeReferenceSetFingerprint(
        IEnumerable<LockedScientificReference> references)
    {
        ArgumentNullException.ThrowIfNull(references);
        LockedScientificReference[] items = references.ToArray();
        if (items.Length == 0)
            throw new ArgumentException(
                "Reference set must not be empty.",
                nameof(references));

        if (items.Any(x =>
                string.IsNullOrWhiteSpace(x.StableId) ||
                string.IsNullOrWhiteSpace(x.Citation)))
            throw new ArgumentException(
                "Scientific reference identity fields must not be empty.");

        string[] canonical = items
            .Select(x => new
            {
                x.StableId,
                Doi = NormalizeDoi(x.PrimaryIdentifier),
                x.Citation
            })
            .OrderBy(x => x.StableId, StringComparer.Ordinal)
            .Select(x => $"{x.StableId}|{x.Doi}|{x.Citation.Trim()}")
            .ToArray();

        if (canonical.Select(x => x.Split('|')[0]).Distinct(StringComparer.Ordinal).Count() != canonical.Length)
            throw new ArgumentException(
                "Stable IDs must be unique in a scientific reference set.");
        if (items.Select(x => NormalizeDoi(x.PrimaryIdentifier)).Distinct(StringComparer.Ordinal).Count() != items.Length)
            throw new ArgumentException(
                "Primary identifiers must be unique in a scientific reference set.");

        return Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        string.Join("\n", canonical))))
            .ToLowerInvariant();
    }
}
