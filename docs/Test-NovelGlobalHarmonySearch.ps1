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
        throw "NGHS validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version][string]$version.version -lt [version]"0.59.0") {
    throw "NGHS validation requires repository version 0.59.0 or later."
}

$requiredFiles =
    @(
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\NovelGlobalHarmonySearchOptimizer.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\NovelGlobalHarmonySearchParameters.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\NovelGlobalHarmonySearchState.cs",
        "tests\MetaheuristicsPlatform.Tests\NovelGlobalHarmonySearchTests.cs",
        "benchmarks\MetaheuristicsPlatform.Benchmarks\NovelGlobalHarmonySearchBenchmarks.cs",
        "docs\pages\algorithms\novel-global-harmony-search-zou-gao-wu-li-2010.md"
    )

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative) -PathType Leaf)) {
        throw "NGHS validation: required file missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\NovelGlobalHarmonySearchOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\NovelGlobalHarmonySearchParameters.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\NovelGlobalHarmonySearchTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\novel-global-harmony-search-zou-gao-wu-li-2010.md"

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchReferences.cs"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.NovelGlobalHarmonySearch",
    "HarmonySearchReferences.ZouGaoWuLi2010NovelGlobal",
    "2.0 * bestHarmony[coordinate]",
    "worstHarmony[coordinate]",
    "reflectedBest > upper",
    "reflectedBest < lower",
    "worstHarmony[coordinate] +",
    "mutationProbability",
    "random.NextDouble() <=",
    "improvisedHarmony.AsSpan().CopyTo(",
    "UnconditionallyReplacedWorstHarmony: true",
    "MaximumNovelGlobalHarmonySearchImprovisations"
)) {
    if (-not $optimizer.Contains($marker)) {
        throw "NGHS validation: implementation marker '$marker' is missing."
    }
}

if ($parameters.Contains("HarmonyMemoryConsiderationRate") -or
    $parameters.Contains("PitchAdjustmentRate") -or
    $parameters.Contains("Bandwidth")) {

    throw "NGHS validation: HMCR, PAR and BW must not reappear in canonical NGHS parameters."
}

foreach ($parameterMarker in @(
    "HarmonyMemorySize { get; init; } = 5",
    "MutationProbability { get; init; } = 0.005"
)) {
    if (-not $parameters.Contains($parameterMarker)) {
        throw "NGHS validation: canonical parameter marker '$parameterMarker' is missing."
    }
}

foreach ($testMarker in @(
    "DefaultsMatchCanonicalContinuousNghsSettings",
    "PublicParametersExcludeHmcrParAndBandwidth",
    "ReplacementIsUnconditionalEvenWithoutStrictImprovement",
    "MutationProbabilityOneMutatesEveryCoordinate",
    "OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization",
    "EvaluationBudgetMayStopDuringHarmonyMemoryInitialization",
    "MaximizationIsSupported",
    "SameSeedProducesSameResult",
    "FactoryCreatesFiveDistinctHarmonySearchIdentities",
    "OptimizationCallbackEvents.IterationCompleted"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "NGHS validation: focused test '$testMarker' is missing."
    }
}

if (-not $references.Contains("10.1016/j.cie.2009.11.003") -or
    -not $references.Contains("10.1016/j.neucom.2010.07.010")) {

    throw "NGHS validation: original and supporting 2010 provenance are incomplete."
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

foreach ($identity in @(
    "harmony-search-geem-kim-loganathan-2001",
    "improved-harmony-search-mahdavi-fesanghary-damangir-2007",
    "global-best-harmony-search-omran-mahdavi-2008",
    "self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010",
    "novel-global-harmony-search-zou-gao-wu-li-2010"
)) {
    $matches =
        @(
            $catalog.algorithms |
            Where-Object {
                [string]$_.id -eq $identity
            }
        )

    if ($matches.Count -ne 1) {
        throw "NGHS validation: identity '$identity' must occur exactly once."
    }
}

$nghs =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "novel-global-harmony-search-zou-gao-wu-li-2010"
        }
    )[0]

if ([string]$nghs.doi -ne "10.1016/j.cie.2009.11.003") {
    throw "NGHS validation: primary DOI mismatch."
}

if ([string]$nghs.category -ne "other-metaheuristics") {
    throw "NGHS validation: documentary family mismatch."
}

if (-not ([string]$nghs.update).Contains("\begin{aligned}")) {
    throw "NGHS validation: aligned mathematical update block is missing."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {

    throw "NGHS validation: doubled Doxygen/TeX command escaping detected."
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
    "10.1016/j.cie.2009.11.003",
    "10.1016/j.neucom.2010.07.010",
    "unconditionally",
    "no HMCR, PAR or BW",
    "2x_i^{best}-x_i^{worst}"
)) {
    if (-not $page.Contains($marker)) {
        throw "NGHS validation: page marker '$marker' is missing."
    }
}

Write-Host `
    "NGHS validation passed: Zou-Gao-Wu-Li 2010 bounded reflected-best position update, low-probability mutation and unconditional worst-harmony replacement executable." `
    -ForegroundColor Green
