[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Iteration APSF-HS validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json
if ([version][string]$version.version -lt [version]"0.61.0") {
    throw "Iteration APSF-HS validation requires repository version 0.61.0 or later."
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\AdvancedParameterSettingFreeHarmonySearchIterationOptimizer.cs"
$parameters =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\HarmonySearch\AdvancedParameterSettingFreeHarmonySearchIterationParameters.cs"
$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AdvancedParameterSettingFreeHarmonySearchIterationTests.cs"
$page =
    Read-Utf8 "docs\pages\algorithms\advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020.md"

foreach ($marker in @(
    "10.0 * improvisation /",
    "5.0 / Math.Log(dimension)",
    "0.5 +",
    "GetPitchAdjustmentRate(",
    "(4.0 / dimension) -",
    "dimension == 1",
    "return 0.5",
    "searchSpace.Clamp(",
    "MaximumAdvancedParameterSettingFreeHarmonySearchIterationImprovisations"
)) {
    if (-not $optimizer.Contains($marker) -and
        -not $parameters.Contains($marker)) {
        throw "Iteration APSF-HS validation: implementation marker '$marker' is missing."
    }
}

foreach ($forbidden in @(
    "ParameterSettingFreeHarmonySearchOperationType",
    "operationTypeMemory",
    "TargetObjective",
    "LossStart",
    "LossObjective"
)) {
    if ($optimizer.Contains($forbidden) -or
        $parameters.Contains($forbidden)) {
        throw "Iteration APSF-HS validation: forbidden cross-scheme marker '$forbidden' detected."
    }
}

foreach ($testMarker in @(
    "PublishedHmcrEquationIsMatched",
    "PublishedParEquationIsMatched",
    "HmcrIncreasesWithIteration",
    "OneDimensionUsesDocumentedRightHandLimit",
    "PublicParametersDoNotExposeHmcrParOrOperationTypeMemory",
    "OneImprovisationUsesOneEvaluationAfterHarmonyMemoryInitialization",
    "EvaluationBudgetMayStopDuringHarmonyMemoryInitialization",
    "MaximizationIsSupported",
    "SameSeedProducesSameResult",
    "FactoryCreatesSevenDistinctHarmonySearchIdentities"
)) {
    if (-not $tests.Contains($testMarker)) {
        throw "Iteration APSF-HS validation: focused test '$testMarker' is missing."
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
    "advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020"
)

foreach ($identity in $ids) {
    if (@($catalog.algorithms | Where-Object { [string]$_.id -eq $identity }).Count -ne 1) {
        throw "Iteration APSF-HS validation: identity '$identity' must occur exactly once."
    }
}

$entry =
    @($catalog.algorithms | Where-Object {
        [string]$_.id -eq
            "advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020"
    })[0]

if ([string]$entry.doi -ne "10.3390/app10072586") {
    throw "Iteration APSF-HS validation: DOI mismatch."
}

if ($page.Contains('\\f') -or
    $page.Contains('\\begin') -or
    $page.Contains('\\end')) {
    throw "Iteration APSF-HS validation: doubled Doxygen/TeX escaping detected."
}

foreach ($marker in @(
    "iteration PSF scheme",
    "10.3390/app10072586",
    "HMCR(t)",
    "PAR(t)",
    "right-hand dimensional limit",
    "object-dependent adaptive bandwidth method is possible only",
    "no Operation Type Matrix"
)) {
    if (-not $page.Contains($marker)) {
        throw "Iteration APSF-HS validation: page marker '$marker' is missing."
    }
}

Write-Host `
    "Iteration APSF-HS validation passed: Jeong-Park-Geem-Sim 2020 Equations (5),(6),(8) executable; object scheme excluded." `
    -ForegroundColor Green
