[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath =
    Join-Path $Root "docs\algorithm-catalog.json"

if (-not (Test-Path $catalogPath)) {
    throw "Missing docs/algorithm-catalog.json."
}

$catalog =
    Get-Content $catalogPath -Raw |
    ConvertFrom-Json

$algorithms = @($catalog.algorithms)
$families = @($catalog.families)

if ($algorithms.Count -lt 9) {
    throw "Documentation parity: expected at least the nine currently public algorithms."
}

$requiredFields = @(
    "id","name","class","category","kind","family","time","space",
    "applicability","sourcePath","publication","doi","implementation",
    "page","problem","update","assumptions","convergence","factoryMode"
)

$ids = @{}

foreach ($algorithm in $algorithms) {
    foreach ($field in $requiredFields) {
        $value = $algorithm.$field

        if ($null -eq $value -or
            [string]::IsNullOrWhiteSpace([string]$value)) {
            throw "Documentation parity: '$($algorithm.id)' is missing '$field'."
        }
    }

    if ($ids.ContainsKey($algorithm.id)) {
        throw "Documentation parity: duplicate stable ID '$($algorithm.id)'."
    }

    $ids[$algorithm.id] = $true

    $pagePath =
        Join-Path $Root $algorithm.page

    if (-not (Test-Path $pagePath)) {
        throw "Documentation parity: missing page '$($algorithm.page)'."
    }

    $page =
        Get-Content $pagePath -Raw

    $requiredPageMarkers = @(
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
        $algorithm.id,
        $algorithm.doi
    )

    foreach ($marker in $requiredPageMarkers) {
        if (-not $page.Contains([string]$marker)) {
            throw "Documentation parity: '$($algorithm.id)' page is missing '$marker'."
        }
    }

    $sourcePath =
        Join-Path $Root $algorithm.sourcePath

    if (-not (Test-Path $sourcePath)) {
        throw "Documentation parity: catalog source path does not exist: '$($algorithm.sourcePath)'."
    }
}

foreach ($family in $families) {
    $familyPath =
        Join-Path $Root "docs\pages\families\$($family.id).md"

    if (-not (Test-Path $familyPath)) {
        throw "Documentation parity: missing family page '$($family.id)'."
    }
}

$requiredRepoFiles = @(
    "README.md",
    "CHANGELOG.md",
    "version.json",
    "API-STABILITY.md",
    "CITATION.cff",
    "docs\Doxyfile",
    "docs\mainpage.md",
    "docs\build-documentation.ps1",
    "docs\Test-DocumentationLinks.ps1",
    "docs\Test-DocumentationParity.ps1",
    "docs\Test-SimulatedAnnealingCoolingCatalog.ps1",
    "docs\Build-SimulatedAnnealingCoolingDocumentation.ps1",
    "docs\sa-cooling-catalog.json",
    "docs\pages\components\simulated-annealing-cooling-schedules.md",
    "docs\pages\algorithms\tabu-search-glover.md",
    "docs\Test-TabuSearchFoundation.ps1",
    "docs\Test-TabuSearchAdvancedMemory.ps1",
    "docs\Build-TabuSearchAdvancedDocumentation.ps1",
    "docs\ts-memory-control-catalog.json",
    "docs\pages\components\tabu-search-memory-control-strategies.md",
    "docs\pages\algorithms\reactive-tabu-search-battiti-tecchiolli-1994.md",
    "docs\doxygen-custom.css",
    "docs\assets\algorithms-icon.svg",
    "docs\assets\metaheuristicsplatform-logo.svg",
    ".github\workflows\build.yml",
    ".github\workflows\documentation.yml",
    ".github\workflows\release.yml",
    "build\Build-Validated.ps1",
    "build\Build-All.ps1",
    "build\Prepare-ReleaseAssets.ps1",
    "tools\Test-PowerShellSyntax.ps1",
    "tools\Test-Automation.ps1",
    "tools\Get-BuildTarget.ps1"
)

foreach ($relative in $requiredRepoFiles) {
    if (-not (Test-Path (Join-Path $Root $relative))) {
        throw "Documentation parity: required ULSAlgorithms-parity file missing: '$relative'."
    }
}

$readme =
    Get-Content (Join-Path $Root "README.md") -Raw

foreach ($algorithm in $algorithms) {
    if (-not $readme.Contains($algorithm.id)) {
        throw "Documentation parity: README panel missing stable ID '$($algorithm.id)'."
    }
}


$idsSourcePath =
    Join-Path $Root "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs"

if (-not (Test-Path $idsSourcePath)) {
    throw "Documentation parity: MetaheuristicAlgorithmIds.cs is missing."
}

$idsSource =
    Get-Content $idsSourcePath -Raw

foreach ($algorithm in $algorithms) {
    if (-not $idsSource.Contains('"' + $algorithm.id + '"')) {
        throw "Documentation parity: stable ID '$($algorithm.id)' is not declared in MetaheuristicAlgorithmIds."
    }
}

$version =
    Get-Content (Join-Path $Root "version.json") -Raw |
    ConvertFrom-Json

if ([string]$version.version -ne "0.22.0") {
    throw "Documentation parity: version.json must be 0.22.0 for this pack."
}

& (Join-Path $Root "docs\Test-SimulatedAnnealingCoolingCatalog.ps1") -Root $Root
& (Join-Path $Root "docs\Test-TabuSearchFoundation.ps1") -Root $Root
& (Join-Path $Root "docs\Test-TabuSearchAdvancedMemory.ps1") -Root $Root

Write-Host "Documentation parity validation passed: $($algorithms.Count) algorithms, $($families.Count) family pages." -ForegroundColor Green
