[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Reactive GRASP validation: missing '$Relative'."
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
            throw "Reactive GRASP validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([string]$version.version -ne "0.29.0") {
    throw "Reactive GRASP validation: version.json must be 0.29.0."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\ReactiveGraspOptimizer.cs" @(
        'Id = "reactive-grasp-prais-ribeiro-2000"',
        "PraisRibeiroReactiveAlphaController",
        "MetaheuristicMechanism.Adaptive",
        "controller.Observe",
        "context.CompleteIteration",
        "MaximumReactiveGraspIterations"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\PraisRibeiroReactiveAlphaController.cs" @(
        "_bestObjective / average",
        "average / _bestObjective",
        "_probabilities[i] *= inverse",
        "_distinctObserved == _alphas.Length",
        "strictly positive objective values"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\ReactiveGraspParameters.cs" @(
        "AlphaValues",
        "ProbabilityUpdatePeriod",
        "MaximumConstructionSteps",
        "0.0, 0.1, 0.2"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspReferences.cs" @(
        "PraisRibeiro2000",
        "10.1287/ijoc.12.3.164.12639"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspOptimizer.cs" @(
        "context.CompleteIteration"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\ReactiveGraspTests.cs" @(
        "MinimizationRatioUpdateFavorsLowerAverageObjective",
        "MaximizationMirrorRatioFavorsHigherAverageObjective",
        "CanonicalRatioUpdateRejectsZeroOrNegativeObjectives",
        "ReactiveOptimizerUsesCommonIterationStoppingLifecycle",
        "CanonicalGraspNowUsesCommonIterationStoppingLifecycle",
        "StableIdAndRuntimeCatalogExposeReactiveGrasp"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        '"reactive-grasp-prais-ribeiro-2000"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        '"reactive-grasp-prais-ribeiro-2000"',
        '"constructive-methods"',
        '"10.1287/ijoc.12.3.164.12639"'
    )

Require-Contains `
    "docs\pages\algorithms\reactive-grasp-prais-ribeiro-2000.md" @(
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
        "reactive-grasp-prais-ribeiro-2000",
        "10.1287/ijoc.12.3.164.12639",
        "\f["
    )

$catalog =
    (Read-Utf8 "docs\grasp-catalog.json") |
    ConvertFrom-Json

if (@($catalog.executable).Count -ne 2) {
    throw "Reactive GRASP validation: expected exactly two executable GRASP algorithms."
}

if (@($catalog.reviewedDeferred).Count -lt 1) {
    throw "Reactive GRASP validation: Path Relinking must remain reviewed/deferred."
}

$documentationCatalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$algorithms =
    @($documentationCatalog.algorithms)

$families =
    @($documentationCatalog.families)

if ($algorithms.Count -lt 21) {
    throw "Reactive GRASP validation: expected at least 21 public algorithms."
}

if ($families.Count -lt 5) {
    throw "Reactive GRASP validation: expected at least 5 family pages."
}

if (@($algorithms | Where-Object id -eq "reactive-grasp-prais-ribeiro-2000").Count -ne 1) {
    throw "Reactive GRASP validation: documentation catalog must expose exactly one Reactive GRASP entry."
}

Require-Contains `
    "README.md" @(
        "21 public algorithms",
        "### Constructive methods",
        "grasp-feo-resende-1995",
        "reactive-grasp-prais-ribeiro-2000"
    )

Write-Host `
    "Reactive GRASP validation passed: Prais-Ribeiro adaptive alpha probabilities + exact common iteration lifecycle." `
    -ForegroundColor Green
