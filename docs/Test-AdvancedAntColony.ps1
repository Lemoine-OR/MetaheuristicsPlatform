[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Advanced ACO validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.45.0") {
    throw "Advanced ACO validation: expected repository version 0.45.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntColonySystemOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\MaxMinAntSystemOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AdvancedAntColonyParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AdvancedAntColonyState.cs",
    "tests\MetaheuristicsPlatform.Tests\AdvancedAntColonyTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\AdvancedAntColonyBenchmarks.cs",
    "docs\advanced-ant-colony-optimization-catalog.json",
    "docs\Build-AdvancedAntColonyDocumentation.ps1",
    "docs\pages\components\advanced-ant-colony-optimization.md",
    "docs\pages\algorithms\ant-colony-system-dorigo-gambardella-1997.md",
    "docs\pages\algorithms\max-min-ant-system-stutzle-hoos-2000.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Advanced ACO validation: missing '$relative'."
    }
}

$acs =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntColonySystemOptimizer.cs"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.AntColonySystem",
    "ExploitationProbability",
    "LocalUpdateRate",
    "GlobalEvaporationRate",
    "selectedKeyUpdate",
    "bestPath"
)) {
    if (-not $acs.Contains($marker)) {
        throw "Advanced ACO validation: ACS is missing '$marker'."
    }
}

$mmas =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\MaxMinAntSystemOptimizer.cs"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.MaxMinAntSystem",
    "MinimumPheromone",
    "MaximumPheromone",
    "BestSource",
    "RestartAfterNonImprovingIterations",
    "pheromones.Evaporate()"
)) {
    if (-not $mmas.Contains($marker)) {
        throw "Advanced ACO validation: MMAS is missing '$marker'."
    }
}

$engine =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemConstructionEngine.cs"

foreach ($marker in @(
    "exploitationProbability",
    "Gumbel-max",
    "selectedKeyUpdate",
    "Math.Log(tau)",
    "Math.Log(eta)"
)) {
    if (-not $engine.Contains($marker)) {
        throw "Advanced ACO validation: shared construction engine is missing '$marker'."
    }
}

$memory =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemPheromoneMemory.cs"

foreach ($marker in @(
    "minimum",
    "maximum",
    "public void Set",
    "public void Reset",
    "Evaporate()"
)) {
    if (-not $memory.Contains($marker)) {
        throw "Advanced ACO validation: shared pheromone memory is missing '$marker'."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AdvancedAntColonyTests.cs"

foreach ($marker in @(
    "AcsUsesExactlyOneObjectiveEvaluationPerConstructedAnt",
    "MmasBudgetStopsInsideColonyWithoutOvershoot",
    "SameSeedProducesSameAcsResult",
    "StableIdsSupportTypedFactoryRegistration"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Advanced ACO validation: focused test '$marker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\advanced-ant-colony-optimization-catalog.json") |
    ConvertFrom-Json

$entries = @($catalog.entries)
$implemented = @($entries | Where-Object status -eq "implemented")
$deferred = @($entries | Where-Object status -eq "reviewed-deferred")

if ($implemented.Count -ne 8 -or $deferred.Count -ne 2) {
    throw "Advanced ACO validation: expected 8 implemented and 2 reviewed/deferred components."
}

foreach ($entry in $entries) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.formulaMode) -or
        [string]::IsNullOrWhiteSpace([string]$entry.formula)) {
        throw "Advanced ACO validation: '$($entry.id)' lacks a scientific model."
    }
}

foreach ($pageRelative in @(
    "docs\pages\algorithms\ant-colony-system-dorigo-gambardella-1997.md",
    "docs\pages\algorithms\max-min-ant-system-stutzle-hoos-2000.md",
    "docs\pages\components\advanced-ant-colony-optimization.md"
)) {
    $page = Read-Utf8 $pageRelative

    if ([regex]::IsMatch($page, '\\\([^\r\n]*?\\\)')) {
        throw "Advanced ACO validation: '$pageRelative' contains legacy inline Doxygen math."
    }

    if ([regex]::IsMatch($page, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
        throw "Advanced ACO validation: '$pageRelative' contains legacy display Doxygen math."
    }
}

Write-Host `
    "Advanced ACO validation passed: ACS + MMAS public algorithms, shared construction engine/memory, 8 executable mechanisms, 2 reviewed/deferred variants." `
    -ForegroundColor Green
