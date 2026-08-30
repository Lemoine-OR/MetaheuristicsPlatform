using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MetaheuristicsPlatform.Core;
using Xunit;

namespace MetaheuristicsPlatform.Tests;

public sealed class V1PublicApiFreezeTests
{
    private const string GenerateVariable =
        "METAHEURISTICSPLATFORM_GENERATE_V1_API_BASELINE";

    [Fact]
    public void V1_public_api_baseline_is_preserved()
    {
        var root = FindRepositoryRoot();
        var baselinePath =
            Path.Combine(root, "docs", "v1-public-api-baseline.json");

        var signatures =
            V1PublicApiSurface.Capture(typeof(IMetaheuristic<>).Assembly);

        if (string.Equals(
                Environment.GetEnvironmentVariable(GenerateVariable),
                "1",
                StringComparison.Ordinal))
        {
            var payload = new
            {
                schemaVersion = 1,
                baselineRelease = "1.0.0",
                sourceCommit = "7ac478247fc88052296565f22a2eb2d2809f0b5f",
                signatures
            };

            var json =
                JsonSerializer.Serialize(
                    payload,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                baselinePath,
                json + Environment.NewLine,
                new UTF8Encoding(false));

            return;
        }

        Assert.True(
            File.Exists(baselinePath),
            "The v1 public API baseline is missing.");

        using var document =
            JsonDocument.Parse(File.ReadAllText(baselinePath));

        var rootElement = document.RootElement;

        Assert.Equal(
            "1.0.0",
            rootElement.GetProperty("baselineRelease").GetString());

        Assert.Equal(
            "7ac478247fc88052296565f22a2eb2d2809f0b5f",
            rootElement.GetProperty("sourceCommit").GetString());

        var baseline =
            rootElement
                .GetProperty("signatures")
                .EnumerateArray()
                .Select(element => element.GetString() ?? "")
                .Where(value => value.Length != 0)
                .ToArray();

        Assert.True(
            baseline.Length > 100,
            "The v1 public API baseline is unexpectedly small.");

        Assert.Equal(
            baseline.Length,
            baseline.Distinct(StringComparer.Ordinal).Count());

        var actual =
            signatures.ToHashSet(StringComparer.Ordinal);

        var missing =
            baseline
                .Where(signature => !actual.Contains(signature))
                .Take(40)
                .ToArray();

        Assert.True(
            missing.Length == 0,
            "Breaking v1 public API change detected. Missing baseline signature(s):" +
            Environment.NewLine +
            string.Join(Environment.NewLine, missing));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "version.json")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Unable to locate the MetaheuristicsPlatform repository root.");
    }
}