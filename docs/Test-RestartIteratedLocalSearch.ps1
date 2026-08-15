[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Require-Contains(
    [string]$Path,
    [string[]]$Markers) {

    $fullPath = Join-Path $Root $Path
    if (-not (Test-Path $fullPath)) {
        throw "v0.24 validation: missing '$Path'."
    }

    $text = Get-Content $fullPath -Raw
    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            throw "v0.24 validation: '$Path' is missing '$marker'."
        }
    }
}

$version = Get-Content (Join-Path $Root "version.json") -Raw | ConvertFrom-Json
$versionText = [string]$version.version
if ($versionText -ne "0.24.0" -and $versionText -ne "0.24.1") {
    throw "v0.24 validation: expected a v0.24.x repository version supported by this validator."
}

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchContracts.cs" @(
    "ISolutionPerturbation",
    "NeighborhoodAcceptanceKind",
    "DelegateSolutionPerturbation"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchParameters.cs" @(
    "MultiStartLocalSearchParameters",
    "IteratedLocalSearchParameters",
    "MaximumStarts",
    "MaximumIterations"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\RestartIteratedLocalSearchOptimizers.cs" @(
    "MultiStartLocalSearchOptimizer",
    "IteratedLocalSearchOptimizer",
    'Id = "multi-start-local-search"',
    'Id = "iterated-local-search-lourenco-martin-stutzle"',
    "LourencoMartinStutzle2003",
    "Marti2003"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchReferences.cs" @(
    "10.1007/0-306-48056-5_11",
    "10.1007/0-306-48056-5_12"
)

Require-Contains "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
    '"multi-start-local-search"',
    '"iterated-local-search-lourenco-martin-stutzle"'
)

Require-Contains "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs" @(
    '"multi-start-local-search"',
    '"iterated-local-search-lourenco-martin-stutzle"'
)

Require-Contains "tests\MetaheuristicsPlatform.Tests\RestartIteratedLocalSearchTests.cs" @(
    "MultiStartRetainsBestSolutionAcrossIndependentStarts",
    "IlsPerturbationCanEscapeInitialBasin",
    "IlsAlwaysAcceptanceNeverLosesBestSoFar"
)

Require-Contains "docs\pages\algorithms\multi-start-local-search.md" @(
    "## Mathematical details",
    "10.1007/0-306-48056-5_12"
)

Require-Contains "docs\pages\algorithms\iterated-local-search-lourenco-martin-stutzle.md" @(
    "## Mathematical details",
    "10.1007/0-306-48056-5_11"
)

Require-Contains "docs\restart-iterated-local-search-catalog.json" @(
    '"multi-start-local-search"',
    '"iterated-local-search-lourenco-martin-stutzle"'
)

$catalog = Get-Content (Join-Path $Root "docs\algorithm-catalog.json") -Raw | ConvertFrom-Json
$ids = @($catalog.algorithms | ForEach-Object { [string]$_.id })
foreach ($id in @("multi-start-local-search", "iterated-local-search-lourenco-martin-stutzle")) {
    if ($ids -notcontains $id) {
        throw "v0.24 validation: docs/algorithm-catalog.json is missing '$id'."
    }
}

# Keep the v0.24 scope exact. These belong to later neighborhood-search releases.
$futureTokens = @(
    "VariableNeighborhoodSearchOptimizer",
    "GuidedLocalSearchOptimizer"
)
$neighborhoodFiles = Get-ChildItem (Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\Neighborhood") -Filter "*.cs" -File
foreach ($file in $neighborhoodFiles) {
    $text = Get-Content $file.FullName -Raw
    foreach ($token in $futureTokens) {
        if ($text.Contains($token)) {
            throw "v0.24 validation: future algorithm '$token' must not be preintroduced in this release."
        }
    }
}

Write-Host "v0.24 restart / ILS validation passed." -ForegroundColor Green
