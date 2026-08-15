[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    return [System.IO.File]::ReadAllText((Join-Path $Root $Relative))
}

function Get-PropertyValue([object]$Object, [string]$Name) {
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) { return $null }
    return $property.Value
}

$requiredFiles = @(
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchContracts.cs",
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchReferences.cs",
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\MoveLocalSearchProcedure.cs",
    "src\MetaheuristicsPlatform\Algorithms\Neighborhood\LocalSearchOptimizers.cs",
    "tests\MetaheuristicsPlatform.Tests\LocalSearchFoundationTests.cs",
    "docs\local-search-foundation-catalog.json"
)
foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Local Search core: missing '$relative'."
    }
}

$version = Get-Content (Join-Path $Root "version.json") -Raw | ConvertFrom-Json
$match = [System.Text.RegularExpressions.Regex]::Match([string]$version.version, '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)')
if (-not $match.Success) { throw "Local Search core: unsupported repository version." }
$repoVersion = [System.Version]::new(
    [int]$match.Groups['major'].Value,
    [int]$match.Groups['minor'].Value,
    [int]$match.Groups['patch'].Value)
if ($repoVersion -lt [System.Version]::new(0, 23, 0)) {
    throw "Local Search core: requires repository version 0.23.0 or later."
}

$contracts = Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchContracts.cs"
foreach ($marker in @("LocalSearchSelectionPolicy", "FirstImprovement", "BestImprovement", "ILocalSearchProcedure")) {
    if (-not $contracts.Contains($marker)) { throw "Local Search core: contracts missing '$marker'." }
}


$parameters = Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\Neighborhood\NeighborhoodSearchParameters.cs"
if (-not $parameters.Contains("LocalSearchParameters")) {
    throw "Local Search core: LocalSearchParameters is missing."
}

$engine = Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\Neighborhood\MoveLocalSearchProcedure.cs"
foreach ($marker in @(
    "IEnumeratedNeighborhood",
    "IReversibleMoveOperator",
    "LocalSearchSelectionPolicy.FirstImprovement",
    "RegisterExternalProbeEvaluation",
    "PromoteOwnedExternalProbeSnapshot",
    "TryEvaluateCandidateObjective",
    "CaptureUndo",
    "ThrowIfCancellationRequested"
)) {
    if (-not $engine.Contains($marker)) { throw "Local Search core: move engine missing '$marker'." }
}

$localSource = Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\Neighborhood\LocalSearchOptimizers.cs"
$ids = @("local-search-best-improvement", "local-search-first-improvement")
foreach ($id in $ids) {
    if (-not $localSource.Contains($id)) { throw "Local Search core: runtime source missing '$id'." }
}

$catalog = Get-Content (Join-Path $Root "docs\local-search-foundation-catalog.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$catalogEntries = @($catalog.algorithms)
if ($catalogEntries.Count -ne 2) { throw "Local Search core: catalog must contain exactly 2 algorithms." }
$catalogIds = @{}
foreach ($entry in $catalogEntries) {
    $id = [string](Get-PropertyValue $entry "id")
    if ([string]::IsNullOrWhiteSpace($id)) { throw "Local Search core: catalog entry has no id." }
    if ($catalogIds.ContainsKey($id)) { throw "Local Search core: duplicate id '$id'." }
    $catalogIds[$id] = $true
    foreach ($field in @("name","doi","sourcePath","page","class","publication")) {
        $value = Get-PropertyValue $entry $field
        if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) {
            throw "Local Search core: '$id' missing '$field'."
        }
    }
    if (-not (Test-Path -LiteralPath (Join-Path $Root ([string]$entry.sourcePath)))) {
        throw "Local Search core: source path missing for '$id'."
    }
    if (-not (Test-Path -LiteralPath (Join-Path $Root ([string]$entry.page)))) {
        throw "Local Search core: documentation page missing for '$id'."
    }
}
foreach ($id in $ids) {
    if (-not $catalogIds.ContainsKey($id)) { throw "Local Search core: catalog missing '$id'." }
}

$globalCatalog = Get-Content (Join-Path $Root "docs\algorithm-catalog.json") -Raw -Encoding UTF8 | ConvertFrom-Json
$runtimeCatalog = Read-Utf8 "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs"
$idsSource = Read-Utf8 "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs"
$readme = Read-Utf8 "README.md"
foreach ($id in $ids) {
    if (@($globalCatalog.algorithms | Where-Object id -eq $id).Count -ne 1) {
        throw "Local Search core: global catalog must contain exactly one '$id'."
    }
    if (-not $runtimeCatalog.Contains('"' + $id + '"')) { throw "Local Search core: runtime catalog missing '$id'." }
    if (-not $idsSource.Contains('"' + $id + '"')) { throw "Local Search core: public IDs missing '$id'." }
    if (-not $readme.Contains($id)) { throw "Local Search core: README missing '$id'." }
}


$pageMarkers = @(
    "## General description", "## Technical specifications", "## Complexity", "## Applicability",
    "## Detailed operation", "## Parameters", "## API example", "## Stable factory ID",
    "## Mathematical details", "### Problem formulation", "### Update equations / iterations",
    "### Assumptions", "### Convergence conditions", "### Scientific references"
)
foreach ($entry in $catalogEntries) {
    $page = Read-Utf8 ([string]$entry.page)
    foreach ($marker in $pageMarkers) {
        if (-not $page.Contains($marker)) { throw "Local Search core: '$($entry.id)' page missing '$marker'." }
    }
    if (-not $page.Contains([string]$entry.id) -or -not $page.Contains([string]$entry.doi)) {
        throw "Local Search core: '$($entry.id)' page missing stable ID or DOI."
    }
    if ($page.Contains("\(") -or $page.Contains("\)")) {
        throw "Local Search core: raw parenthesized LaTeX delimiters are forbidden in Doxygen pages."
    }
}

$tests = Read-Utf8 "tests\MetaheuristicsPlatform.Tests\LocalSearchFoundationTests.cs"
$factCount = [System.Text.RegularExpressions.Regex]::Matches($tests, '\[Fact\]').Count
if ($factCount -lt 13) { throw "Local Search core: expected at least 13 focused tests; found $factCount." }
$optimizeCount = [System.Text.RegularExpressions.Regex]::Matches($tests, '\.Optimize\(').Count
$tokenCount = [System.Text.RegularExpressions.Regex]::Matches(
    $tests,
    'cancellationToken:\s*TestContext\.Current\.CancellationToken').Count
if ($optimizeCount -ne $tokenCount) {
    throw "Local Search core: every Optimize call must pass the xUnit cancellation token. Optimize=$optimizeCount token=$tokenCount."
}
if ($tests.Contains("MultiStartLocalSearchOptimizer") -or $tests.Contains("IteratedLocalSearchOptimizer")) {
    throw "Local Search core: future restart/ILS tests must not be included in v0.23.0."
}

Write-Host "Local Search core validation passed: 2 algorithms, $factCount focused tests." -ForegroundColor Green
