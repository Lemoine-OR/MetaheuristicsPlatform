[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path =
        Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Global-best Harmony Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version][string]$version.version -lt [version]"0.57.0") {
    throw "Global-best Harmony Search validation requires repository version 0.57.0 or later."
}

$requiredFiles =
    @(
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\GlobalBestHarmonySearchOptimizer.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\GlobalBestHarmonySearchParameters.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\GlobalBestHarmonySearchState.cs",
        "tests\MetaheuristicsPlatform.Tests\GlobalBestHarmonySearchTests.cs",
        "benchmarks\MetaheuristicsPlatform.Benchmarks\GlobalBestHarmonySearchBenchmarks.cs",
        "docs\pages\algorithms\global-best-harmony-search-omran-mahdavi-2008.md"
    )

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative) -PathType Leaf)) {
        throw "Global-best Harmony Search validation: required file missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\GlobalBestHarmonySearchOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\GlobalBestHarmonySearchParameters.cs"

$state =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\GlobalBestHarmonySearchState.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\GlobalBestHarmonySearchTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\global-best-harmony-search-omran-mahdavi-2008.md"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.GlobalBestHarmonySearch",
    "HarmonySearchReferences.OmranMahdavi2008",
    "GetPitchAdjustmentRate(",
    "FindBestIndex(",
    "globalBestCoordinate",
    "random.NextInt32(",
    "destination.Length",
    "harmonyMemory[bestIndex][globalBestCoordinate]",
    "searchSpace.Clamp(",
    "MaximumGlobalBestHarmonySearchImprovisations"
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker)) {

        throw "Global-best Harmony Search validation: implementation marker '$marker' is missing."
    }
}

if ($parameters.Contains("Bandwidth") -or
    $optimizer.Contains("PitchAdjustmentBandwidth") -or
    $state.Contains("Bandwidth")) {

    throw "Global-best Harmony Search validation: GHS must not reintroduce a bandwidth parameter/state."
}

foreach ($testMarker in @(
    "DynamicParScheduleMatchesPublishedEquation",
    "PublicParametersContainNoBandwidth",
    "OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization",
    "EvaluationBudgetMayStopDuringHarmonyMemoryInitialization",
    "MaximizationUsesObjectiveSenseSymmetrically",
    "SameSeedProducesSameResult",
    "FactoryCreatesThreeDistinctHarmonySearchIdentities"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "Global-best Harmony Search validation: focused test '$testMarker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

foreach ($identity in @(
    "harmony-search-geem-kim-loganathan-2001",
    "improved-harmony-search-mahdavi-fesanghary-damangir-2007",
    "global-best-harmony-search-omran-mahdavi-2008"
)) {
    $matches =
        @(
            $catalog.algorithms |
            Where-Object {
                [string]$_.id -eq $identity
            }
        )

    if ($matches.Count -ne 1) {
        throw "Global-best Harmony Search validation: identity '$identity' must occur exactly once."
    }
}

$ghs =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "global-best-harmony-search-omran-mahdavi-2008"
        }
    )[0]

if ([string]$ghs.doi -ne "10.1016/j.amc.2007.09.004") {
    throw "Global-best Harmony Search validation: primary DOI mismatch."
}

if ([string]$ghs.category -ne "other-metaheuristics") {
    throw "Global-best Harmony Search validation: documentary family mismatch."
}

if (-not ([string]$ghs.update).Contains("\begin{aligned}")) {
    throw "Global-best Harmony Search validation: aligned mathematical update block is missing."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {

    throw "Global-best Harmony Search validation: doubled Doxygen/TeX command escaping detected."
}

foreach ($marker in @(
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
    "10.1016/j.amc.2007.09.004",
    "x_i^{new}=x_k^{best}",
    "randomly selected decision-variable index",
    "no bandwidth parameter",
    "platform boundary repair"
)) {
    if (-not $page.Contains($marker)) {
        throw "Global-best Harmony Search validation: page marker '$marker' is missing."
    }
}

Write-Host `
    "Global-best Harmony Search validation passed: Omran-Mahdavi 2008 dynamic PAR + bandwidth-free cross-coordinate global-best pitch executable; HS/IHS identities preserved." `
    -ForegroundColor Green
