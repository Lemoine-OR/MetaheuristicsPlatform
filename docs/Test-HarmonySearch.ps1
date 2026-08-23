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
        throw "Harmony Search validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version][string]$version.version -lt [version]"0.55.0") {
    throw "Harmony Search validation requires repository version 0.55.0 or later."
}

$requiredFiles =
    @(
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchOptimizer.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchParameters.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchState.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchReferences.cs",
        "tests\MetaheuristicsPlatform.Tests\HarmonySearchTests.cs",
        "benchmarks\MetaheuristicsPlatform.Benchmarks\HarmonySearchBenchmarks.cs",
        "docs\pages\algorithms\harmony-search-geem-kim-loganathan-2001.md",
        "docs\pages\families\other-metaheuristics.md"
    )

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative) -PathType Leaf)) {
        throw "Harmony Search validation: required file missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchParameters.cs"

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchReferences.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\HarmonySearchTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\harmony-search-geem-kim-loganathan-2001.md"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.HarmonySearch",
    "MetaheuristicFamily.Other",
    "MetaheuristicMechanism.MemoryBased",
    "HarmonyMemoryConsiderationRate",
    "PitchAdjustmentRate",
    "PitchAdjustmentBandwidth",
    "FindWorstIndex",
    "ReplacedWorstHarmony",
    "MaximumHarmonySearchImprovisations"
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker)) {

        throw "Harmony Search validation: implementation marker '$marker' is missing."
    }
}

foreach ($doi in @(
    "10.1177/003754970107600201",
    "10.1016/j.amc.2006.11.033",
    "10.1016/j.amc.2007.09.004"
)) {
    if (-not $references.Contains($doi)) {
        throw "Harmony Search validation: scientific DOI '$doi' is missing."
    }
}

if (-not $tests.Contains("SameSeedProducesSameResult") -or
    -not $tests.Contains("MaximizationUsesObjectiveSenseSymmetrically") -or
    -not $tests.Contains("EvaluationBudgetMayStopDuringHarmonyMemoryInitialization") -or
    -not $tests.Contains("FactoryCreatesHarmonySearch")) {

    throw "Harmony Search validation: focused regression tests are incomplete."
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$matchItems =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
            "harmony-search-geem-kim-loganathan-2001"
        }
    )

if ($matchItems.Count -ne 1) {
    throw "Harmony Search validation: canonical catalog identity must occur exactly once."
}

$entry =
    $matchItems[0]

if ([string]$entry.doi -ne
    "10.1177/003754970107600201") {

    throw "Harmony Search validation: primary DOI mismatch."
}

if ([string]$entry.category -ne
    "other-metaheuristics") {

    throw "Harmony Search validation: documentary family classification mismatch."
}

if (-not ([string]$entry.update).Contains("\begin{aligned}")) {
    throw "Harmony Search validation: aligned mathematical update block is missing."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {

    throw (
        "Harmony Search validation: scientific page contains doubled Doxygen/TeX " +
        "command escaping. PowerShell here-strings preserve backslashes literally.")
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
    "10.1177/003754970107600201",
    "10.1016/j.amc.2006.11.033",
    "10.1016/j.amc.2007.09.004",
    "absolute bandwidth",
    "separate public identity since v0.56.0",
    "separate public identity since v0.57.0"
)) {
    if (-not $page.Contains($marker)) {
        throw "Harmony Search validation: algorithm page is missing '$marker'."
    }
}

Write-Host `
    "Harmony Search validation passed: canonical 2001 HS unchanged; IHS 2007 and GHS 2008 remain separate public identities." `
    -ForegroundColor Green