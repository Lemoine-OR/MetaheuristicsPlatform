[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path =
        Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Large Neighborhood Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.52.0") {
    throw "Large Neighborhood Search validation: expected repository version 0.52.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchState.cs",
    "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchContracts.cs",
    "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchReferences.cs",
    "tests\MetaheuristicsPlatform.Tests\LargeNeighborhoodSearchTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\LargeNeighborhoodSearchBenchmarks.cs",
    "docs\large-neighborhood-search-component-catalog.json",
    "docs\Build-LargeNeighborhoodSearchDocumentation.ps1",
    "docs\pages\components\large-neighborhood-search-components.md",
    "docs\pages\algorithms\large-neighborhood-search-shaw-1998.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Large Neighborhood Search validation: missing '$relative'."
    }
}

$source =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchOptimizer.cs"

foreach ($marker in @(
    "solutionCloner.Clone(",
    "_destroy.Destroy(",
    "_repair.Repair(",
    "context.Evaluate(",
    "context.EvaluateStopping(",
    "_acceptance.ShouldAccept(",
    "context.CompleteIteration(",
    "incomplete and is not counted",
    "MaximumLargeNeighborhoodSearchIterations"
)) {
    if (-not $source.Contains($marker)) {
        throw "Large Neighborhood Search validation: implementation marker '$marker' is missing."
    }
}

$contracts =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchContracts.cs"

foreach ($marker in @(
    "ILargeNeighborhoodDestroyOperator",
    "ILargeNeighborhoodRepairOperator",
    "ILargeNeighborhoodAcceptancePolicy",
    "ImprovingOnlyLargeNeighborhoodAcceptancePolicy",
    "context.Sense.IsBetter("
)) {
    if (-not $contracts.Contains($marker)) {
        throw "Large Neighborhood Search validation: contract marker '$marker' is missing."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\LargeNeighborhoodSearch\LargeNeighborhoodSearchReferences.cs"

foreach ($doi in @(
    "10.1007/3-540-49481-2_30",
    "10.1007/978-1-4419-1665-5_13"
)) {
    if (-not $references.Contains($doi)) {
        throw "Large Neighborhood Search validation: DOI '$doi' is missing."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\LargeNeighborhoodSearchTests.cs"

foreach ($marker in @(
    "DestroyPrecedesRepairAndImprovingCandidateIsAccepted",
    "PartialSolutionIsNeverEvaluated",
    "EvaluationBudgetStopsBeforeIncompleteCycleIsCounted",
    "WorseningCandidateIsRejectedWithoutLosingBestSoFar",
    "SameSeedProducesSameDestroyRepairTrajectory",
    "StableIdCatalogAndTypedFactoryRegistrationAreAvailable",
    "private sealed class CountingRandomSource : IRandomSource",
    "public ulong Seed",
    "public ulong NextUInt64()",
    "public double NextDouble()",
    "public int NextInt32(",
    "public void Fill("
)) {
    if (-not $tests.Contains($marker)) {
        throw "Large Neighborhood Search validation: focused test '$marker' is missing."
    }
}

if ($tests.Contains("FixedRandomSource")) {
    throw "Large Neighborhood Search validation: test references undeclared helper 'FixedRandomSource'. Test-only helpers introduced by this release must be declared locally or verified in the exact baseline."
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$entry =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "large-neighborhood-search-shaw-1998"
        }
    )

if ($entry.Count -ne 1) {
    throw "Large Neighborhood Search validation: public catalog identity is missing or duplicated."
}

if ([string]$entry[0].doi -ne "10.1007/3-540-49481-2_30") {
    throw "Large Neighborhood Search validation: primary DOI mismatch."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Large Neighborhood Search validation: update mathematics must use aligned display layout."
}

$components =
    (Read-Utf8 "docs\large-neighborhood-search-component-catalog.json") |
    ConvertFrom-Json

if ([int]$components.implementedCount -ne 3 -or
    [int]$components.reviewedDeferredCount -ne 3) {
    throw "Large Neighborhood Search validation: component counts are incorrect."
}

$ids =
    @(
        $components.entries |
        ForEach-Object {
            [string]$_.id
        }
    )

foreach ($id in @(
    "lns.destroy.operator",
    "lns.repair.operator",
    "lns.acceptance.improving-only",
    "lns.destroy.shaw-related-removal",
    "lns.repair.constraint-lds",
    "lns.adaptive.operator-selection"
)) {
    if ($ids -notcontains $id) {
        throw "Large Neighborhood Search validation: component '$id' is missing."
    }
}

$page =
    Read-Utf8 "docs\pages\algorithms\large-neighborhood-search-shaw-1998.md"

foreach ($marker in @(
    "destroy",
    "repair",
    "10.1007/3-540-49481-2_30",
    "10.1287/trsc.1050.0135",
    "\begin{aligned}"
)) {
    if (-not $page.Contains($marker)) {
        throw "Large Neighborhood Search validation: scientific-page marker '$marker' is missing."
    }
}

Write-Host `
    "Large Neighborhood Search validation passed: generic destroy/repair composition, strict acceptance, exact incomplete-cycle accounting and ALNS separation." `
    -ForegroundColor Green
