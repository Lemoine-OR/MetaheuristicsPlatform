[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Restart CMA-ES validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.48.0") {
    throw "Restart CMA-ES validation: expected repository version 0.48.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\CMAES\RestartCmaEsKernel.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\RestartCmaEsParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\RestartCmaEsState.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\IpopCmaEsOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\BipopCmaEsOptimizer.cs",
    "tests\MetaheuristicsPlatform.Tests\RestartCmaEsTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\RestartCmaEsBenchmarks.cs",
    "docs\pages\algorithms\ipop-cma-es-auger-hansen-2005.md",
    "docs\pages\algorithms\bipop-cma-es-hansen-2009.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Restart CMA-ES validation: missing '$relative'."
    }
}

$kernel =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CMAES\RestartCmaEsKernel.cs"

foreach ($marker in @(
    "new OptimizationContext<double[]>",
    "context.Evaluate(",
    "context.EvaluateStopping(",
    "context.CompleteIteration(",
    "ResolveLargePopulation",
    "ResolveBipopSmallPopulation",
    "largeBudget <= smallBudget",
    "uniform *",
    "sigma0 *"
)) {
    if (-not $kernel.Contains($marker)) {
        throw "Restart CMA-ES validation: kernel is missing '$marker'."
    }
}

if (($kernel.Split(
        [string[]]@("new OptimizationContext<double[]>"),
        [System.StringSplitOptions]::None).Count - 1) -ne 1) {
    throw "Restart CMA-ES validation: exactly one global OptimizationContext must be created."
}

$catalog =
    (Read-Utf8 "docs\cma-es-component-catalog.json") |
    ConvertFrom-Json

foreach ($id in @(
    "cma.restart.ipop",
    "cma.restart.bipop"
)) {
    $entry =
        @($catalog.entries | Where-Object { [string]$_.id -eq $id })

    if ($entry.Count -ne 1 -or
        [string]$entry[0].status -ne "implemented" -or
        [string]$entry[0].formulaMode -ne "math" -or
        [string]::IsNullOrWhiteSpace([string]$entry[0].doi)) {
        throw "Restart CMA-ES validation: '$id' is not one implemented mathematical component."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\RestartCmaEsTests.cs"

foreach ($marker in @(
    "IpopDoublesPopulationAcrossRestarts",
    "BipopBalancesSmallAndLargeBudgets",
    "GlobalEvaluationBudgetNeverOvershootsAcrossRestarts",
    "BipopIsDeterministicForSameSeed",
    "RestartIdsAreRegisteredByFactory"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Restart CMA-ES validation: focused test '$marker' is missing."
    }
}

foreach ($pageRelative in @(
    "docs\pages\algorithms\ipop-cma-es-auger-hansen-2005.md",
    "docs\pages\algorithms\bipop-cma-es-hansen-2009.md"
)) {
    $page = Read-Utf8 $pageRelative

    if ([regex]::IsMatch($page, '\\\([^\r\n]*?\\\)')) {
        throw "Restart CMA-ES validation: '$pageRelative' contains legacy inline Doxygen math."
    }

    if ([regex]::IsMatch($page, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
        throw "Restart CMA-ES validation: '$pageRelative' contains legacy display Doxygen math."
    }
}

Write-Host `
    "Restart CMA-ES validation passed: IPOP + BIPOP executable under one exact global OptimizationContext and evaluation lifecycle." `
    -ForegroundColor Green
