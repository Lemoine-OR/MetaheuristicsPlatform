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
if ([version]([string]$version.version) -lt [version]"0.32.0") {
    throw "GRASP Path Relinking validation: expected repository version 0.32.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\AdvancedPathRelinkingProcedure.cs" @(
        "IAdvancedPathRelinkingProcedure<TSolution>",
        "PathRelinkingDirectionStrategy.Backward",
        "PathRelinkingDirectionStrategy.BackAndForward",
        "PathRelinkingDirectionStrategy.Mixed",
        "PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive",
        "ArrayPool<CandidateProbe>",
        "RegisterExternalProbeEvaluation",
        "PromoteOwnedExternalProbeSnapshot",
        "PathRelinkingTruncatedFractionCompleted"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\EvolutionaryPathRelinkingProcedure.cs" @(
        "EvolutionaryPathRelinkingResult<TSolution>",
        "for (int firstIndex = 0;",
        "for (int secondIndex = firstIndex + 1;",
        "CreateEmptySibling",
        "TryAddEvolutionary",
        "_localSearch.Improve",
        "EvolutionaryPathRelinkingConverged",
        "MaximumEvolutionaryPathRelinkingGenerations"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\EliteSolutionPool.cs" @(
        "TryAddEvolutionary",
        "improvesBest",
        "improvesWorst",
        "replacementIndex",
        "closestDistance",
        "TryGetBest",
        "GetAt",
        "CreateEmptySibling"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspPathRelinkingParameters.cs" @(
        "EvolutionaryPathRelinkingEnabled",
        "MaximumEvolutionaryGenerations",
        "MaximumEvolutionaryPathSteps",
        "ImproveEvolutionaryOffspring",
        "EvolutionaryPathDirection",
        "PathRelinkingDirectionStrategy.Mixed",
        "EvolutionaryPathMoveSelection",
        "PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive",
        "CreateEvolutionaryPathRelinkingExecutionOptions"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Constructive\GraspPathRelinkingOptimizer.cs" @(
        'Id = "grasp-path-relinking"',
        "EvolutionaryPathRelinkingProcedure<TSolution>",
        "EvolutionaryPathRelinkingEnabled",
        "evolutionaryProcedure.Evolve",
        "ResendeWerneck2004",
        "ResendeMartiGallegoDuarte2010",
        "EvolutionaryGenerationsCompleted"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        "optional EvPR adds O(E^2*C_PR)",
        "Resende & Werneck (2004)",
        "Resende, Marti, Gallego & Duarte (2010)",
        "EvolutionaryPathRelinkingProcedure<TSolution>"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\GraspPathRelinkingTests.cs" @(
        "EvolutionaryAdmissionReplacesClosestDominatedElite",
        "EvolutionaryAllPairsFindsInteriorImprovement",
        "OptimizerRunsEvolutionaryPathRelinkingPostOptimization",
        "EvolutionaryParametersRejectInvalidGenerationAndPathLimits",
        "DescriptorCarriesResendeWerneckEvolutionaryReference"
    )

$catalog = (Read-Utf8 "docs\path-relinking-strategy-catalog.json") | ConvertFrom-Json

if (@($catalog.implemented).Count -ne 7) {
    throw "GRASP Path Relinking validation: expected 7 implemented strategy entries."
}

if (@($catalog.reviewedDeferred).Count -ne 0) {
    throw "GRASP Path Relinking validation: no Path Relinking strategy should remain deferred in v0.32.0."
}

$evolutionary =
    @($catalog.implemented |
      Where-Object id -eq "pr.evolutionary")

if ($evolutionary.Count -ne 1 -or
    [string]$evolutionary[0].formulaMode -ne "math") {
    throw "GRASP Path Relinking validation: pr.evolutionary must be an implemented mathematical component."
}

foreach ($entry in @($catalog.implemented)) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.formula) -or
        [string]$entry.formulaMode -ne "math") {
        throw "GRASP Path Relinking validation: implemented entry '$($entry.id)' lacks a mathematical model."
    }
}

Require-Contains `
    "docs\pages\components\path-relinking-strategies.md" @(
        "@page path_relinking_strategies",
        "## Evolutionary path relinking",
        "all unordered pairs",
        "TryAddEvolutionary",
        "10.1023/B:HEUR.0000019986.96257.50",
        "10.1016/j.cor.2008.05.011",
        "10.1007/s10732-011-9167-1",
        "\f["
    )

Require-Contains `
    "docs\pages\algorithms\grasp-path-relinking.md" @(
        "EvolutionaryPathRelinkingProcedure",
        "EvolutionaryPathRelinkingEnabled",
        "MaximumEvolutionaryGenerations",
        "EvolutionaryPathDirection",
        "EvolutionaryPathMoveSelection",
        "EvolutionaryGenerationsCompleted",
        "@subpage path_relinking_strategies",
        "10.1023/B:HEUR.0000019986.96257.50"
    )

Require-Contains `
    "docs\Build-PathRelinkingStrategyDocumentation.ps1" @(
        "Seven executable path-relinking strategies",
        "generational evolutionary PR",
        "formulaMode",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "README.md" @(
        "7 executable strategies",
        "generational Evolutionary Path Relinking"
    )

Write-Host `
    "GRASP Path Relinking validation passed: 6 pairwise/path policies + generational Evolutionary Path Relinking executable." `
    -ForegroundColor Green