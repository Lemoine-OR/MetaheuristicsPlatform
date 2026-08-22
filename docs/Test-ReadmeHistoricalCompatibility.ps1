[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$readmePath =
    Join-Path $Root "README.md"

if (-not (Test-Path -LiteralPath $readmePath)) {
    throw "README historical compatibility: README.md is missing."
}

$readme =
    [System.IO.File]::ReadAllText(
        $readmePath,
        [System.Text.Encoding]::UTF8)

# Aggregate algorithm counts intentionally belong to Test-ReadmeQuality.ps1 and are not historical markers.
$requiredMarkers = @(
    "public algorithms",
    "### Swarm intelligence",
    "### Evolutionary methods",
    "### Trajectory-based methods",
    "### Constructive methods",
    "### Hybrid / memetic methods",
    "grasp-feo-resende-1995",
    "reactive-grasp-prais-ribeiro-2000",
    "GRASP with Path Relinking",
    "7 executable strategies",
    "generational Evolutionary Path Relinking",
    "Memetic Algorithm - Moscato",
    "memetic-algorithm-moscato-1989",
    "Scatter Search",
    "scatter-search-marti-laguna-glover-2006",
    "iterated-greedy-ruiz-stutzle-2007",
    "Advanced Iterated Greedy Strategies",
    "components/advanced-iterated-greedy-strategies.html",
    "ig.*",
    "threshold-accepting-dueck-scheuer-1990",
    "components/threshold-accepting-schedules.html",
    "## Scientific components",
    "PSO Communication Topology Catalog",
    "components/pso-communication-topologies.html",
    "components/simulated-annealing-cooling-schedules.html",
    "components/acceptance-based-trajectory-methods.html",
    "components/tabu-search-memory-control-strategies.html",
    "components/advanced-variable-neighborhood-search-variants.html",
    "components/path-relinking-strategies.html",
    "components/advanced-scatter-search-strategies.html",
    "components/advanced-genetic-algorithm-operators.html",
    "components/memetic-algorithm-components.html",
    "components/advanced-ant-colony-optimization.html",
    "components/cma-es-components.html"
)

foreach ($marker in $requiredMarkers) {
    if (-not $readme.Contains($marker)) {
        throw "README historical compatibility: missing '$marker'."
    }
}

# Aggregate counts are current-product invariants owned by Test-ReadmeQuality.ps1.
# Historical algorithm validators may keep lower-bound catalog assertions, but they must
# never require a literal aggregate count inside a README marker assertion.
$aggregateCountPattern =
    '\b\d+\s+(?:public algorithms|swarm methods|evolutionary methods|trajectory methods|constructive methods|hybrid / memetic methods)\b'

$readmeAssertionPattern =
    '(?ms)(?:C|Require-Contains)\s+(?:`\s*)?"README\.md"\s+@\((?<markers>.*?)\)'

function Get-FrozenReadmeAggregateCounts(
    [string]$ValidatorText) {

    $found =
        New-Object System.Collections.Generic.List[string]

    $readmeBlocks =
        [regex]::Matches(
            $ValidatorText,
            $readmeAssertionPattern)

    foreach ($readmeBlock in $readmeBlocks) {
        $markers =
            [string]$readmeBlock.Groups["markers"].Value

        $countMatches =
            [regex]::Matches(
                $markers,
                $aggregateCountPattern,
                [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

        foreach ($countMatch in $countMatches) {
            $found.Add(
                [string]$countMatch.Value)
        }
    }

    return (@($found))
}

# Self-test: a historical lower bound in catalog logic is legitimate and must not be
# confused with a literal count required from README.md.
$validGuardFixture =
    @(
        'if ($algorithms.Count -lt 20) { throw "expected at least 20 public algorithms." }',
        'Require-Contains "README.md" @("grasp-feo-resende-1995")'
    ) -join [Environment]::NewLine

if (@(Get-FrozenReadmeAggregateCounts $validGuardFixture).Count -ne 0) {
    throw "README historical compatibility self-test: catalog lower bound produced a false-positive README aggregate count."
}

$invalidGuardFixtures =
    @(
        'Require-Contains "README.md" @("19 trajectory methods","iterated-greedy-ruiz-stutzle-2007")',
        'C "README.md" @("19 trajectory methods","great-deluge-dueck-1993")'
    )

foreach ($invalidGuardFixture in $invalidGuardFixtures) {
    $fixtureMatches =
        @(Get-FrozenReadmeAggregateCounts $invalidGuardFixture)

    if ($fixtureMatches.Count -ne 1 -or
        [string]$fixtureMatches[0] -ne "19 trajectory methods") {
        throw "README historical compatibility self-test: frozen README aggregate count was not detected."
    }
}

$algorithmValidators =
    @(
        Get-ChildItem `
            -LiteralPath (Join-Path $Root "docs") `
            -Filter "Test-*.ps1" `
            -File
    )

foreach ($validator in $algorithmValidators) {
    if ($validator.Name -eq "Test-ReadmeQuality.ps1" -or
        $validator.Name -eq "Test-ReadmeHistoricalCompatibility.ps1") {
        continue
    }

    $validatorText =
        [System.IO.File]::ReadAllText(
            $validator.FullName,
            [System.Text.Encoding]::UTF8)

    $frozenCounts =
        @(Get-FrozenReadmeAggregateCounts $validatorText)

    foreach ($frozenCount in $frozenCounts) {
        $message =
            "README historical compatibility: algorithm validator '$($validator.Name)' freezes aggregate README count '$frozenCount'. Aggregate counts belong only to Test-ReadmeQuality.ps1."

        throw $message
    }
}

Write-Host `
    "Historical-validator README aggregate-count isolation passed: catalog lower bounds are allowed; frozen README counts are forbidden." `
    -ForegroundColor Green

Write-Host `
    "README historical compatibility passed: legacy validator markers preserved inside the redesigned public README." `
    -ForegroundColor Green
