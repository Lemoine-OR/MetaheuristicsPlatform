[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path =
        Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Continuous Cross-Entropy validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.50.0") {
    throw "Continuous Cross-Entropy validation: expected repository version 0.50.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\CrossEntropy\ContinuousCrossEntropyOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\CrossEntropy\ContinuousCrossEntropyParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\CrossEntropy\ContinuousCrossEntropyState.cs",
    "src\MetaheuristicsPlatform\Algorithms\CrossEntropy\CrossEntropyReferences.cs",
    "tests\MetaheuristicsPlatform.Tests\ContinuousCrossEntropyTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\ContinuousCrossEntropyBenchmarks.cs",
    "docs\pages\algorithms\cross-entropy-continuous-kroese-porotsky-rubinstein-2006.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Continuous Cross-Entropy validation: missing '$relative'."
    }
}

$source =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CrossEntropy\ContinuousCrossEntropyOptimizer.cs"

foreach ($marker in @(
    "eliteStandardDeviation",
    "ResolveDynamicStandardDeviationSmoothing",
    "Math.Ceiling(",
    "context.Evaluate(",
    "context.EvaluateStopping(",
    "context.CompleteIteration(",
    "Never update the sampling distribution from a partial",
    "CrossEntropyDistributionConverged"
)) {
    if (-not $source.Contains($marker)) {
        throw "Continuous Cross-Entropy validation: implementation marker '$marker' is missing."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CrossEntropy\CrossEntropyReferences.cs"

foreach ($doi in @(
    "10.1023/A:1010091220143",
    "10.1007/s10479-005-5724-z",
    "10.1007/s11009-006-9753-0"
)) {
    if (-not $references.Contains($doi)) {
        throw "Continuous Cross-Entropy validation: DOI '$doi' is missing."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\ContinuousCrossEntropyTests.cs"

foreach ($marker in @(
    "TwoCompleteIterationsUseExactlyTwoSamplePopulations",
    "EvaluationBudgetStopsInsideIterationWithoutDistributionUpdate",
    "SameSeedProducesSameResult",
    "InvalidParametersAreRejected",
    "FactoryCreatesContinuousCrossEntropy"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Continuous Cross-Entropy validation: focused test '$marker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\algorithm-catalog.json") |
    ConvertFrom-Json

$entry =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "cross-entropy-continuous-kroese-porotsky-rubinstein-2006"
        }
    )

if ($entry.Count -ne 1) {
    throw "Continuous Cross-Entropy validation: catalog identity is missing or duplicated."
}

if ([string]$entry[0].doi -ne "10.1007/s11009-006-9753-0") {
    throw "Continuous Cross-Entropy validation: primary DOI mismatch."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Continuous Cross-Entropy validation: update mathematics must use aligned display layout."
}

$page =
    Read-Utf8 "docs\pages\algorithms\cross-entropy-continuous-kroese-porotsky-rubinstein-2006.md"

if ([regex]::IsMatch($page, '\\\([^\r\n]*?\\\)')) {
    throw "Continuous Cross-Entropy validation: page contains legacy inline Doxygen math."
}

if ([regex]::IsMatch($page, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
    throw "Continuous Cross-Entropy validation: page contains legacy display Doxygen math."
}

Write-Host `
    "Continuous Cross-Entropy validation passed: elite normal updating + dynamic sigma smoothing + exact partial-iteration accounting." `
    -ForegroundColor Green
