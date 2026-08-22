[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if(-not (Test-Path -LiteralPath $path)) {
        throw "Advanced Scatter Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative

    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Advanced Scatter Search validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if([version]([string]$version.version) -lt [version]"0.40.0") {
    throw "Advanced Scatter Search validation: expected version 0.40.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\AdvancedScatterSearchStrategies.cs" @(
        "ScatterSearchReferenceSetRefreshMode",
        "DynamicImmediate",
        "IScatterSearchReferenceSetRebuildingMethod",
        "TwoTierScatterSearchReferenceSetUpdateMethod",
        "MinimumQualityDistance",
        "MinimumDistanceExcluding",
        "MaxMinScatterSearchReferenceSetRebuildingMethod",
        "qualityTier",
        "GloverScatterSearchSubsetGenerationMethod",
        "FindBestOutside"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ScatterSearchComponentIds.cs" @(
        "ss.refset.update.dynamic-refresh",
        "ss.refset.update.two-tier",
        "ss.refset.rebuild.max-min",
        "ss.diversity.minimum-distance",
        "ss.subsets.glover-types-1-4",
        "ss.refset.update.three-tier-good-generators",
        "ss.diversity.hashing"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ScatterSearchOptimizer.cs" @(
        "_referenceSetRebuilding",
        "ReferenceSetRefreshMode",
        "DynamicImmediate",
        "MaximumReferenceSetRebuilds",
        "RebuildDiversificationPopulationSize",
        "TryRebuild"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\ScatterSearch\ScatterSearchParameters.cs" @(
        "ReferenceSetRefreshMode",
        "MaximumReferenceSetRebuilds",
        "RebuildDiversificationPopulationSize"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\AdvancedScatterSearchTests.cs" @(
        "AdvancedComponentIdsAreStable",
        "TwoTierUpdateImprovesQualityTier",
        "TwoTierUpdateCanImproveDiversityTierWithoutImprovingQuality",
        "MinimumDiversityThresholdFiltersQualityTier",
        "GloverSubsetFamiliesGenerateRepresentativeTypesOneToFour",
        "MaxMinRebuildPreservesQualityTierAndRefreshesDiversityTier",
        "DynamicRefreshCombinesNewReferenceSolutionBeforeStaleScheduleContinues",
        "DiversityReplacementExcludesTheMemberBeingReplaced",
        "RebuildPreservesBestQualityMembersRegardlessOfStorageOrder"
    )

$catalog =
    (Read-Utf8 "docs\advanced-scatter-search-catalog.json") |
    ConvertFrom-Json

$entries = @($catalog.entries)
$implemented = @($entries | Where-Object status -eq "implemented")
$deferred = @($entries | Where-Object status -eq "reviewed/deferred")

if($implemented.Count -ne 5) {
    throw "Advanced Scatter Search validation: expected exactly 5 implemented entries."
}

if($deferred.Count -ne 6) {
    throw "Advanced Scatter Search validation: expected exactly 6 reviewed/deferred entries."
}

if([string]$catalog.algorithmId -ne "scatter-search-marti-laguna-glover-2006") {
    throw "Advanced Scatter Search validation: canonical public Scatter Search ID changed."
}

Require-Contains `
    "docs\pages\components\advanced-scatter-search-strategies.md" @(
        "@page advanced_scatter_search_strategies",
        "ss.refset.update.dynamic-refresh",
        "ss.refset.update.two-tier",
        "ss.refset.rebuild.max-min",
        "ss.diversity.minimum-distance",
        "ss.subsets.glover-types-1-4",
        "10.1016/j.ejor.2004.08.004",
        "10.1007/978-1-4615-0337-8",
        "10.1007/978-3-540-39930-8_4"
    )

Require-Contains `
    "docs\Build-AdvancedScatterSearchDocumentation.ps1" @(
        "Advanced Scatter Search Strategies",
        "advanced-scatter-search-strategies.html",
        "advanced-scatter-search-catalog.json",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Write-Host `
    "Advanced Scatter Search validation passed: 5 executable generic ss.* components + 6 scientifically reviewed/deferred advanced designs; canonical public algorithm count unchanged." `
    -ForegroundColor Green
