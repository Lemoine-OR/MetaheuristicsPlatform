[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Advanced CMA-ES validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.47.0") {
    throw "Advanced CMA-ES validation: expected repository version 0.47.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\CMAES\AdvancedCmaEsKernel.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\ActiveCmaEsOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\SeparableCmaEsOptimizer.cs",
    "tests\MetaheuristicsPlatform.Tests\AdvancedCmaEsTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\AdvancedCmaEsBenchmarks.cs",
    "docs\pages\algorithms\active-cma-es-hansen-ros-2010.md",
    "docs\pages\algorithms\separable-cma-es-ros-hansen-2008.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Advanced CMA-ES validation: missing '$relative'."
    }
}

$kernel =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CMAES\AdvancedCmaEsKernel.cs"

foreach ($marker in @(
    "ActiveFullCovariance",
    "SeparableCovariance",
    "BuildActiveWeights",
    "zeroCrossing",
    "(populationSize + 1.0)",
    "ApplyInverseSquareRoot",
    "ResolveSeparableCovarianceLearningRate",
    "double retention",
    "hSigma",
    "Never update the distribution from a partial generation",
    "MaximumActiveCmaEsGenerations",
    "MaximumSeparableCmaEsGenerations"
)) {
    if (-not $kernel.Contains($marker)) {
        throw "Advanced CMA-ES validation: kernel is missing '$marker'."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsReferences.cs"

foreach ($doi in @(
    "10.1145/1830761.1830788",
    "10.1109/CEC.2006.1688662",
    "10.1007/978-3-540-87700-4_30",
    "10.1109/CEC.2005.1554902",
    "10.1145/1570256.1570333"
)) {
    if (-not $references.Contains($doi)) {
        throw "Advanced CMA-ES validation: reference DOI '$doi' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\cma-es-component-catalog.json") |
    ConvertFrom-Json

$implemented =
    @($catalog.entries | Where-Object status -eq "implemented")

$deferred =
    @($catalog.entries | Where-Object status -eq "reviewed-deferred")

if ($implemented.Count -ne 8 -or $deferred.Count -ne 2) {
    throw "Advanced CMA-ES validation: expected 8 implemented and 2 reviewed/deferred components."
}

foreach ($id in @(
    "cma.covariance.active",
    "cma.variant.separable"
)) {
    $entry =
        @($catalog.entries | Where-Object { [string]$_.id -eq $id })

    if ($entry.Count -ne 1 -or
        [string]$entry[0].status -ne "implemented" -or
        [string]$entry[0].formulaMode -ne "math") {
        throw "Advanced CMA-ES validation: '$id' is not one executable mathematical component."
    }
}

foreach ($id in @(
    "cma.restart.ipop",
    "cma.restart.bipop"
)) {
    $entry =
        @($catalog.entries | Where-Object { [string]$_.id -eq $id })

    if ($entry.Count -ne 1 -or
        [string]$entry[0].status -ne "reviewed-deferred") {
        throw "Advanced CMA-ES validation: restart identity '$id' must remain reviewed/deferred in v0.47."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AdvancedCmaEsTests.cs"

foreach ($marker in @(
    "EvaluationBudgetStopsInsideGenerationWithoutOvershoot",
    "ActiveDefaultOddPopulationCompletesOneGeneration",
    "ActiveSameSeedProducesSameResult",
    "SeparableSameSeedProducesSameResult",
    "AdvancedIdsAreRegisteredByFactory"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Advanced CMA-ES validation: focused test '$marker' is missing."
    }
}

foreach ($pageRelative in @(
    "docs\pages\algorithms\active-cma-es-hansen-ros-2010.md",
    "docs\pages\algorithms\separable-cma-es-ros-hansen-2008.md"
)) {
    $page = Read-Utf8 $pageRelative

    if ([regex]::IsMatch($page, '\\\([^\r\n]*?\\\)')) {
        throw "Advanced CMA-ES validation: '$pageRelative' contains legacy inline Doxygen math."
    }

    if ([regex]::IsMatch($page, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
        throw "Advanced CMA-ES validation: '$pageRelative' contains legacy display Doxygen math."
    }
}

Write-Host `
    "Advanced CMA-ES validation passed: weighted Active CMA-ES + linear-memory sep-CMA-ES executable; IPOP/BIPOP remain scientifically distinct and deferred." `
    -ForegroundColor Green
