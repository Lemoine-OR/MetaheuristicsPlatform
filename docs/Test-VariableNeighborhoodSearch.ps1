[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "v0.25 VNS validation: missing '$Relative'."
    }
    return [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative, [string[]]$Markers) {
    $text = Read-Utf8 $Relative
    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            throw "v0.25 VNS validation: '$Relative' is missing '$marker'."
        }
    }
}

$version = (Read-Utf8 "version.json") | ConvertFrom-Json
if ([string]$version.version -ne "0.25.0") {
    throw "v0.25 VNS validation: version.json must be 0.25.0."
}

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\VariableNeighborhoodSearchReferences.cs" @(
    "MladenovicHansen1997",
    "HansenMladenovic2001",
    "10.1016/S0305-0548(97)00031-2",
    "10.1016/S0377-2217(00)00100-4"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\VariableNeighborhoodSearchParameters.cs" @(
    "VariableNeighborhoodDescentParameters",
    "VariableNeighborhoodSearchParameters",
    "MaximumNeighborhoodRestarts",
    "MaximumCycles"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\VariableNeighborhoodDescentProcedure.cs" @(
    "VariableNeighborhoodDescentProcedure",
    "ILocalSearchProcedure",
    "NeighborhoodRestarts",
    "VariableNeighborhoodDescentState"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\VariableNeighborhoodSearchOptimizers.cs" @(
    "VariableNeighborhoodDescentOptimizer",
    "VariableNeighborhoodSearchOptimizer",
    'Id = "variable-neighborhood-descent"',
    'Id = "variable-neighborhood-search-mladenovic-hansen"',
    "MaximumVnsCycles"
)

Require-Contains "tests\MetaheuristicsPlatform.Tests\VariableNeighborhoodSearchTests.cs" @(
    "VndRestartsAtFirstNeighborhoodAfterImprovement",
    "ReusableVndProcedureCanBeComposedInsideVns",
    "VnsResetsShakingNeighborhoodAfterStrictImprovement",
    "VnsNeverLosesBestSoFarWhenLaterShakesAreWorse",
    "VnsDescriptorCarriesOriginalScientificReference"
)

Require-Contains "docs\pages\algorithms\variable-neighborhood-descent.md" @(
    "## Mathematical details",
    "10.1016/S0377-2217(00)00100-4"
)

Require-Contains "docs\pages\algorithms\variable-neighborhood-search-mladenovic-hansen.md" @(
    "## Mathematical details",
    "10.1016/S0305-0548(97)00031-2"
)

Require-Contains "docs\variable-neighborhood-search-catalog.json" @(
    '"variable-neighborhood-descent"',
    '"variable-neighborhood-search-mladenovic-hansen"'
)

Require-Contains "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
    '"variable-neighborhood-descent"',
    '"variable-neighborhood-search-mladenovic-hansen"'
)

Require-Contains "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
    '"variable-neighborhood-descent"',
    '"variable-neighborhood-search-mladenovic-hansen"'
)

$catalog = (Read-Utf8 "docs\algorithm-catalog.json") | ConvertFrom-Json
$ids = @($catalog.algorithms | ForEach-Object { [string]$_.id })
foreach ($id in @(
    "variable-neighborhood-descent",
    "variable-neighborhood-search-mladenovic-hansen"
)) {
    if ($ids -notcontains $id) {
        throw "v0.25 VNS validation: docs/algorithm-catalog.json is missing '$id'."
    }
}

if (@($catalog.algorithms).Count -lt 15) {
    throw "v0.25 VNS validation: expected at least fifteen public algorithms."
}

$readme = Read-Utf8 "README.md"
foreach ($marker in @(
    "<strong>15 public algorithms",
    "<strong>9 trajectory methods",
    "variable-neighborhood-descent",
    "variable-neighborhood-search-mladenovic-hansen"
)) {
    if (-not $readme.Contains($marker)) {
        throw "v0.25 VNS validation: README is missing '$marker'."
    }
}

$futureTokens = @(
    "GuidedLocalSearchOptimizer"
)
$neighborhoodFiles = Get-ChildItem `
    (Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\Neighborhood") `
    -Filter "*.cs" -File

foreach ($file in $neighborhoodFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    foreach ($token in $futureTokens) {
        if ($text.Contains($token)) {
            throw "v0.25 VNS validation: future algorithm '$token' must not be preintroduced."
        }
    }
}

Write-Host "Variable Neighborhood Search validation passed: VND + canonical basic VNS." -ForegroundColor Green
