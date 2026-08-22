[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if(-not (Test-Path -LiteralPath $path)) {
        throw "Advanced Genetic Algorithm validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative

    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Advanced Genetic Algorithm validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if([version]([string]$version.version) -lt [version]"0.42.0") {
    throw "Advanced Genetic Algorithm validation: expected version 0.42.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\GeneticAlgorithmComponentIds.cs" @(
        "ga.selection.tournament",
        "ga.selection.truncation",
        "ga.selection.linear-ranking",
        "ga.selection.exponential-ranking",
        "ga.selection.fitness-proportionate-explicit-weights",
        "ga.crossover.one-point",
        "ga.crossover.two-point",
        "ga.crossover.uniform",
        "ga.crossover.pmx",
        "ga.crossover.ox1",
        "ga.crossover.sbx-bounded",
        "ga.mutation.bit-flip",
        "ga.mutation.integer-random-reset",
        "ga.mutation.swap",
        "ga.mutation.inversion",
        "ga.mutation.gaussian-bounded",
        "ga.mutation.polynomial-bounded",
        "ga.replacement.generational-elitist",
        "ga.replacement.steady-state"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\AdvancedGeneticSelectionMethods.cs" @(
        "TruncationGeneticParentSelectionMethod",
        "LinearRankingGeneticParentSelectionMethod",
        "ExponentialRankingGeneticParentSelectionMethod",
        "ExplicitFitnessProportionateGeneticParentSelectionMethod",
        "RankBestFirst",
        "SampleWeights"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\SequenceGeneticCrossoverMethods.cs" @(
        "OnePointGeneticCrossoverMethod",
        "TwoPointGeneticCrossoverMethod",
        "UniformGeneticCrossoverMethod",
        "ExchangeProbability"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\PermutationGeneticCrossoverMethods.cs" @(
        "PartiallyMappedGeneticCrossoverMethod",
        "OrderGeneticCrossoverMethod",
        "unique alleles",
        "SetEquals"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\RealGeneticCrossoverMethods.cs" @(
        "BoundedSimulatedBinaryGeneticCrossoverMethod",
        "DistributionIndex",
        "PerVariableCrossoverProbability",
        "Math.Clamp"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\GeneticMutationMethods.cs" @(
        "BitFlipGeneticMutationMethod",
        "IntegerRandomResetGeneticMutationMethod",
        "SwapGeneticMutationMethod",
        "InversionGeneticMutationMethod",
        "BoundedGaussianGeneticMutationMethod",
        "BoundedPolynomialGeneticMutationMethod",
        "NextStandardNormal"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\GeneticAlgorithm\AdvancedGeneticAlgorithmReferences.cs" @(
        "10.1016/B978-0-08-050684-5.50008-2",
        "Uniform Crossover in Genetic Algorithms",
        "10.5555/645512.657265",
        "A Study of Reproduction in Generational and Steady-State Genetic Algorithms",
        "10.1016/B978-0-08-050684-5.50009-4",
        "10.5555/645511.657095",
        "10.5555/1625135.1625164",
        "Simulated Binary Crossover for Continuous Search Space",
        "10.1109/4235.996017",
        "10.1504/IJAISC.2014.059280"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\AdvancedGeneticAlgorithmTests.cs" @(
        "ComponentIdsAreStableAndDistinct",
        "TruncationSelectionHonorsMinimization",
        "LinearRankingSupportsMaximization",
        "ExplicitFitnessProportionateRejectsInvalidWeights",
        "PmxPreservesPermutationAndCopiedSegment",
        "OrderCrossoverPreservesPermutationAndSegment",
        "BoundedSbxKeepsChildrenInsideBounds",
        "IntegerRandomResetStaysInsideBoundsAndChangesSelectedGene",
        "IntegerRandomResetRejectsIntervalWiderThanRandomContract",
        "BoundedGaussianMutationProjectsToBounds",
        "BoundedPolynomialMutationStaysInsideBounds",
        "AdvancedReferencesKeepVerifiedDoisAndExplicitNulls"
    )

$catalog =
    (Read-Utf8 "docs\advanced-genetic-algorithm-catalog.json") |
    ConvertFrom-Json

$entries = @($catalog.entries)
$implemented = @($entries | Where-Object status -eq "implemented")
$deferred = @($entries | Where-Object status -eq "reviewed/deferred")

if($implemented.Count -ne 18) {
    throw "Advanced Genetic Algorithm validation: expected exactly 18 implemented component entries."
}

if($deferred.Count -ne 1 -or
   [string]$deferred[0].id -ne "ga.replacement.steady-state") {
    throw "Advanced Genetic Algorithm validation: steady-state must be the unique reviewed/deferred entry."
}

if([string]$catalog.algorithmId -ne "genetic-algorithm-generational" -or
   [int]$catalog.publicAlgorithmCountDelta -ne 0) {
    throw "Advanced Genetic Algorithm validation: canonical public GA identity changed."
}

$ids = @($entries | ForEach-Object { [string]$_.id })

if(@($ids | Select-Object -Unique).Count -ne $ids.Count) {
    throw "Advanced Genetic Algorithm validation: duplicate component ID."
}

foreach($id in $ids) {
    if(-not $id.StartsWith("ga.")) {
        throw "Advanced Genetic Algorithm validation: non-ga component ID '$id'."
    }
}

$fitnessProportionate =
    @($entries | Where-Object id -eq "ga.selection.fitness-proportionate-explicit-weights")

if($fitnessProportionate.Count -ne 1 -or
   [string]$fitnessProportionate[0].reference -ne "Goldberg & Deb (1991)" -or
   [string]$fitnessProportionate[0].doi -ne "10.1016/B978-0-08-050684-5.50008-2") {
    throw "Advanced Genetic Algorithm validation: fitness-proportionate selection provenance mismatch."
}

$steadyState =
    @($entries | Where-Object id -eq "ga.replacement.steady-state")

if($steadyState.Count -ne 1 -or
   [string]$steadyState[0].reference -ne "Syswerda (1991)" -or
   [string]$steadyState[0].doi -ne "10.1016/B978-0-08-050684-5.50009-4") {
    throw "Advanced Genetic Algorithm validation: steady-state lifecycle provenance mismatch."
}

Require-Contains `
    "docs\pages\components\advanced-genetic-algorithm-operators.md" @(
        "@page advanced_genetic_algorithm_operators",
        "ga.selection.linear-ranking",
        "ga.crossover.pmx",
        "ga.crossover.ox1",
        "ga.crossover.sbx-bounded",
        "ga.mutation.polynomial-bounded",
        "ga.replacement.steady-state",
        "10.1162/EVCO.1996.4.4.361",
        "10.5555/645512.657265",
        "10.1016/B978-0-08-050684-5.50009-4",
        "10.1109/4235.996017",
        "10.1504/IJAISC.2014.059280",
        "No DOI is asserted"
    )

Require-Contains `
    "docs\Build-AdvancedGeneticAlgorithmDocumentation.ps1" @(
        "Advanced Genetic Algorithm Operators",
        "advanced-genetic-algorithm-operators.html",
        "advanced-genetic-algorithm-catalog.json",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\pages\algorithms\genetic-algorithm-generational.md" @(
        "@subpage advanced_genetic_algorithm_operators"
    )

$algorithmCatalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$gaEntries =
    @($algorithmCatalog.algorithms |
      Where-Object id -eq "genetic-algorithm-generational")

if($gaEntries.Count -ne 1) {
    throw "Advanced Genetic Algorithm validation: canonical GA public catalog entry count changed."
}

Write-Host `
    "Advanced Genetic Algorithm validation passed: 18 executable ga.* components + steady-state replacement reviewed/deferred; canonical public GA ID unchanged." `
    -ForegroundColor Green
