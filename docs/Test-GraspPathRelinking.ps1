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
    return [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative, [string[]]$Markers) {
    $text = Read-Utf8 $Relative
    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            throw "GRASP Path Relinking validation: '$Relative' is missing '$marker'."
        }
    }
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json
if ([version]([string]$version.version) -lt [version]"0.31.0") {
    throw "GRASP Path Relinking validation: expected repository version 0.31.0 or later."
}

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Constructive\AdvancedPathRelinkingProcedure.cs" @(
    "IAdvancedPathRelinkingProcedure<TSolution>",
    "PathRelinkingDirectionStrategy.Backward",
    "PathRelinkingDirectionStrategy.BackAndForward",
    "PathRelinkingDirectionStrategy.Mixed",
    "PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive",
    "ArrayPool<CandidateProbe>",
    "RegisterExternalProbeEvaluation",
    "PromoteOwnedExternalProbeSnapshot",
    "strictly decrease",
    "PathRelinkingTruncatedFractionCompleted"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Constructive\PathRelinkingExecutionOptions.cs" @(
    "Forward",
    "Backward",
    "BackAndForward",
    "Mixed",
    "GreedyRandomizedAdaptive",
    "PathFraction",
    "GreedyRandomizedAlpha",
    "IsCanonicalGreedyForward"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspPathRelinkingOptimizer.cs" @(
    'Id = "grasp-path-relinking"',
    "IAdvancedPathRelinkingProcedure<TSolution>",
    "CreatePathRelinkingExecutionOptions",
    "guidingFitness",
    "RelinkAdvanced",
    "RibeiroResende2012"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Constructive\EliteSolutionPool.cs" @(
    "out double guidingFitness",
    "guidingFitness = _fitness[selectedIndex]",
    "Reservoir"
)

Require-Contains "tests\MetaheuristicsPlatform.Tests\GraspPathRelinkingTests.cs" @(
    "AdvancedBackwardRelinkingUsesKnownEliteFitnessAndReachesOppositeEndpoint",
    "AdvancedBackAndForwardTraversesBothDirections",
    "AdvancedMixedRelinkingAlternatesEndpointsUntilTheyMeet",
    "TruncatedPathRelinkingStopsAfterRequestedDistanceFraction",
    "GreedyRandomizedAdaptivePathRelinkingUsesRclSampling",
    "ElitePoolCanReturnStoredGuideFitnessWithoutReevaluation",
    "AdvancedParametersRejectInvalidPathFractionAndRclAlpha"
)

$catalog = (Read-Utf8 "docs\path-relinking-strategy-catalog.json") | ConvertFrom-Json
if (@($catalog.implemented).Count -ne 6) {
    throw "GRASP Path Relinking validation: expected 6 implemented advanced strategy entries."
}
if (@($catalog.reviewedDeferred).Count -ne 1 -or
    [string]$catalog.reviewedDeferred[0].id -ne "pr.evolutionary") {
    throw "GRASP Path Relinking validation: evolutionary path relinking must remain explicitly reviewed/deferred."
}
foreach ($entry in @($catalog.implemented)) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.formula) -or
        [string]$entry.formulaMode -ne "math") {
        throw "GRASP Path Relinking validation: implemented entry '$($entry.id)' lacks a mathematical model."
    }
}

Require-Contains "docs\pages\components\path-relinking-strategies.md" @(
    "@page path_relinking_strategies",
    "## Direction policies",
    "## Greedy-randomized adaptive move selection",
    "## Truncated path relinking",
    "## Evolutionary path relinking",
    "10.1007/s10732-011-9167-1",
    "\f["
)

Require-Contains "docs\pages\algorithms\grasp-path-relinking.md" @(
    "AdvancedPathRelinkingProcedure",
    "PathDirection",
    "PathMoveSelection",
    "PathFraction",
    "PathRelinkingAlpha",
    "@subpage path_relinking_strategies",
    "10.1007/s10732-011-9167-1"
)

Require-Contains "docs\Build-PathRelinkingStrategyDocumentation.ps1" @(
    "Advanced Path Relinking Strategies",
    "path-relinking-strategies.html",
    "formula-note",
    "mathjax@3.2.2/es5/tex-chtml.js"
)

Require-Contains "docs\build-documentation.ps1" @(
    "Build-PathRelinkingStrategyDocumentation.ps1"
)

Require-Contains "README.md" @(
    "components/path-relinking-strategies.html",
    "Evolutionary Path Relinking"
)
Write-Host "GRASP Path Relinking validation passed: 6 executable advanced pairwise strategies + evolutionary PR reviewed/deferred." -ForegroundColor Green