[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Threshold Accepting validation: missing '$Relative'."
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
            throw "Threshold Accepting validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.33.0") {
    throw "Threshold Accepting validation: expected repository version 0.33.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\TA\ThresholdAcceptancePolicy.cs" @(
        "ITrajectoryAcceptancePolicy",
        "TrajectoryTransitionQuality.Worsening",
        "ComputeDegradation",
        "degradation <=",
        "Threshold"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\TA\ThresholdAcceptingSchedules.cs" @(
        "LinearThresholdSchedule",
        "GeometricThresholdSchedule",
        "ExplicitThresholdSchedule",
        "non-increasing",
        "IThresholdAcceptingSchedule"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\TA\ThresholdAcceptingOptimizer.cs" @(
        'Id =',
        '"threshold-accepting-dueck-scheuer-1990"',
        "ReversibleTrajectoryStepExecutor",
        "RegisterOwnedExternalEvaluationSnapshot",
        "TransitionsPerThresholdLevel",
        "MinimumThreshold",
        "10.1016/0021-9991(90)90201-B"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Trajectory\TrajectoryObjectiveComparison.cs" @(
        "ComputeDegradation",
        "OptimizationSense.Minimize",
        "OptimizationSense.Maximize"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\ThresholdAcceptingTests.cs" @(
        "AcceptancePolicyUsesDeterministicWorseningThreshold",
        "AcceptancePolicyMirrorsMaximizationSense",
        "ZeroThresholdReducesAcceptanceToNonWorseningMoves",
        "LinearScheduleReachesZeroExactly",
        "ExplicitScheduleRequiresMonotoneThresholdSequence",
        "OptimizerDeltaFastPathDoesNotApplyRejectedMove",
        "OptimizerAdvancesThresholdLevelsAndStopsAtMinimum",
        "StableIdCatalogAndDescriptorExposeThresholdAccepting"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        "ThresholdAccepting",
        '"threshold-accepting-dueck-scheuer-1990"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        '"threshold-accepting-dueck-scheuer-1990"',
        "ThresholdAcceptingOptimizer<TSolution,TMove,TUndo>",
        "10.1016/0021-9991(90)90201-B"
    )

$catalog =
    (Read-Utf8 "docs\threshold-accepting-schedule-catalog.json") |
    ConvertFrom-Json

$implemented =
    @($catalog.entries |
      Where-Object status -eq "implemented")

$deferred =
    @($catalog.entries |
      Where-Object status -eq "reviewed-deferred")

if ($implemented.Count -ne 3) {
    throw "Threshold Accepting validation: expected 3 executable monotone threshold schedules."
}

if ($deferred.Count -ne 1 -or
    [string]$deferred[0].id -ne "ta.acceptance.old-bachelor") {
    throw "Threshold Accepting validation: Old Bachelor Acceptance must remain the single reviewed/deferred non-monotone controller."
}

foreach ($entry in $implemented) {
    if ([string]$entry.formulaMode -ne "math" -or
        [string]::IsNullOrWhiteSpace([string]$entry.formula)) {
        throw "Threshold Accepting validation: implemented schedule '$($entry.id)' lacks mathematics."
    }
}

Require-Contains `
    "docs\pages\components\threshold-accepting-schedules.md" @(
        "@page threshold_accepting_schedules",
        "## Deterministic acceptance rule",
        "## Implemented monotone threshold schedules",
        "## Reviewed but intentionally deferred: Old Bachelor Acceptance",
        "10.1016/0021-9991(90)90201-B",
        "10.1137/S0036142995286076",
        "10.1287/ijoc.7.4.417",
        "\f["
    )

Require-Contains `
    "docs\pages\algorithms\threshold-accepting-dueck-scheuer-1990.md" @(
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
        "@subpage threshold_accepting_schedules",
        "threshold-accepting-dueck-scheuer-1990"
    )

Require-Contains `
    "docs\Build-ThresholdAcceptingScheduleDocumentation.ps1" @(
        "Threshold Accepting Schedules",
        "threshold-accepting-schedules.html",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\build-documentation.ps1" @(
        "Build-ThresholdAcceptingScheduleDocumentation.ps1"
    )

Require-Contains `
    "README.md" @(
        "23 public algorithms",
        "14 trajectory methods",
        "threshold-accepting-dueck-scheuer-1990",
        "components/threshold-accepting-schedules.html"
    )

Write-Host `
    "Threshold Accepting validation passed: canonical deterministic acceptance + 3 executable monotone threshold schedules + Old Bachelor Acceptance reviewed/deferred." `
    -ForegroundColor Green