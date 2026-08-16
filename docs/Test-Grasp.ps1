[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "GRASP validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains(
    [string]$Relative,
    [string[]]$Markers) {

    $text = Read-Utf8 $Relative

    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            throw "GRASP validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

$versionText = [version]([string]$version.version)

if ($versionText -lt [version]"0.28.0") {
    throw "GRASP validation: expected repository version 0.28.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspOptimizer.cs" @(
        'Id = "grasp-feo-resende-1995"',
        "IGraspConstructionProcedure<TSolution>",
        "ILocalSearchProcedure<TSolution>",
        "MetaheuristicFamily.Constructive",
        "MetaheuristicMechanism.Constructive",
        "GraspReferences.FeoResende1995",
        "MaximumGraspIterations"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\CanonicalGraspConstructionProcedure.cs" @(
        "ComputeThreshold",
        "restrictedCandidateCount",
        "random.NextInt32(restrictedCandidateCount)",
        "best + alpha * (worst - best)",
        "best - alpha * (best - worst)",
        "GRASP greedy scores must be finite"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspContracts.cs" @(
        "IGraspCandidateEnumerator",
        "IGraspConstructionModel",
        "IGraspConstructionProcedure",
        "GraspGreedyScoreSense"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspReferences.cs" @(
        "10.1016/0167-6377(89)90002-3",
        "10.1007/BF01096763"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\GraspTests.cs" @(
        "AlphaZeroKeepsOnlyGreedyBestCandidateForMinimizationScore",
        "AlphaOneAdmitsEntireCandidateListAndUsesUniformReservoirSelection",
        "ConstructionIsAdaptiveAndRecomputesCandidateScoresAfterEachSelection",
        "GraspOptimizerComposesConstructionAndReusableLocalSearch",
        "StableIdAndRuntimeCatalogExposeCanonicalGrasp"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        '"grasp-feo-resende-1995"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        '"grasp-feo-resende-1995"',
        '"constructive-methods"',
        '"10.1007/BF01096763"'
    )

Require-Contains `
    "docs\pages\algorithms\grasp-feo-resende-1995.md" @(
        "## General description",
        "## Technical specifications",
        "## Complexity",
        "## Applicability",
        "## Detailed operation",
        "## Parameters",
        "## API example",
        "## Stable factory ID",
        "## Mathematical details",
        "### Problem formulation",
        "### Update equations / iterations",
        "### Assumptions",
        "### Convergence conditions",
        "### Scientific references",
        "grasp-feo-resende-1995",
        "10.1007/BF01096763",
        "\f["
    )

Require-Contains `
    "docs\pages\families\constructive-methods.md" @(
        "@page family_constructive_methods",
        "grasp_feo_resende_1995"
    )

$catalog =
    (Read-Utf8 "docs\grasp-catalog.json") |
    ConvertFrom-Json

if (@($catalog.executable).Count -lt 1) {
    throw "GRASP validation: expected one executable core GRASP entry."
}

if (@($catalog.reviewedDeferred).Count -lt 1) {
    throw "GRASP validation: expected Reactive GRASP and Path Relinking to be reviewed/deferred."
}

$documentationCatalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$algorithms =
    @($documentationCatalog.algorithms)

$families =
    @($documentationCatalog.families)

if ($algorithms.Count -lt 20) {
    throw "GRASP validation: expected at least 20 public algorithms."
}

if ($families.Count -lt 5) {
    throw "GRASP validation: expected at least 5 family pages."
}

if (@($algorithms | Where-Object id -eq "grasp-feo-resende-1995").Count -ne 1) {
    throw "GRASP validation: documentation catalog must expose exactly one canonical GRASP entry."
}

if (@($families | Where-Object id -eq "constructive-methods").Count -ne 1) {
    throw "GRASP validation: documentation catalog must expose the constructive family."
}

Require-Contains `
    "README.md" @(
        "public algorithms",
        "### Constructive methods",
        "grasp-feo-resende-1995"
    )

Write-Host `
    "GRASP core validation passed: canonical adaptive threshold-RCL construction + reusable local search." `
    -ForegroundColor Green
