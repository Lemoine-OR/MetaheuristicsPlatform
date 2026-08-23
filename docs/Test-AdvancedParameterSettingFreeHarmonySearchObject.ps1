[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Object APSF-HS validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json
if ([version][string]$version.version -lt [version]"0.62.0") {
    throw "Object APSF-HS validation requires version 0.62.0 or later."
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\AdvancedParameterSettingFreeHarmonySearchObjectOptimizer.cs"

$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\AdvancedParameterSettingFreeHarmonySearchObjectParameters.cs"

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AdvancedParameterSettingFreeHarmonySearchObjectTests.cs"

$page =
    Read-Utf8 "docs\pages\algorithms\advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020.md"

foreach ($marker in @(
    "10.0 -",
    "lossMean - TargetObjective",
    "lossStart -",
    "5.0 / Math.Log(dimension)",
    "GetPitchAdjustmentRate(",
    "GetAdaptiveBandwidthFraction(",
    "candidate >= 0.0001",
    "previousBlockMean -",
    "currentBlockMean",
    "0.1;",
    "problem.Sense !=",
    "OptimizationSense.Minimize",
    "searchSpace.Clamp(",
    "AdvancedParameterSettingFreeHarmonySearchObjectTarget"
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker)) {
        throw "Object APSF-HS validation: implementation marker '$marker' is missing."
    }
}

foreach ($forbidden in @(
    "operationTypeMemory",
    "ParameterSettingFreeHarmonySearchOperationType",
    "10.0 * improvisation /"
)) {
    if ($optimizer.Contains($forbidden) -or
        $parameters.Contains($forbidden)) {
        throw "Object APSF-HS validation: forbidden cross-scheme marker '$forbidden' detected."
    }
}

foreach ($testMarker in @(
    "PublishedObjectHmcrEquationIsMatched",
    "PublishedParEquationIsMatched",
    "EquationNineUsesImprovementBranchAtThreshold",
    "EquationNineUsesFallbackWhenImprovementIsTooSmall",
    "OneDimensionUsesDocumentedRightHandLimit",
    "MaximizationIsRejectedBecausePublishedEquationIsForMinimum",
    "TargetCanStopDuringInitialization",
    "SameSeedProducesSameResult",
    "FactoryCreatesEightDistinctHarmonySearchIdentities"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "Object APSF-HS validation: focused test '$testMarker' is missing."
    }
}

$catalog = (Read-Utf8 "docs\algorithm-catalog.json") | ConvertFrom-Json
$ids = @(
    "harmony-search-geem-kim-loganathan-2001",
    "improved-harmony-search-mahdavi-fesanghary-damangir-2007",
    "global-best-harmony-search-omran-mahdavi-2008",
    "self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010",
    "novel-global-harmony-search-zou-gao-wu-li-2010",
    "parameter-setting-free-harmony-search-geem-sim-2010",
    "advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020",
    "advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020"
)

foreach ($identity in $ids) {
    if (@($catalog.algorithms | Where-Object {
        [string]$_.id -eq $identity
    }).Count -ne 1) {
        throw "Object APSF-HS validation: identity '$identity' must occur exactly once."
    }
}

$entry =
    @($catalog.algorithms | Where-Object {
        [string]$_.id -eq
            "advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020"
    })[0]

if ([string]$entry.doi -ne "10.3390/app10072586") {
    throw "Object APSF-HS validation: DOI mismatch."
}

foreach ($marker in @(
    "Equation (7)",
    "Equation (8)",
    "Equation (9)",
    "minimization-only",
    "Loss_{mean}",
    "Loss_{start}",
    "Loss_{obj}",
    "0.0001",
    "0.1",
    "specific",
    "0.1% full-range",
    "coordinate-wise",
    "10.3390/app10072586"
)) {
    if (-not $page.Contains($marker)) {
        throw "Object APSF-HS validation: page marker '$marker' is missing."
    }
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {
    throw "Object APSF-HS validation: doubled Doxygen/TeX escaping detected."
}

Write-Host `
    "Object APSF-HS validation passed: Jeong-Park-Geem-Sim 2020 Equations (7),(8),(9), minimization target semantics and object-only adaptive bandwidth executable." `
    -ForegroundColor Green
