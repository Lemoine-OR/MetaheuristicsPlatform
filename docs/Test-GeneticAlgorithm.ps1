[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if(-not (Test-Path -LiteralPath $path)) {
        throw "Genetic Algorithm validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains(
    [string]$Relative,
    [string[]]$Markers) {

    $text = Read-Utf8 $Relative

    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Genetic Algorithm validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if([version]([string]$version.version) -lt [version]"0.41.0") {
    throw "Genetic Algorithm validation: expected version 0.41.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        "genetic-algorithm-generational"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        "genetic-algorithm-generational",
        "GenerationalGeneticAlgorithmOptimizer<TSolution>",
        "10.1007/978-3-662-05094-1_3"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\GeneticAlgorithmContracts.cs" @(
        "IGeneticPopulationInitializer",
        "IGeneticParentSelectionMethod",
        "IGeneticCrossoverMethod",
        "IGeneticMutationMethod",
        "GeneticOffspringPair",
        "DelegateGeneticPopulationInitializer",
        "DelegateGeneticCrossoverMethod",
        "DelegateGeneticMutationMethod"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\TournamentGeneticParentSelectionMethod.cs" @(
        "TournamentSize",
        "sampling with replacement",
        "sense.IsBetter"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\GenerationalGeneticAlgorithmOptimizer.cs" @(
        "MetaheuristicAlgorithmIds.GeneticAlgorithm",
        "CopyElites",
        "ShouldApply",
        "CrossoverProbability",
        "MutationProbability",
        "MaximumGenerations",
        "solutionCloner.Clone",
        "context.CompleteIteration"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\GeneticAlgorithmReferences.cs" @(
        "10.1007/978-3-662-05094-1_3",
        "10.1007/BF00175354",
        "10.1162/EVCO.1996.4.4.361"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\GeneticAlgorithmTests.cs" @(
        "TournamentSelectionHonorsMinimization",
        "TournamentSelectionHonorsMaximization",
        "InitializationStopsAtEvaluationBudgetAndReturnsBest",
        "ElitismCopiesMembersWithoutReevaluation",
        "OddPopulationSizeIsFilledExactly",
        "ZeroCrossoverProbabilitySkipsCrossover",
        "ZeroMutationProbabilitySkipsMutation",
        "UnitProbabilitiesInvokeExpectedVariationCounts",
        "SameSeedProducesSameResult",
        "PopulationOwnsInitializerSnapshots",
        "StableIdSupportsTypedFactoryRegistration"
    )

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$ga =
    @(
        $catalog.algorithms |
        Where-Object id -eq "genetic-algorithm-generational"
    )

if($ga.Count -ne 1) {
    throw "Genetic Algorithm validation: expected exactly one canonical catalog entry."
}

if([string]$ga[0].factoryMode -ne "registration") {
    throw "Genetic Algorithm validation: composed GA must use registration factory mode."
}

if([string]$ga[0].doi -ne "10.1007/978-3-662-05094-1_3") {
    throw "Genetic Algorithm validation: principal DOI mismatch."
}

Require-Contains `
    "docs\pages\algorithms\genetic-algorithm-generational.md" @(
        "@page genetic_algorithm_generational",
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
        "genetic-algorithm-generational",
        "10.1007/978-3-662-05094-1_3",
        "10.1007/BF00175354",
        "10.1162/EVCO.1996.4.4.361"
    )

Write-Host `
    "Genetic Algorithm validation passed: generic fixed-size generational GA + tournament selection + representation-specific crossover/mutation + optional elitism + stable registration ID." `
    -ForegroundColor Green
