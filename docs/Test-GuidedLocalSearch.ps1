[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "v0.26 GLS validation: missing '$Relative'."
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
            throw "v0.26 GLS validation: '$Relative' is missing '$marker'."
        }
    }
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json
$versionText = [version]([string]$version.version)
if ($versionText -lt [version]"0.26.0") {
    throw "v0.26 GLS validation: expected repository version 0.26.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\GuidedLocalSearchContracts.cs" @(
        "IGuidedLocalSearchFeatureEnumerator",
        "IGuidedLocalSearchFeatureModel",
        "IGuidedLocalSearchPenaltyDeltaEvaluator",
        "GuidedLocalSearchState"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\GuidedLocalSearchParameters.cs" @(
        "GuidedLocalSearchParameters",
        "PenaltyWeight",
        "MaximumPenaltyUpdates",
        "MaximumAcceptedMovesPerPenaltyPhase",
        "SelectionPolicy"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\GuidedLocalSearchReferences.cs" @(
        "TsangVoudouris1997",
        "VoudourisTsang1999",
        "10.1016/S0167-6377(96)00042-9",
        "10.1016/S0377-2217(98)00099-X"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\GuidedLocalSearchOptimizer.cs" @(
        "GuidedLocalSearchOptimizer",
        'Id = "guided-local-search-voudouris-tsang-1999"',
        "FindMaximumUtilityFeatures",
        "TryEvaluateCandidatePenaltySum",
        "RegisterExternalProbeEvaluation",
        "PromoteOwnedExternalProbeSnapshot"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\GuidedLocalSearchTests.cs" @(
        "GlsPenalizationEscapesOriginalLocalOptimum",
        "GlsPenalizesAllMaximumUtilityTies",
        "GlsGeneralizesPenaltyDirectionToMaximization",
        "ExactObjectiveAndPenaltyDeltasAvoidFullCandidateEvaluation",
        "DescriptorCarriesCanonicalGuidedLocalSearchReferences"
    )

Require-Contains `
    "docs\pages\algorithms\guided-local-search-voudouris-tsang-1999.md" @(
        "## Mathematical details",
        "u_i",
        "10.1016/S0167-6377(96)00042-9",
        "10.1016/S0377-2217(98)00099-X"
    )

Require-Contains `
    "docs\guided-local-search-catalog.json" @(
        '"guided-local-search-voudouris-tsang-1999"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
        '"guided-local-search-voudouris-tsang-1999"'
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
        '"guided-local-search-voudouris-tsang-1999"'
    )

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$ids =
    @($catalog.algorithms |
      ForEach-Object { [string]$_.id })

if ($ids -notcontains
    "guided-local-search-voudouris-tsang-1999") {
    throw "v0.26 GLS validation: documentation catalog is missing GLS."
}

if (@($catalog.algorithms).Count -lt 16) {
    throw "v0.26 GLS validation: expected at least sixteen public algorithms."
}

$readme = Read-Utf8 "README.md"
foreach ($marker in @(
    "guided-local-search-voudouris-tsang-1999"
)) {
    if (-not $readme.Contains($marker)) {
        throw "v0.26 GLS validation: README is missing '$marker'."
    }
}

Write-Host `
    "Guided Local Search validation passed: canonical feature penalties + exact-delta fast paths." `
    -ForegroundColor Green
