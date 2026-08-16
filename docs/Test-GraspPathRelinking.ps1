[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "GRASP Path Relinking validation: missing '$Relative'."
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
            throw "GRASP Path Relinking validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.30.0") {
    throw "GRASP Path Relinking validation: expected repository version 0.30.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspPathRelinkingOptimizer.cs" @(
        'Id = "grasp-path-relinking"',
        "EliteSolutionPool<TSolution>",
        "IPathRelinkingProcedure<TSolution>",
        "MetaheuristicFamily.Hybrid",
        "MetaheuristicMechanism.MemoryBased",
        "context.CompleteIteration",
        "MaximumGraspPathRelinkingIterations"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GreedyForwardPathRelinkingProcedure.cs" @(
        "RegisterExternalProbeEvaluation",
        "PromoteOwnedExternalProbeSnapshot",
        "TryEvaluateCandidateObjective",
        "strictly decrease",
        "MaximumPathSteps"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\EliteSolutionPool.cs" @(
        "Reservoir",
        "_minimumDistance",
        "TrySelectGuide",
        "TryAdd"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\PathRelinkingContracts.cs" @(
        "IPathRelinkingDistance",
        "IPathRelinkingNeighborhood",
        "IPathRelinkingProcedure",
        "PathRelinkingProcedureResult"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\GraspPathRelinkingTests.cs" @(
        "ForwardRelinkingSelectsBestTargetDirectedMoveAndReachesGuide",
        "ForwardRelinkingRejectsMoveThatDoesNotDecreaseGuideDistance",
        "ElitePoolRejectsNonDiverseCandidate",
        "ElitePoolReplacesWorstWhenBetterDiverseCandidateArrives",
        "OptimizerUsesCommonOuterIterationStoppingLifecycle",
        "StableIdAndRuntimeCatalogExposeGraspPathRelinking"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        '"grasp-path-relinking"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        '"grasp-path-relinking"',
        '"constructive-methods"',
        '"10.1287/ijoc.1030.0059"'
    )

Require-Contains `
    "docs\pages\families\hybrid-methods.md" @(
        "@subpage grasp_path_relinking"
    )

Require-Contains `
    "docs\build-documentation.ps1" @(
        "additionalCategories",
        "# MULTI-FAMILY-ITEMS"
    )

Require-Contains `
    "docs\pages\algorithms\grasp-path-relinking.md" @(
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
        "grasp-path-relinking",
        "10.1287/ijoc.1030.0059",
        "\f["
    )

$catalog =
    (Read-Utf8 "docs\grasp-catalog.json") |
    ConvertFrom-Json

if (@($catalog.executable | Where-Object id -eq "grasp-path-relinking").Count -ne 1) {
    throw "GRASP Path Relinking validation: executable catalog entry missing."
}

if (@($catalog.reviewedDeferred | Where-Object id -eq "grasp-path-relinking").Count -ne 0) {
    throw "GRASP Path Relinking validation: method must no longer be deferred."
}

$documentationCatalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

if (@($documentationCatalog.algorithms).Count -lt 22) {
    throw "GRASP Path Relinking validation: expected at least 22 public algorithms."
}

if (@($documentationCatalog.algorithms | Where-Object id -eq "grasp-path-relinking").Count -ne 1) {
    throw "GRASP Path Relinking validation: documentation catalog entry missing or duplicated."
}

Require-Contains `
    "README.md" @(
        "22 public algorithms",
        "grasp-path-relinking"
    )

Write-Host `
    "GRASP Path Relinking validation passed: elite memory + greedy forward path engine + public GRASP-PR optimizer." `
    -ForegroundColor Green