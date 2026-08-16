[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "v0.27 advanced VNS validation: missing '$Relative'."
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
            throw "v0.27 advanced VNS validation: '$Relative' is missing '$marker'."
        }
    }
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json

if ([string]$version.version -ne "0.27.0") {
    throw "v0.27 advanced VNS validation: version.json must be 0.27.0."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\AdvancedVariableNeighborhoodSearchContracts.cs" @(
        "ISolutionDistance",
        "AdvancedVariableNeighborhoodSearchState"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\AdvancedVariableNeighborhoodSearchParameters.cs" @(
        "ReducedVariableNeighborhoodSearchParameters",
        "GeneralVariableNeighborhoodSearchParameters",
        "SkewedVariableNeighborhoodSearchParameters",
        "Alpha",
        "MaximumNeighborhoodRestarts"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\AdvancedVariableNeighborhoodSearchReferences.cs" @(
        "HansenMladenovicTodosijevicHanafi2017",
        "10.1007/s13675-016-0075-x"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\AdvancedVariableNeighborhoodSearchOptimizers.cs" @(
        "ReducedVariableNeighborhoodSearchOptimizer",
        "GeneralVariableNeighborhoodSearchOptimizer",
        "SkewedVariableNeighborhoodSearchOptimizer",
        'Id = "reduced-variable-neighborhood-search"',
        'Id = "general-variable-neighborhood-search"',
        'Id = "skewed-variable-neighborhood-search-hansen-mladenovic-2001"',
        "VariableNeighborhoodDescentProcedure",
        "ISolutionDistance"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\AdvancedVariableNeighborhoodSearchTests.cs" @(
        "RvnsFindsImprovementWithoutLocalSearch",
        "GvnsUsesVariableNeighborhoodDescentAsImprovementPhase",
        "SvnsAcceptsWorseDistantCandidateAndEscapesValley",
        "SvnsWithZeroAlphaRejectsStrictlyWorseRecentering",
        "AdvancedVnsDescriptorsCarryCanonicalReferences"
    )

foreach ($page in @(
    "docs\pages\algorithms\reduced-variable-neighborhood-search.md",
    "docs\pages\algorithms\general-variable-neighborhood-search.md",
    "docs\pages\algorithms\skewed-variable-neighborhood-search-hansen-mladenovic-2001.md"
)) {
    Require-Contains $page @(
        "## Mathematical details",
        "10.1016/S0377-2217(00)00100-4",
        "10.1007/s13675-016-0075-x"
    )
}

Require-Contains `
    "docs\pages\components\advanced-variable-neighborhood-search-variants.md" @(
        "RVNS",
        "GVNS",
        "SVNS",
        "VNDS",
        "Reviewed / deferred"
    )

$variantCatalog =
    (Read-Utf8 "docs\advanced-variable-neighborhood-search-catalog.json") |
    ConvertFrom-Json

if (@($variantCatalog.executable).Count -ne 3) {
    throw "v0.27 advanced VNS validation: expected exactly three executable variants."
}

if (@($variantCatalog.reviewedDeferred).Count -ne 1) {
    throw "v0.27 advanced VNS validation: expected exactly one reviewed/deferred variant."
}

if ([string]$variantCatalog.reviewedDeferred[0].id -ne
    "variable-neighborhood-decomposition-search") {
    throw "v0.27 advanced VNS validation: VNDS reviewed entry is missing."
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$ids =
    @($catalog.algorithms |
      ForEach-Object { [string]$_.id })

foreach ($id in @(
    "reduced-variable-neighborhood-search",
    "general-variable-neighborhood-search",
    "skewed-variable-neighborhood-search-hansen-mladenovic-2001"
)) {
    if ($ids -notcontains $id) {
        throw "v0.27 advanced VNS validation: documentation catalog is missing '$id'."
    }
}

if (@($catalog.algorithms).Count -lt 19) {
    throw "v0.27 advanced VNS validation: expected at least nineteen public algorithms."
}

$readme = Read-Utf8 "README.md"

foreach ($marker in @(
    "<strong>19 public algorithms",
    "<strong>13 trajectory methods",
    "reduced-variable-neighborhood-search",
    "general-variable-neighborhood-search",
    "skewed-variable-neighborhood-search-hansen-mladenovic-2001"
)) {
    if (-not $readme.Contains($marker)) {
        throw "v0.27 advanced VNS validation: README is missing '$marker'."
    }
}

Write-Host `
    "Advanced VNS validation passed: 3 executable variants (RVNS/GVNS/SVNS) + VNDS reviewed/deferred." `
    -ForegroundColor Green
