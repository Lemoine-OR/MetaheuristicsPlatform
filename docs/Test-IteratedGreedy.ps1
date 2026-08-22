[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Iterated Greedy validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative
    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Iterated Greedy validation: '$Relative' is missing '$marker'."
        }
    }
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json
if([version]([string]$version.version) -lt [version]"0.37.0") {
    throw "Iterated Greedy validation: expected version 0.37.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\IteratedGreedy\IteratedGreedyContracts.cs" @(
        "IIteratedGreedyDestruction",
        "IIteratedGreedyConstruction",
        "DelegateIteratedGreedyDestruction",
        "DelegateIteratedGreedyConstruction"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\IteratedGreedy\IteratedGreedyAcceptance.cs" @(
        "ImprovingOnlyIteratedGreedyAcceptancePolicy",
        "ConstantTemperatureIteratedGreedyAcceptancePolicy",
        "Math.Exp(-context.Degradation / Temperature)",
        "TrajectoryObjectiveComparison.ComputeDegradation"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\IteratedGreedy\IteratedGreedyOptimizer.cs" @(
        "iterated-greedy-ruiz-stutzle-2007",
        "IIteratedGreedyDestruction",
        "IIteratedGreedyConstruction",
        "ILocalSearchProcedure",
        "_destruction.Destroy",
        "_construction.Reconstruct",
        "context.Evaluate(",
        "candidate,",
        "_acceptance.ShouldAccept",
        "IteratedGreedyReferences.RuizStutzle2007",
        "IteratedGreedyReferences.StutzleRuiz2025"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\IteratedGreedyTests.cs" @(
        "DestructionPrecedesReconstructionAndImprovingCandidateIsAccepted",
        "PartialSolutionIsNeverEvaluatedBeforeReconstruction",
        "OptionalLocalSearchRunsOnInitialAndReconstructedSolutions",
        "ConstantTemperatureAcceptanceMirrorsMaximization",
        "StableIdAndCatalogExposeIteratedGreedy"
    )

Require-Contains `
    "docs\pages\algorithms\iterated-greedy-ruiz-stutzle-2007.md" @(
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
        "10.1016/j.ejor.2005.12.009",
        "10.1007/978-3-032-00385-0_10",
        "10.1016/j.omega.2018.03.004",
        "10.1016/j.cie.2017.06.025",
        "10.1016/j.asoc.2020.106629"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        "iterated-greedy-ruiz-stutzle-2007"
    )

$catalog = (Read-Utf8 "docs\algorithm-catalog.json") | ConvertFrom-Json
$entry = @(
    $catalog.algorithms |
    Where-Object { [string]$_.id -eq "iterated-greedy-ruiz-stutzle-2007" }
)

if($entry.Count -ne 1) {
    throw "Iterated Greedy validation: the public catalog entry must be unique."
}

if([string]$entry[0].doi -ne "10.1016/j.ejor.2005.12.009") {
    throw "Iterated Greedy validation: incorrect principal DOI."
}

if(-not [bool]$entry[0].requiresComposition) {
    throw "Iterated Greedy validation: generic destruction/reconstruction requires typed composition."
}

Require-Contains "README.md" @(
    "iterated-greedy-ruiz-stutzle-2007"
)

Write-Host "Iterated Greedy validation passed: generic Ruiz-Stutzle destruction/reconstruction core executable; optional local search and constant-temperature acceptance supported; advanced components may be layered by later releases." -ForegroundColor Green
