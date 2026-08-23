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
        throw "SGHS validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version][string]$version.version -lt [version]"0.58.0") {
    throw "SGHS validation requires repository version 0.58.0 or later."
}

$requiredFiles =
    @(
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\SelfAdaptiveGlobalBestHarmonySearchOptimizer.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\SelfAdaptiveGlobalBestHarmonySearchParameters.cs",
        "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\SelfAdaptiveGlobalBestHarmonySearchState.cs",
        "tests\MetaheuristicsPlatform.Tests\SelfAdaptiveGlobalBestHarmonySearchTests.cs",
        "benchmarks\MetaheuristicsPlatform.Benchmarks\SelfAdaptiveGlobalBestHarmonySearchBenchmarks.cs",
        "docs\pages\algorithms\self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010.md"
    )

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative) -PathType Leaf)) {
        throw "SGHS validation: required file missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\SelfAdaptiveGlobalBestHarmonySearchOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\SelfAdaptiveGlobalBestHarmonySearchParameters.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\SelfAdaptiveGlobalBestHarmonySearchTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010.md"

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\HarmonySearchReferences.cs"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.SelfAdaptiveGlobalBestHarmonySearch",
    "HarmonySearchReferences.PanSuganthanTasgetirenLiang2010",
    "SampleTruncatedNormal(",
    "HarmonyMemoryConsiderationRateStandardDeviation",
    "PitchAdjustmentRateStandardDeviation",
    "successfulHmcr.Add(",
    "successfulPar.Add(",
    "learningPeriodPosition ==",
    "Average(",
    "GetPitchAdjustmentBandwidth(",
    "harmonyMemory[bestIndex][coordinate]",
    "searchSpace.Clamp(",
    "MaximumSelfAdaptiveGlobalBestHarmonySearchImprovisations"
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker)) {
        throw "SGHS validation: implementation marker '$marker' is missing."
    }
}

if ($optimizer.Contains("globalBestCoordinate") -or
    $optimizer.Contains("harmonyMemory[bestIndex][globalBestCoordinate]")) {
    throw "SGHS validation: GHS random cross-coordinate best assignment must not survive in SGHS."
}

if ($optimizer.IndexOf("value +=") -lt 0 -or
    $optimizer.IndexOf("harmonyMemory[bestIndex][coordinate]") -lt 0 -or
    $optimizer.IndexOf("value +=") -gt
        $optimizer.IndexOf("harmonyMemory[bestIndex][coordinate]")) {
    throw "SGHS validation: memory perturbation must precede the corresponding-coordinate best overwrite."
}

foreach ($constantMarker in @(
    "HarmonyMemoryConsiderationRateMinimum = 0.9",
    "HarmonyMemoryConsiderationRateMaximum = 1.0",
    "PitchAdjustmentRateMinimum = 0.0",
    "PitchAdjustmentRateMaximum = 1.0",
    "HarmonyMemoryConsiderationRateStandardDeviation = 0.01",
    "PitchAdjustmentRateStandardDeviation = 0.05",
    "InitialMeanHarmonyMemoryConsiderationRate { get; init; } = 0.98",
    "InitialMeanPitchAdjustmentRate { get; init; } = 0.9",
    "LearningPeriod { get; init; } = 100",
    "MinimumPitchAdjustmentBandwidth { get; init; } = 0.0005",
    "MaximumPitchAdjustmentBandwidthFractionOfRange { get; init; } = 0.1"
)) {
    if (-not $parameters.Contains($constantMarker)) {
        throw "SGHS validation: canonical setting marker '$constantMarker' is missing."
    }
}

foreach ($testMarker in @(
    "DefaultsMatchPublishedSghsLearningSettings",
    "BandwidthScheduleMatchesPublishedPiecewiseRule",
    "SampledRatesRemainInsidePublishedRanges",
    "EmptySuccessfulLearningPeriodPreservesMeans",
    "OptimizationCallbackEvents Events",
    "OptimizationCallbackEvents.IterationCompleted",
    "OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization",
    "EvaluationBudgetMayStopDuringHarmonyMemoryInitialization",
    "SameSeedProducesSameResult",
    "FactoryCreatesFourDistinctHarmonySearchIdentities"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "SGHS validation: focused test '$testMarker' is missing."
    }
}

if ($tests.Contains("OptimizationCallbackEvents.All") -or
    -not $tests.Contains(
        "OptimizationCallbackEvents.IterationCompleted")) {

    throw (
        "SGHS validation: state-capture callback must subscribe only to " +
        "IterationCompleted so the final Completed lifecycle event cannot " +
        "duplicate the same algorithm state.")
}

if (-not $references.Contains("10.1016/j.amc.2010.01.088")) {
    throw "SGHS validation: Pan-Suganthan-Tasgetiren-Liang DOI is missing from source provenance."
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
        throw "SGHS validation: identity '$identity' must occur exactly once."
    }
}

$sghs =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010"
        }
    )[0]

if ([string]$sghs.doi -ne "10.1016/j.amc.2010.01.088") {
    throw "SGHS validation: primary DOI mismatch."
}

if ([string]$sghs.category -ne "other-metaheuristics") {
    throw "SGHS validation: documentary family mismatch."
}

if (-not ([string]$sghs.update).Contains("\begin{aligned}")) {
    throw "SGHS validation: aligned mathematical update block is missing."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {
    throw "SGHS validation: doubled Doxygen/TeX command escaping detected."
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
    "10.1016/j.amc.2010.01.088",
    "HMCR_t",
    "PAR_t",
    "BW_i(t)",
    "x_i^{new}=x_i^{best}",
    "successful improvisations",
    "rejection sampling",
    "defensive completion"
)) {
    if (-not $page.Contains($marker)) {
        throw "SGHS validation: page marker '$marker' is missing."
    }
}

Write-Host `
    "SGHS validation passed: Pan-Suganthan-Tasgetiren-Liang 2010 adaptive HMCR/PAR learning, piecewise BW and corresponding-coordinate global-best pitch executable." `
    -ForegroundColor Green
