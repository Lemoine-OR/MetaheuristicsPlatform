[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if(-not (Test-Path -LiteralPath $path)) {
        throw "Scatter Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative

    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Scatter Search validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if([version]([string]$version.version) -lt [version]"0.39.0") {
    throw "Scatter Search validation: expected version 0.39.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ScatterSearchContracts.cs" @(
        "IScatterSearchDiversificationGenerationMethod",
        "IScatterSearchImprovementMethod",
        "IScatterSearchReferenceSetUpdateMethod",
        "IScatterSearchSubsetGenerationMethod",
        "IScatterSearchSolutionCombinationMethod",
        "IScatterSearchDistance"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ClassicalScatterSearchReferenceSetUpdateMethod.cs" @(
        "qualityReferenceSetSize",
        "MinimumDistance",
        "DuplicateDistanceTolerance",
        "sense.IsBetter",
        "solutionCloner.Clone"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\PairwiseNewScatterSearchSubsetGenerationMethod.cs" @(
        "referenceSet[i].IsNew",
        "referenceSet[j].IsNew",
        "ScatterSearchSubset"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ScatterSearchOptimizer.cs" @(
        "scatter-search-marti-laguna-glover-2006",
        "MetaheuristicSolutionModel.Population",
        "MetaheuristicFamily.Evolutionary",
        "MetaheuristicMechanism.MemoryBased",
        "_diversification.Generate",
        "_referenceSetUpdate.Initialize",
        "_subsetGeneration.Generate",
        "_combination.Combine",
        "_referenceSetUpdate.TryUpdate",
        "ReferenceSetStable"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ScatterSearchReferences.cs" @(
        "Rafael Martí; Manuel Laguna; Fred Glover",
        "10.1016/j.ejor.2004.08.004",
        "10.1007/978-1-4615-0337-8",
        "10.1007/978-3-540-39930-8_4"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\ScatterSearchTests.cs" @(
        "InitialReferenceSetBlendsQualityAndMaxMinDiversity",
        "PairwiseSubsetGenerationUsesAtLeastOneNewReferencePoint",
        "StrictlyBetterDistinctCandidateReplacesWorstReferencePoint",
        "ReferenceSetUpdateMirrorsMaximization",
        "StableReferenceSetTerminatesWithoutBurningMaximumIterations",
        "NonFiniteDistanceIsRejected",
        "CombinationAliasingCannotMutateReferenceSetThroughImprovement"
    )

Require-Contains `
    "docs\pages\algorithms\scatter-search-marti-laguna-glover-2006.md" @(
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
        "scatter-search-marti-laguna-glover-2006",
        "10.1016/j.ejor.2004.08.004",
        "Advanced Scatter Search"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        'ScatterSearch =',
        '"scatter-search-marti-laguna-glover-2006"'
    )

Require-Contains `
    "README.md" @(
        "Scatter Search",
        "scatter-search-marti-laguna-glover-2006"
    )

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$entry =
    @($catalog.algorithms |
        Where-Object {
            [string]$_.id -eq "scatter-search-marti-laguna-glover-2006"
        })

if($entry.Count -ne 1) {
    throw "Scatter Search validation: expected exactly one algorithm-catalog entry."
}

if([string]$entry[0].category -ne "evolutionary-methods" -or
   [string]$entry[0].factoryMode -ne "registration" -or
   -not [bool]$entry[0].requiresComposition) {
    throw "Scatter Search validation: catalog classification/factory mode is incorrect."
}

Write-Host "Scatter Search validation passed: five-method foundation + quality/diversity RefSet + pairwise-new subset generation + stable composition ID." -ForegroundColor Green
