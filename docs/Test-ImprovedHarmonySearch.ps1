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
        throw "Improved Harmony Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version][string]$version.version -lt [version]"0.56.0") {
    throw "Improved Harmony Search validation requires repository version 0.56.0 or later."
}

$requiredFiles =
    @(
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ImprovedHarmonySearchOptimizer.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ImprovedHarmonySearchParameters.cs",
        "tests\MetaheuristicsPlatform.Tests\ImprovedHarmonySearchTests.cs",
        "benchmarks\MetaheuristicsPlatform.Benchmarks\ImprovedHarmonySearchBenchmarks.cs",
        "docs\pages\algorithms\improved-harmony-search-mahdavi-fesanghary-damangir-2007.md"
    )

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative) -PathType Leaf)) {
        throw "Improved Harmony Search validation: required file missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ImprovedHarmonySearchOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ImprovedHarmonySearchParameters.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\ImprovedHarmonySearchTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\improved-harmony-search-mahdavi-fesanghary-damangir-2007.md"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.ImprovedHarmonySearch",
    "HarmonySearchReferences.MahdaviFesangharyDamangir2007",
    "MinimumPitchAdjustmentRate",
    "MaximumPitchAdjustmentRate",
    "MinimumPitchAdjustmentBandwidth",
    "MaximumPitchAdjustmentBandwidth",
    "GetPitchAdjustmentRate(",
    "GetPitchAdjustmentBandwidth(",
    "MaximumImprovedHarmonySearchImprovisations",
    "FindWorstIndex("
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker)) {

        throw "Improved Harmony Search validation: implementation marker '$marker' is missing."
    }
}

foreach ($formulaMarker in @(
    "MinimumPitchAdjustmentRate +",
    "MaximumPitchAdjustmentRate -",
    "Math.Exp(",
    "Math.Log(",
    "MinimumPitchAdjustmentBandwidth /",
    "MaximumPitchAdjustmentBandwidth"
)) {
    if (-not $parameters.Contains($formulaMarker)) {
        throw "Improved Harmony Search validation: schedule marker '$formulaMarker' is missing."
    }
}

foreach ($testMarker in @(
    "DynamicSchedulesMatchPublishedEquations",
    "OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization",
    "EvaluationBudgetMayStopDuringHarmonyMemoryInitialization",
    "MaximizationUsesObjectiveSenseSymmetrically",
    "SameSeedProducesSameResult",
    "FactoryCreatesImprovedHarmonySearchAndCanonicalHsRemainsDistinct"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "Improved Harmony Search validation: focused test '$testMarker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$ihs =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "improved-harmony-search-mahdavi-fesanghary-damangir-2007"
        }
    )

if ($ihs.Count -ne 1) {
    throw "Improved Harmony Search validation: public catalog identity must occur exactly once."
}

if ([string]$ihs[0].doi -ne "10.1016/j.amc.2006.11.033") {
    throw "Improved Harmony Search validation: primary DOI mismatch."
}

if ([string]$ihs[0].category -ne "other-metaheuristics") {
    throw "Improved Harmony Search validation: documentary family mismatch."
}

if (-not ([string]$ihs[0].update).Contains("\begin{aligned}")) {
    throw "Improved Harmony Search validation: aligned mathematical update block is missing."
}

$canonical =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "harmony-search-geem-kim-loganathan-2001"
        }
    )

if ($canonical.Count -ne 1) {
    throw "Improved Harmony Search validation: canonical 2001 HS identity must remain exactly once."
}

$ghs =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "global-best-harmony-search-omran-mahdavi-2008"
        }
    )

if ($ghs.Count -ne 1) {
    throw "Improved Harmony Search validation: GHS must remain a separate public identity."
}

if ($optimizer.Contains("GlobalBest") -or
    $parameters.Contains("GlobalBest")) {
    throw "Improved Harmony Search validation: IHS must not absorb GHS global-best mechanics."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {

    throw "Improved Harmony Search validation: doubled Doxygen/TeX command escaping detected."
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
    "10.1016/j.amc.2006.11.033",
    "PAR_{\min}",
    "PAR_{\max}",
    "bw_{\min}",
    "bw_{\max}",
    "fixed HMCR",
    "GHS is a separate public identity since v0.57.0"
)) {
    if (-not $page.Contains($marker)) {
        throw "Improved Harmony Search validation: page marker '$marker' is missing."
    }
}

Write-Host `
    "Improved Harmony Search validation passed: Mahdavi-Fesanghary-Damangir 2007 PAR/bw schedules executable; HS and GHS remain separate identities." `
    -ForegroundColor Green
