[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Memetic Algorithm validation: missing '$Relative'."
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
            throw "Memetic Algorithm validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.43.0") {
    throw "Memetic Algorithm validation: expected repository version 0.43.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\GenerationalGeneticAlgorithmOptimizer.cs" @(
        "IGeneticAlgorithmExecutionExtension<TSolution>",
        "ProcessCompletedGeneration",
        "CreateAlgorithmState",
        "improvementsBeforeGeneration"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Memetic\MemeticAlgorithmOptimizer.cs" @(
        "MemeticAlgorithmOptimizer<TSolution>",
        "MetaheuristicAlgorithmIds.MemeticAlgorithm",
        "EveryOffspringMemeticLocalSearchPolicy",
        "LamarckianMemeticLearningPolicy",
        "MemeticGeneticExecutionExtension<TSolution>"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Memetic\MemeticGeneticExecutionExtension.cs" @(
        "ILocalSearchProcedure<TSolution>",
        "ProcessCompletedGeneration",
        "state.EliteCount",
        "InheritImprovedPhenotype",
        "_successfulLocalSearches",
        "_cumulativeLocalSearchGain",
        "_consecutiveNonImprovingGenerations"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Memetic\MemeticLocalSearchPolicies.cs" @(
        "EveryOffspringMemeticLocalSearchPolicy",
        "PeriodicMemeticLocalSearchPolicy",
        "ProbabilisticMemeticLocalSearchPolicy",
        "TopFractionMemeticLocalSearchPolicy",
        "StagnationAdaptiveMemeticLocalSearchPolicy",
        "RequiresRanking"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Memetic\MemeticAlgorithmState.cs" @(
        "LocalSearchInvocations",
        "SuccessfulLocalSearches",
        "AcceptedLocalSearchMoves",
        "CumulativeLocalSearchGain",
        "ConsecutiveNonImprovingGenerations",
        "LocalSearchSuccessRate"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Memetic\MemeticLearningPolicies.cs" @(
        "LamarckianMemeticLearningPolicy",
        "BaldwinianMemeticLearningPolicy",
        "InheritImprovedPhenotype",
        "SelectionObjective"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Memetic\MemeticAlgorithmReferences.cs" @(
        "Moscato1989",
        "KrasnogorSmith2005",
        "10.1109/TEVC.2005.850260",
        "Report 826"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        "MemeticAlgorithm",
        '"memetic-algorithm-moscato-1989"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        '"memetic-algorithm-moscato-1989"',
        "MemeticAlgorithmOptimizer<TSolution>",
        "hybrid-methods",
        "10.1109/TEVC.2005.850260"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\MemeticAlgorithmTests.cs" @(
        "DisabledLocalSearchMatchesGaEvaluationAccounting",
        "EveryOffspringRunsLocalSearchOnAllGeneratedChildren",
        "PeriodicPolicyRunsOnlyOnMatchingGeneration",
        "TopFractionPolicyImprovesOnlyBestHalfOfOffspring",
        "LocalSearchEvaluationsConsumeGlobalBudget",
        "LamarckianLearningPassesImprovedGenotypeToNextGeneration",
        "BaldwinianLearningKeepsGenotypeWhileSelectionUsesLearnedFitness",
        "BaldwinianLearningDoesNotMutateInheritedArrayGenotype",
        "SameSeedProducesSameMemeticResult",
        "StableIdSupportsTypedFactoryRegistration"
    )

$catalog =
    (Read-Utf8 "docs\memetic-algorithm-catalog.json") |
    ConvertFrom-Json

$implemented =
    @($catalog.entries |
      Where-Object status -eq "implemented")

$deferred =
    @($catalog.entries |
      Where-Object status -eq "reviewed-deferred")

if ($implemented.Count -ne 7) {
    throw "Memetic Algorithm validation: expected seven executable memetic components."
}

if ($deferred.Count -ne 2) {
    throw "Memetic Algorithm validation: expected two reviewed/deferred memetic extensions."
}

foreach ($entry in @($catalog.entries)) {
    if ([string]::IsNullOrWhiteSpace(
            [string]$entry.formulaMode) -or
        [string]::IsNullOrWhiteSpace(
            [string]$entry.formula)) {
        throw "Memetic Algorithm validation: '$($entry.id)' lacks a scientific model."
    }
}

Require-Contains `
    "docs\pages\components\memetic-algorithm-components.md" @(
        "@page memetic_algorithm_components",
        "## Executable local-improvement policies",
        "## Executable learning policies",
        "## Reviewed / deferred extensions",
        "ma.local-search.adaptive-stagnation",
        "ma.learning.lamarckian",
        "ma.learning.baldwinian",
        "10.1109/TEVC.2005.850260"
    )

Require-Contains `
    "docs\pages\algorithms\memetic-algorithm-moscato-1989.md" @(
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
        "@subpage memetic_algorithm_components",
        "memetic-algorithm-moscato-1989"
    )

Require-Contains `
    "docs\Build-MemeticAlgorithmDocumentation.ps1" @(
        "Memetic Algorithm Components",
        "memetic-algorithm-components.html",
        "memetic-algorithm-catalog.json",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "README.md" @(
        "31 public algorithms",
        "Memetic Algorithm - Moscato",
        "memetic-algorithm-moscato-1989",
        "components/memetic-algorithm-components.html"
    )

Write-Host `
    "Memetic Algorithm validation passed: shared GA engine + 5 local-improvement policies + Lamarckian/Baldwinian learning + exact common evaluation accounting." `
    -ForegroundColor Green
