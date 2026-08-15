[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$required = @(
    "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuSearchOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuSearchParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuTenurePolicy.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuReaction.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuTenureContext.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuSearchState.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\IReactiveTabuTenurePolicy.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ConfigurationRepetitionMemory.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\AttributeFrequencyMemory.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\ITabuSearchSolutionSignatureProvider.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\DelegateTabuSearchSolutionSignatureProvider.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchRepetitionObservation.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchComponentIds.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchComponentDescriptor.cs",
    "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchComponentCatalog.cs",
    "docs\ts-memory-control-catalog.json",
    "docs\pages\components\tabu-search-memory-control-strategies.md",
    "docs\pages\algorithms\reactive-tabu-search-battiti-tecchiolli-1994.md",
    "docs\Build-TabuSearchAdvancedDocumentation.ps1",
    "tests\MetaheuristicsPlatform.Tests\ReactiveTabuSearchAdvancedTests.cs"
)

foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Tabu Search advanced memory: missing '$relative'."
    }
}

$version =
    Get-Content -LiteralPath (Join-Path $Root "version.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json

$versionText = [string]$version.version
$versionMatch =
    [System.Text.RegularExpressions.Regex]::Match(
        $versionText,
        '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)')

if (-not $versionMatch.Success) {
    throw "Tabu Search advanced memory: unsupported repository version '$versionText'."
}

$repositoryVersion =
    [System.Version]::new(
        [int]$versionMatch.Groups['major'].Value,
        [int]$versionMatch.Groups['minor'].Value,
        [int]$versionMatch.Groups['patch'].Value)

if ($repositoryVersion -lt [System.Version]::new(0, 22, 0)) {
    throw "Tabu Search advanced memory: requires repository version 0.22.0 or later."
}

$catalog =
    Get-Content -LiteralPath (Join-Path $Root "docs\ts-memory-control-catalog.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json

$entries = @($catalog.entries)

if ($entries.Count -lt 13) {
    throw "Tabu Search advanced memory: expected at least 13 reviewed component entries."
}

$ids = @{}
$implemented = 0
$reviewed = 0

$idsSource =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchComponentIds.cs"
    ) -Raw -Encoding UTF8

$runtimeCatalog =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\TabuSearchComponentCatalog.cs"
    ) -Raw -Encoding UTF8

$page =
    Get-Content -LiteralPath (
        Join-Path $Root "docs\pages\components\tabu-search-memory-control-strategies.md"
    ) -Raw -Encoding UTF8

if ($page.Contains("\(") -or $page.Contains("\)")) {
    throw "Tabu Search advanced memory: component page must use Doxygen \f$...\f$ inline math delimiters."
}

foreach ($entry in $entries) {
    $id = [string]$entry.id

    if ([string]::IsNullOrWhiteSpace($id)) {
        throw "Tabu Search advanced memory: component entry without stable ID."
    }

    if ($ids.ContainsKey($id)) {
        throw "Tabu Search advanced memory: duplicate component ID '$id'."
    }

    $ids[$id] = $true

    foreach ($field in @(
        "name",
        "availability",
        "category",
        "scope",
        "formula",
        "complexity",
        "parameters",
        "reference",
        "source"
    )) {
        $property = $entry.PSObject.Properties[$field]
        if ($null -eq $property -or
            [string]::IsNullOrWhiteSpace([string]$property.Value)) {
            throw "Tabu Search advanced memory: '$id' is missing '$field'."
        }
    }

    if (-not $page.Contains($id)) {
        throw "Tabu Search advanced memory: component page is missing '$id'."
    }

    $doiProperty = $entry.PSObject.Properties["doi"]
    $doi =
        if ($null -eq $doiProperty) {
            ""
        }
        else {
            [string]$doiProperty.Value
        }

    if (-not [string]::IsNullOrWhiteSpace($doi) -and
        -not $page.Contains($doi)) {
        throw "Tabu Search advanced memory: component page is missing DOI '$doi'."
    }

    switch ([string]$entry.availability) {
        "implemented" {
            $implemented++

            foreach ($field in @("implementationClass", "sourcePath")) {
                $property = $entry.PSObject.Properties[$field]
                if ($null -eq $property -or
                    [string]::IsNullOrWhiteSpace([string]$property.Value)) {
                    throw "Tabu Search advanced memory: implemented '$id' is missing '$field'."
                }
            }

            $sourcePathValue =
                [string]$entry.PSObject.Properties["sourcePath"].Value
            $sourcePath =
                Join-Path $Root $sourcePathValue

            if (-not (Test-Path -LiteralPath $sourcePath)) {
                throw "Tabu Search advanced memory: source missing for '$id': '$sourcePathValue'."
            }

            if (-not $idsSource.Contains('"' + $id + '"')) {
                throw "Tabu Search advanced memory: stable component ID '$id' missing from TabuSearchComponentIds."
            }

            if (-not $runtimeCatalog.Contains('TabuSearchComponentIds.')) {
                throw "Tabu Search advanced memory: runtime component catalog is malformed."
            }

            if (-not $runtimeCatalog.Contains([string]$entry.name)) {
                throw "Tabu Search advanced memory: runtime component catalog is missing '$($entry.name)'."
            }
        }

        "reviewed-composite" {
            $reviewed++

            $sourcePathProperty =
                $entry.PSObject.Properties["sourcePath"]

            if ($null -ne $sourcePathProperty -and
                -not [string]::IsNullOrWhiteSpace([string]$sourcePathProperty.Value)) {
                throw "Tabu Search advanced memory: reviewed component '$id' must not pretend to have a source path."
            }
        }

        default {
            throw "Tabu Search advanced memory: unsupported availability '$($entry.availability)' for '$id'."
        }
    }
}

if ($implemented -lt 10) {
    throw "Tabu Search advanced memory: expected at least 10 executable component entries, found $implemented."
}

if ($reviewed -lt 3) {
    throw "Tabu Search advanced memory: expected at least 3 reviewed advanced strategies."
}

$implementedCountProperty = $catalog.PSObject.Properties["implementedCount"]
$reviewedCountProperty = $catalog.PSObject.Properties["reviewedCompositeCount"]

if ($null -eq $implementedCountProperty -or
    [int]$implementedCountProperty.Value -ne $implemented) {
    throw "Tabu Search advanced memory: implementedCount metadata does not match the catalog entries."
}

if ($null -eq $reviewedCountProperty -or
    [int]$reviewedCountProperty.Value -ne $reviewed) {
    throw "Tabu Search advanced memory: reviewedCompositeCount metadata does not match the catalog entries."
}

$algorithmIds =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs"
    ) -Raw -Encoding UTF8

$metaCatalog =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Catalog\MetaheuristicCatalog.cs"
    ) -Raw -Encoding UTF8

$rtsId = "reactive-tabu-search-battiti-tecchiolli-1994"

if (-not $algorithmIds.Contains('"' + $rtsId + '"')) {
    throw "Tabu Search advanced memory: Reactive Tabu Search stable algorithm ID missing."
}

if (-not $metaCatalog.Contains('"' + $rtsId + '"')) {
    throw "Tabu Search advanced memory: Reactive Tabu Search runtime catalog entry missing."
}

$docsCatalog =
    Get-Content -LiteralPath (Join-Path $Root "docs\algorithm-catalog.json") -Raw -Encoding UTF8 |
    ConvertFrom-Json

$rtsEntries =
    @(
        $docsCatalog.algorithms |
        Where-Object id -eq $rtsId
    )

if ($rtsEntries.Count -ne 1) {
    throw "Tabu Search advanced memory: documentation catalog must contain exactly one RTS entry."
}

if ([string]$rtsEntries[0].doi -ne "10.1287/ijoc.6.2.126") {
    throw "Tabu Search advanced memory: RTS canonical DOI mismatch."
}

$rtsOptimizer =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuSearchOptimizer.cs"
    ) -Raw -Encoding UTF8

foreach ($marker in @(
    "ConfigurationRepetitionMemory",
    "ReactiveTabuTenurePolicy",
    "AttributeFrequencyMemory",
    "NextInt32(",
    "applicableCount",
    "FrequencyPenaltyWeight",
    "IntensificationAfterIterationsWithoutImprovement",
    "reactive-tabu-search-battiti-tecchiolli-1994"
)) {
    if (-not $rtsOptimizer.Contains($marker)) {
        throw "Tabu Search advanced memory: RTS optimizer missing '$marker'."
    }
}

if ([System.Text.RegularExpressions.Regex]::IsMatch(
        $rtsOptimizer,
        '(?s)\?\?\s*static\s+')) {
    throw "Tabu Search advanced memory: a static lambda used as the right operand of ?? must be parenthesized."
}

$cancellationChecks =
    [System.Text.RegularExpressions.Regex]::Matches(
        $rtsOptimizer,
        'cancellationToken\.ThrowIfCancellationRequested\(\)').Count

if ($cancellationChecks -lt 2) {
    throw "Tabu Search advanced memory: normal and diversification neighborhood scans must both remain cancellation-responsive."
}

foreach ($fastPathMarker in @(
    "FrequencyPenaltyWeight > 0.0",
    "frequencyMemory is not null",
    "intensificationEnabled"
)) {
    if (-not $rtsOptimizer.Contains($fastPathMarker)) {
        throw "Tabu Search advanced memory: optional-control fast-path marker '$fastPathMarker' is missing."
    }
}

$reactivePolicy =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuTenurePolicy.cs"
    ) -Raw -Encoding UTF8

foreach ($marker in @(
    "MovingAverageCycleLength",
    "AcknowledgeDiversification",
    "_increaseFactor",
    "_decreaseFactor"
)) {
    if (-not $reactivePolicy.Contains($marker)) {
        throw "Tabu Search advanced memory: reactive policy missing '$marker'."
    }
}

$reactiveReaction =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\ReactiveTabuReaction.cs"
    ) -Raw -Encoding UTF8

foreach ($marker in @(
    "TabuTenure",
    "TenureChanged",
    "DiversificationRequested",
    "DiversificationMoves"
)) {
    if (-not $reactiveReaction.Contains($marker)) {
        throw "Tabu Search advanced memory: reactive reaction contract missing '$marker'."
    }
}

$repetitionMemory =
    Get-Content -LiteralPath (
        Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TS\ConfigurationRepetitionMemory.cs"
    ) -Raw -Encoding UTF8

if (-not $repetitionMemory.Contains("Dictionary<ulong, Entry>")) {
    throw "Tabu Search advanced memory: repetition memory must use hash-based signature lookup."
}

$rtsPage =
    Get-Content -LiteralPath (
        Join-Path $Root "docs\pages\algorithms\reactive-tabu-search-battiti-tecchiolli-1994.md"
    ) -Raw -Encoding UTF8

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
    "10.1287/ijoc.6.2.126",
    $rtsId
)) {
    if (-not $rtsPage.Contains([string]$marker)) {
        throw "Tabu Search advanced memory: RTS page missing '$marker'."
    }
}

if ($rtsPage.Contains("\(") -or $rtsPage.Contains("\)")) {
    throw "Tabu Search advanced memory: RTS Doxygen page contains raw parenthesized LaTeX delimiters."
}

$builder =
    Get-Content -LiteralPath (
        Join-Path $Root "docs\Build-TabuSearchAdvancedDocumentation.ps1"
    ) -Raw -Encoding UTF8

if ([System.Text.RegularExpressions.Regex]::IsMatch(
        $builder,
        '(?im)\$home\b')) {
    throw "Tabu Search advanced memory: documentation builder must not use PowerShell automatic variable HOME."
}

if ($builder.Contains('$entry.doi')) {
    throw "Tabu Search advanced memory: documentation builder must access optional DOI through PSObject.Properties under StrictMode."
}

if (-not $builder.Contains("Get-OptionalPropertyString")) {
    throw "Tabu Search advanced memory: documentation builder is missing StrictMode-safe optional-property access."
}

$advancedTests =
    Get-Content -LiteralPath (
        Join-Path $Root "tests\MetaheuristicsPlatform.Tests\ReactiveTabuSearchAdvancedTests.cs"
    ) -Raw -Encoding UTF8

$factCount =
    [System.Text.RegularExpressions.Regex]::Matches(
        $advancedTests,
        '\[Fact\]').Count

if ($factCount -lt 25) {
    throw "Tabu Search advanced memory: expected at least 25 focused advanced RTS tests."
}

$optimizeCallCount =
    [System.Text.RegularExpressions.Regex]::Matches(
        $advancedTests,
        'optimizer\.Optimize\(').Count

$testCancellationTokenCount =
    [System.Text.RegularExpressions.Regex]::Matches(
        $advancedTests,
        'TestContext\.Current\.CancellationToken').Count

if ($optimizeCallCount -eq 0) {
    throw "Tabu Search advanced memory: no RTS integration tests found."
}

if ($optimizeCallCount -ne $testCancellationTokenCount) {
    throw (
        "Tabu Search advanced memory: every RTS integration test must pass " +
        "TestContext.Current.CancellationToken (Optimize calls: $optimizeCallCount; tokens: $testCancellationTokenCount).")
}


if (-not $advancedTests.Contains("TabuSearchComponentCatalog.All.Count >= 10")) {
    throw "Tabu Search advanced memory: component-count unit test must use a forward-compatible minimum, not an exact count."
}

Write-Host (
    "Tabu Search advanced memory validation passed: {0} executable, {1} reviewed, {2} total entries." -f
    $implemented,
    $reviewed,
    $entries.Count
) -ForegroundColor Green
