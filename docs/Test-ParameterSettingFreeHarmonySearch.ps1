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
        throw "PSF-HS validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version][string]$version.version -lt [version]"0.60.0") {
    throw "PSF-HS validation requires repository version 0.60.0 or later."
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ParameterSettingFreeHarmonySearchOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ParameterSettingFreeHarmonySearchParameters.cs"

$state =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\ParameterSettingFreeHarmonySearchState.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\ParameterSettingFreeHarmonySearchTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\parameter-setting-free-harmony-search-geem-sim-2010.md"

foreach ($marker in @(
    "ParameterSettingFreeHarmonySearchOperationType.RandomSelection",
    "ParameterSettingFreeHarmonySearchOperationType.MemoryConsideration",
    "ParameterSettingFreeHarmonySearchOperationType.PitchAdjustment",
    "RehearsalHarmonyMemoryConsiderationRate",
    "RehearsalPitchAdjustmentRate",
    "CalculateAdaptiveRates(",
    "memoryOrPitch",
    "(double)memoryOrPitch /",
    "(double)pitchCount /",
    "operationTypeMemory[worstIndex, coordinate]",
    "PitchAdjustmentBandwidthFractionOfRange",
    "MaximumParameterSettingFreeHarmonySearchImprovisations"
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker) -and
        -not $state.Contains($marker)) {
        throw "PSF-HS validation: implementation marker '$marker' is missing."
    }
}

if (-not $optimizer.Contains(
        "memoryOrPitch == 0") -or
    -not $optimizer.Contains(
        "? 0.0")) {

    throw "PSF-HS validation: zero-denominator defensive PAR completion is missing."
}

foreach ($testMarker in @(
    "DefaultsPreserveConventionalRehearsalSettings",
    "RehearsalCountIsMemoryCyclesTimesHmsCappedByNi",
    "ConstantProblemLeavesInitialRandomOtmAndYieldsZeroAdaptiveRates",
    "OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization",
    "EvaluationBudgetMayStopDuringHarmonyMemoryInitialization",
    "MaximizationIsSupported",
    "SameSeedProducesSameResult",
    "FactoryCreatesSixDistinctHarmonySearchIdentities",
    "OptimizationCallbackEvents.IterationCompleted"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "PSF-HS validation: focused test '$testMarker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$ids =
    @(
        "harmony-search-geem-kim-loganathan-2001",
        "improved-harmony-search-mahdavi-fesanghary-damangir-2007",
        "global-best-harmony-search-omran-mahdavi-2008",
        "self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010",
        "novel-global-harmony-search-zou-gao-wu-li-2010",
        "parameter-setting-free-harmony-search-geem-sim-2010"
    )

foreach ($identity in $ids) {
    if (@(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq $identity
        }
    ).Count -ne 1) {
        throw "PSF-HS validation: identity '$identity' must occur exactly once."
    }
}

$psf =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "parameter-setting-free-harmony-search-geem-sim-2010"
        }
    )[0]

if ([string]$psf.doi -ne "10.1016/j.amc.2010.09.049") {
    throw "PSF-HS validation: primary DOI mismatch."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {
    throw "PSF-HS validation: doubled Doxygen/TeX escaping detected."
}

foreach ($marker in @(
    "Operation Type Matrix",
    "RandomSelection",
    "MemoryConsideration",
    "PitchAdjustment",
    "HMCR_i",
    "PAR_i",
    "rehearsal HMCR = 0.5",
    "rehearsal PAR = 0.5",
    "strictly better candidate",
    "0/0",
    "10.1016/j.amc.2010.09.049"
)) {
    if (-not $page.Contains($marker)) {
        throw "PSF-HS validation: page marker '$marker' is missing."
    }
}

Write-Host `
    "PSF-HS validation passed: Geem-Sim 2010 OTM rehearsal/performance HMCR-PAR adaptation executable; zero-denominator corner case explicit." `
    -ForegroundColor Green
