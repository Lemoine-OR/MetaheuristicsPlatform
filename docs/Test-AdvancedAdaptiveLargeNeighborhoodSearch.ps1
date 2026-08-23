[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Advanced ALNS validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.54.0") {
    throw "Advanced ALNS validation: expected repository version 0.54.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodOperatorSelection.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdvancedAdaptiveLargeNeighborhoodSelection.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdvancedAdaptiveLargeNeighborhoodAcceptance.cs",
    "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdvancedAdaptiveLargeNeighborhoodSearchReferences.cs",
    "tests\MetaheuristicsPlatform.Tests\AdvancedAdaptiveLargeNeighborhoodSearchTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\AdvancedAdaptiveLargeNeighborhoodSearchBenchmarks.cs",
    "docs\advanced-adaptive-large-neighborhood-search-catalog.json",
    "docs\Build-AdvancedAdaptiveLargeNeighborhoodSearchDocumentation.ps1",
    "docs\pages\components\advanced-adaptive-large-neighborhood-search-components.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Advanced ALNS validation: missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdaptiveLargeNeighborhoodSearchOptimizer.cs"

foreach ($marker in @(
    "IAdaptiveLargeNeighborhoodOperatorSelectionStrategy",
    "IndependentSegmentedRouletteOperatorSelectionStrategy.Instance",
    "selectionSession.Select(",
    "selectionSession.RecordOutcome(",
    "selectionSession.CompleteIteration("
)) {
    if (-not $optimizer.Contains($marker)) {
        throw "Advanced ALNS validation: optimizer extension marker '$marker' is missing."
    }
}

$selection =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdvancedAdaptiveLargeNeighborhoodSelection.cs"

foreach ($marker in @(
    "PairCoupledSegmentedRouletteOperatorSelectionStrategy",
    "AlphaUcbOperatorPairSelectionStrategy",
    "Math.Sqrt(",
    "Math.Log(1.0 + iteration)",
    "AdaptiveLargeNeighborhoodAdaptation.UpdateWeight("
)) {
    if (-not $selection.Contains($marker)) {
        throw "Advanced ALNS validation: selection marker '$marker' is missing."
    }
}

$acceptance =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdvancedAdaptiveLargeNeighborhoodAcceptance.cs"

foreach ($marker in @(
    "TrajectoryAcceptanceLargeNeighborhoodAdapter",
    "ThresholdAcceptancePolicy",
    "RecordToRecordTravelAcceptancePolicy",
    "TrajectoryAcceptanceContext"
)) {
    if (-not $acceptance.Contains($marker)) {
        throw "Advanced ALNS validation: acceptance marker '$marker' is missing."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AdaptiveLargeNeighborhoodSearch\AdvancedAdaptiveLargeNeighborhoodSearchReferences.cs"

foreach ($doi in @(
    "10.1002/net.21905",
    "10.1007/s12532-021-00209-7",
    "10.1007/s10732-018-9377-x"
)) {
    if (-not $references.Contains($doi)) {
        throw "Advanced ALNS validation: DOI '$doi' is missing."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AdvancedAdaptiveLargeNeighborhoodSearchTests.cs"

foreach ($marker in @(
    "PairCoupledSegmentedRouletteLearnsJointPairWeight",
    "AlphaUcbExploresAllPairsBeforeExploitation",
    "ThresholdAdapterUsesTrajectoryAcceptanceWithoutRandomDraw",
    "RecordToRecordAdapterUsesBestRecordDeviation",
    "private sealed class SequenceRandomSource",
    "public ulong NextUInt64()",
    "public void Fill("
)) {
    if (-not $tests.Contains($marker)) {
        throw "Advanced ALNS validation: focused test marker '$marker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\advanced-adaptive-large-neighborhood-search-catalog.json") |
    ConvertFrom-Json

if ([int]$catalog.implementedCount -ne 4 -or
    [int]$catalog.reviewedDeferredCount -ne 2) {
    throw "Advanced ALNS validation: component counts are incorrect."
}

foreach ($entry in @($catalog.entries | Where-Object formulaMode -eq "math")) {
    if (-not ([string]$entry.formula).StartsWith('\begin{aligned}')) {
        throw "Advanced ALNS validation: mathematical component '$($entry.id)' must use aligned layout."
    }
}

$publicCatalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

if (@($publicCatalog.algorithms).Count -ne 44) {
    throw "Advanced ALNS validation: v0.54 is a component release and must keep 44 public algorithms."
}

if (@($publicCatalog.algorithms |
    Where-Object id -eq "adaptive-large-neighborhood-search-ropke-pisinger-2006").Count -ne 1) {
    throw "Advanced ALNS validation: canonical v0.53 ALNS identity must remain unique."
}

Write-Host `
    "Advanced ALNS validation passed: pair-coupled segmented roulette + alpha-UCB pair learning + Threshold/RTR acceptance composition; public algorithm count remains 44." `
    -ForegroundColor Green
