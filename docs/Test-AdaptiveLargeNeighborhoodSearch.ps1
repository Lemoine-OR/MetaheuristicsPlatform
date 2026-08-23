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
        throw "Adaptive Large Neighborhood Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.53.0") {
    throw "Adaptive Large Neighborhood Search validation: expected repository version 0.53.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchOperators.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodAdaptation.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchAcceptance.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchState.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchReferences.cs",
    "tests\MetaheuristicsPlatform.Tests\AdaptiveLargeNeighborhoodSearchTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\AdaptiveLargeNeighborhoodSearchBenchmarks.cs",
    "docs\adaptive-large-neighborhood-search-component-catalog.json",
    "docs\Build-AdaptiveLargeNeighborhoodSearchDocumentation.ps1",
    "docs\pages\components\adaptive-large-neighborhood-search-components.md",
    "docs\pages\algorithms\adaptive-large-neighborhood-search-ropke-pisinger-2006.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Adaptive Large Neighborhood Search validation: missing '$relative'."
    }
}

$source =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchOptimizer.cs"

foreach ($marker in @(
    "AdaptiveLargeNeighborhoodAdaptation.SelectIndex(",
    "_destroyOperators[destroyIndex]",
    "_repairOperators[repairIndex]",
    "visited.Add(",
    "AdaptiveLargeNeighborhoodAdaptation.DetermineReward(",
    "UpdateWeights(",
    "context.Evaluate(",
    "context.EvaluateStopping(",
    "context.CompleteIteration(",
    "MaximumAdaptiveLargeNeighborhoodSearchIterations"
)) {
    if (-not $source.Contains($marker)) {
        throw "Adaptive Large Neighborhood Search validation: implementation marker '$marker' is missing."
    }
}

$adaptation =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodAdaptation.cs"

foreach ($marker in @(
    "random.NextDouble()",
    "accumulatedScore /",
    "(1.0 - reactionFactor) * currentWeight",
    "parameters.GlobalBestReward",
    "parameters.ImprovingReward",
    "parameters.AcceptedReward"
)) {
    if (-not $adaptation.Contains($marker)) {
        throw "Adaptive Large Neighborhood Search validation: adaptation marker '$marker' is missing."
    }
}

$acceptance =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchAcceptance.cs"

foreach ($marker in @(
    "TrajectoryObjectiveComparison.ComputeDegradation",
    "Math.Pow(",
    "Math.Exp(",
    "random.NextDouble()"
)) {
    if (-not $acceptance.Contains($marker)) {
        throw "Adaptive Large Neighborhood Search validation: acceptance marker '$marker' is missing."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchReferences.cs"

foreach ($doi in @(
    "10.1287/trsc.1050.0135",
    "10.1016/j.cor.2005.09.012"
)) {
    if (-not $references.Contains($doi)) {
        throw "Adaptive Large Neighborhood Search validation: DOI '$doi' is missing."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AdaptiveLargeNeighborhoodSearchTests.cs"

foreach ($marker in @(
    "RouletteSelectionUsesPublishedWeightProportions",
    "SegmentWeightUpdateMatchesRopkePisingerFormula",
    "NovelOutcomeRewardTiersAreCanonical",
    "GeometricMetropolisAcceptanceIsSenseSymmetric",
    "OneIterationUsesExactlyOneDestroyAndOneRepair",
    "EvaluationBudgetStopsBeforeIncompleteAdaptiveCycleIsCounted",
    "SameSeedProducesSameAdaptiveTrajectory",
    "StableIdCatalogAndTypedFactoryRegistrationAreAvailable",
    "private sealed class CountingRandomSource",
    "public ulong NextUInt64()",
    "public void Fill("
)) {
    if (-not $tests.Contains($marker)) {
        throw "Adaptive Large Neighborhood Search validation: focused test marker '$marker' is missing."
    }
}

if ($tests.Contains("FixedRandomSource")) {
    throw "Adaptive Large Neighborhood Search validation: tests reference undeclared helper 'FixedRandomSource'."
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$entry =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "adaptive-large-neighborhood-search-ropke-pisinger-2006"
        }
    )

if ($entry.Count -ne 1) {
    throw "Adaptive Large Neighborhood Search validation: public catalog identity is missing or duplicated."
}

if ([string]$entry[0].doi -ne "10.1287/trsc.1050.0135") {
    throw "Adaptive Large Neighborhood Search validation: primary DOI mismatch."
}

if (-not [bool]$entry[0].requiresComposition) {
    throw "Adaptive Large Neighborhood Search validation: typed composition must remain explicit."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Adaptive Large Neighborhood Search validation: update mathematics must use aligned display layout."
}

$components =
    (Read-Utf8 "docs\adaptive-large-neighborhood-search-component-catalog.json") |
    ConvertFrom-Json

if ([int]$components.implementedCount -ne 4 -or
    [int]$components.reviewedDeferredCount -ne 3) {
    throw "Adaptive Large Neighborhood Search validation: component counts are incorrect."
}

Write-Host `
    "Adaptive Large Neighborhood Search validation passed: independent roulette pools + novelty-aware sigma rewards + segmented reaction-factor learning + geometric Metropolis acceptance." `
    -ForegroundColor Green
