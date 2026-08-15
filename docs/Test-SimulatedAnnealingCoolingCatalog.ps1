[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath =
    Join-Path $Root "docs\sa-cooling-catalog.json"

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "SA cooling catalog validation: missing docs/sa-cooling-catalog.json."
}

$catalog =
    Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 |
    ConvertFrom-Json

$entries = @($catalog.entries)

if ($entries.Count -lt 13) {
    throw "SA cooling catalog validation: expected at least 13 reviewed entries."
}

$requiredFields = @(
    "id",
    "name",
    "availability",
    "scope",
    "formula",
    "parameters",
    "asymptotic",
    "assumptions",
    "convergence",
    "reference",
    "source"
)

$ids = @{}
$implemented = 0
$reviewedComposite = 0

$pagePath =
    Join-Path $Root "docs\pages\components\simulated-annealing-cooling-schedules.md"

if (-not (Test-Path -LiteralPath $pagePath)) {
    throw "SA cooling catalog validation: missing scientific cooling-catalog page."
}

$page =
    Get-Content -LiteralPath $pagePath -Raw -Encoding UTF8

if ($page.Contains("\(") -or
    $page.Contains("\)")) {
    throw "SA cooling catalog validation: Doxygen Markdown must use \f$...\f$ for inline mathematics, not \(...\)."
}

$coolingDocumentationBuilderPath =
    Join-Path $Root "docs\Build-SimulatedAnnealingCoolingDocumentation.ps1"

if (-not (Test-Path -LiteralPath $coolingDocumentationBuilderPath)) {
    throw "SA cooling catalog validation: missing SA cooling documentation builder."
}

$coolingDocumentationBuilderSource =
    Get-Content -LiteralPath $coolingDocumentationBuilderPath -Raw -Encoding UTF8

if ([System.Text.RegularExpressions.Regex]::IsMatch(
        $coolingDocumentationBuilderSource,
        '(?im)\$home\b')) {
    throw "SA cooling catalog validation: documentation builder must not use PowerShell automatic variable name HOME (case-insensitive)."
}

$enumPath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\SA\SimulatedAnnealingCoolingScheduleKind.cs"
$idsPath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\SA\SimulatedAnnealingCoolingScheduleIds.cs"
$runtimeCatalogPath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\SA\SimulatedAnnealingCoolingScheduleCatalog.cs"
$parametersPath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\SA\SimulatedAnnealingParameters.cs"
$optimizerPath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\SA\SimulatedAnnealingOptimizer.cs"

foreach ($path in @($enumPath,$idsPath,$runtimeCatalogPath,$parametersPath,$optimizerPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "SA cooling catalog validation: required source file missing: '$path'."
    }
}

$enumSource = Get-Content -LiteralPath $enumPath -Raw -Encoding UTF8
$idsSource = Get-Content -LiteralPath $idsPath -Raw -Encoding UTF8
$runtimeCatalogSource = Get-Content -LiteralPath $runtimeCatalogPath -Raw -Encoding UTF8
$parametersSource = Get-Content -LiteralPath $parametersPath -Raw -Encoding UTF8
$optimizerSource = Get-Content -LiteralPath $optimizerPath -Raw -Encoding UTF8

foreach ($entry in $entries) {
    foreach ($field in $requiredFields) {
        $value = $entry.$field
        if ($null -eq $value -or
            [string]::IsNullOrWhiteSpace([string]$value)) {
            throw "SA cooling catalog validation: '$($entry.id)' is missing '$field'."
        }
    }

    $id = [string]$entry.id

    if ($ids.ContainsKey($id)) {
        throw "SA cooling catalog validation: duplicate stable ID '$id'."
    }
    $ids[$id] = $true

    if (-not $page.Contains($id)) {
        throw "SA cooling catalog validation: scientific page is missing stable ID '$id'."
    }

    $doi = [string]$entry.doi
    if (-not [string]::IsNullOrWhiteSpace($doi) -and
        -not $page.Contains($doi)) {
        throw "SA cooling catalog validation: scientific page is missing DOI '$doi' for '$id'."
    }

    switch ([string]$entry.availability) {
        "implemented" {
            $implemented++

            foreach ($field in @("kind","implementationClass","sourcePath")) {
                $value = $entry.$field
                if ($null -eq $value -or
                    [string]::IsNullOrWhiteSpace([string]$value)) {
                    throw "SA cooling catalog validation: implemented entry '$id' is missing '$field'."
                }
            }

            $sourcePath =
                Join-Path $Root ([string]$entry.sourcePath)

            if (-not (Test-Path -LiteralPath $sourcePath)) {
                throw "SA cooling catalog validation: source path does not exist for '$id': '$($entry.sourcePath)'."
            }

            $implementationSource =
                Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8

            if (-not $implementationSource.Contains("public string Id")) {
                throw "SA cooling catalog validation: implementation '$($entry.implementationClass)' does not expose the required cooling-schedule Id contract."
            }

            $kindToken =
                "SimulatedAnnealingCoolingScheduleKind.$([string]$entry.kind)"

            if (-not $parametersSource.Contains($kindToken)) {
                throw "SA cooling catalog validation: parameter factory is missing '$kindToken'."
            }

            if (-not $runtimeCatalogSource.Contains($kindToken)) {
                throw "SA cooling catalog validation: runtime catalog is missing '$kindToken'."
            }

            if (-not $enumSource.Contains([string]$entry.kind)) {
                throw "SA cooling catalog validation: enum is missing '$($entry.kind)'."
            }

            if (-not $runtimeCatalogSource.Contains([string]$entry.implementationClass)) {
                throw "SA cooling catalog validation: runtime catalog is missing implementation '$($entry.implementationClass)'."
            }

            if (-not $idsSource.Contains('"' + $id + '"')) {
                throw "SA cooling catalog validation: stable ID '$id' is missing from SimulatedAnnealingCoolingScheduleIds."
            }
        }

        "reviewed-composite" {
            $reviewedComposite++

            if (-not [string]::IsNullOrWhiteSpace([string]$entry.sourcePath)) {
                throw "SA cooling catalog validation: reviewed-composite entry '$id' must not pretend to have an implementation source path."
            }
        }

        default {
            throw "SA cooling catalog validation: unsupported availability '$($entry.availability)' for '$id'."
        }
    }
}

if ($implemented -ne 10) {
    throw "SA cooling catalog validation: expected exactly 10 implemented built-in schedules, found $implemented."
}

if ($reviewedComposite -lt 3) {
    throw "SA cooling catalog validation: expected at least 3 reviewed-composite controllers."
}

if (-not $parametersSource.Contains("CustomCoolingSchedule")) {
    throw "SA cooling catalog validation: custom cooling-schedule extension point is missing."
}

foreach ($marker in @(
    "ISimulatedAnnealingStatisticalCoolingSchedule",
    "SimulatedAnnealingLevelStatisticsAccumulator",
    "LevelObjectiveVariance"
)) {
    if (-not $optimizerSource.Contains($marker)) {
        throw "SA cooling catalog validation: optimizer integration marker '$marker' is missing."
    }
}

$versionPath =
    Join-Path $Root "version.json"

$version =
    Get-Content -LiteralPath $versionPath -Raw -Encoding UTF8 |
    ConvertFrom-Json

if ([string]$version.version -ne "0.20.0") {
    throw "SA cooling catalog validation: version.json must be 0.20.0 for this pack."
}

Write-Host (
    "SA cooling catalog validation passed: {0} implemented, {1} reviewed-composite, {2} total reviewed entries." -f
    $implemented,
    $reviewedComposite,
    $entries.Count) -ForegroundColor Green
