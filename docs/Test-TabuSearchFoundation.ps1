[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$required = @(
    "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ExpirationTabuMemory.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ITabuAttributeProvider.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ITabuAspirationCriterion.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ITabuTenurePolicy.cs",
    "docs\pages\algorithms\tabu-search-glover.md",
    "docs\pages\families\trajectory-based-methods.md",
    "tests\MetaheuristicsPlatform.Tests\TabuSearchFoundationTests.cs"
)

foreach ($relative in $required) {
    if (-not (Test-Path (Join-Path $Root $relative))) {
        throw "Tabu Search foundation: missing '$relative'."
    }
}

$version =
    Get-Content (Join-Path $Root "version.json") -Raw |
    ConvertFrom-Json

$versionText = [string]$version.version
$versionMatch =
    [System.Text.RegularExpressions.Regex]::Match(
        $versionText,
        '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)')

if (-not $versionMatch.Success) {
    throw "Tabu Search foundation: version.json contains unsupported version '$versionText'."
}

$repositoryVersion =
    [System.Version]::new(
        [int]$versionMatch.Groups['major'].Value,
        [int]$versionMatch.Groups['minor'].Value,
        [int]$versionMatch.Groups['patch'].Value)

$minimumSupportedVersion =
    [System.Version]::new(0, 21, 0)

if ($repositoryVersion -lt $minimumSupportedVersion) {
    throw "Tabu Search foundation: requires repository version 0.21.0 or later."
}

$optimizer =
    Get-Content (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchOptimizer.cs"
    ) -Raw

$optimizerMarkers = @(
    "IEnumeratedNeighborhood",
    "IReversibleMoveOperator",
    "IMoveObjectiveDeltaEvaluator",
    "ITabuAttributeProvider",
    "RegisterExternalProbeEvaluation",
    "PromoteOwnedExternalProbeSnapshot",
    "NoAdmissibleMove",
    "tabu-search-glover"
)

foreach ($marker in $optimizerMarkers) {
    if (-not $optimizer.Contains($marker)) {
        throw "Tabu Search foundation: optimizer missing '$marker'."
    }
}

if ([System.Text.RegularExpressions.Regex]::IsMatch(
        $optimizer,
        '(?s)\?\?\s*static\s+')) {
    throw "Tabu Search foundation: a static lambda used as the right operand of ?? must be parenthesized."
}

$context =
    Get-Content (
        Join-Path $Root "src\MetaheuristicsPlatform\Core\OptimizationContext.cs"
    ) -Raw

foreach ($marker in @(
    "RegisterExternalProbeEvaluation",
    "PromoteOwnedExternalProbeSnapshot"
)) {
    if (-not $context.Contains($marker)) {
        throw "Tabu Search foundation: OptimizationContext missing '$marker'."
    }
}

$ids =
    Get-Content (
        Join-Path $Root "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs"
    ) -Raw

if (-not $ids.Contains('"tabu-search-glover"')) {
    throw "Tabu Search foundation: stable algorithm ID missing."
}

$runtimeCatalog =
    Get-Content (
        Join-Path $Root "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs"
    ) -Raw

if (-not $runtimeCatalog.Contains('"tabu-search-glover"')) {
    throw "Tabu Search foundation: runtime catalog entry missing."
}

$docsCatalog =
    Get-Content (
        Join-Path $Root "docs\algorithm-catalog.json"
    ) -Raw |
    ConvertFrom-Json

$entry =
    @(
        $docsCatalog.algorithms |
        Where-Object id -eq "tabu-search-glover"
    )

if ($entry.Count -ne 1) {
    throw "Tabu Search foundation: documentation catalog must contain exactly one TS entry."
}

if ([string]$entry[0].doi -ne "10.1287/ijoc.1.3.190") {
    throw "Tabu Search foundation: canonical DOI mismatch."
}

$memory =
    Get-Content (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\ExpirationTabuMemory.cs"
    ) -Raw

foreach ($marker in @(
    "PriorityQueue<ExpirationEntry, long>",
    "TryPeek",
    "TabuUntilIteration"
)) {
    if (-not $memory.Contains($marker)) {
        throw "Tabu Search foundation: expiration memory missing '$marker'."
    }
}

if ($memory.Contains("Queue<ExpirationEntry>")) {
    throw "Tabu Search foundation: FIFO expiration queue is invalid for varying tenure."
}

$familyPage =
    Get-Content (
        Join-Path $Root "docs\pages\families\trajectory-based-methods.md"
    ) -Raw

if (-not $familyPage.Contains("tabu-search-glover")) {
    throw "Tabu Search foundation: trajectory family page does not list Tabu Search."
}

$page =
    Get-Content (
        Join-Path $Root "docs\pages\algorithms\tabu-search-glover.md"
    ) -Raw

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
    "10.1287/ijoc.1.3.190",
    "10.1287/ijoc.2.1.4",
    "10.1287/ijoc.6.2.126",
    "implemented separately in v0.22"
)) {
    if (-not $page.Contains($marker)) {
        throw "Tabu Search foundation: documentation page missing '$marker'."
    }
}

if ($page.Contains("\(") -or
    $page.Contains("\)")) {
    throw "Tabu Search foundation: use Doxygen inline math delimiters instead of raw parenthesized LaTeX delimiters."
}

if ($page.Contains('\\f$')) {
    throw "Tabu Search foundation: Doxygen inline math delimiter contains a doubled backslash."
}

$foundationTestsPath =
    Join-Path $Root "tests\MetaheuristicsPlatform.Tests\TabuSearchFoundationTests.cs"

$foundationTests =
    Get-Content -LiteralPath $foundationTestsPath -Raw -Encoding UTF8

$optimizeCallCount =
    [System.Text.RegularExpressions.Regex]::Matches(
        $foundationTests,
        'optimizer\.Optimize\(').Count

$testCancellationTokenCount =
    [System.Text.RegularExpressions.Regex]::Matches(
        $foundationTests,
        'TestContext\.Current\.CancellationToken').Count

if ($optimizeCallCount -eq 0) {
    throw "Tabu Search foundation: no optimizer integration tests were found."
}

if ($optimizeCallCount -ne $testCancellationTokenCount) {
    throw (
        "Tabu Search foundation: every optimizer integration test must pass " +
        "TestContext.Current.CancellationToken (Optimize calls: $optimizeCallCount; tokens: $testCancellationTokenCount).")
}

Write-Host "Tabu Search foundation validation passed." -ForegroundColor Green
