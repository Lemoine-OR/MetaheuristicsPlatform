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
        throw "Firefly validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.51.0") {
    throw "Firefly validation: expected repository version 0.51.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\Firefly\FireflyOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\Firefly\FireflyParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\Firefly\FireflyState.cs",
    "src\MetaheuristicsPlatform\Algorithms\Firefly\FireflyReferences.cs",
    "tests\MetaheuristicsPlatform.Tests\FireflyAlgorithmTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\FireflyAlgorithmBenchmarks.cs",
    "docs\pages\algorithms\firefly-algorithm-yang-2009.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Firefly validation: missing '$relative'."
    }
}

$source =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\Firefly\FireflyOptimizer.cs"

foreach ($marker in @(
    "SquaredDistance(",
    "Math.Exp(exponent)",
    "random.NextDouble() - 0.5",
    "context.Evaluate(",
    "context.EvaluateStopping(",
    "context.CompleteIteration(",
    "partial pairwise sweep",
    "MaximumFireflyIterations"
)) {
    if (-not $source.Contains($marker)) {
        throw "Firefly validation: implementation marker '$marker' is missing."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\Firefly\FireflyReferences.cs"

foreach ($doi in @(
    "10.1007/978-3-642-04944-6_14",
    "10.1504/IJBIC.2010.032124"
)) {
    if (-not $references.Contains($doi)) {
        throw "Firefly validation: DOI '$doi' is missing."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\FireflyAlgorithmTests.cs"

foreach ($marker in @(
    "TwoFirefliesWithPureAttractionPerformExactlyOneMove",
    "EvaluationBudgetStopsInsidePairwiseSweepWithoutCountingIteration",
    "ConstantBrightnessCompletesIterationWithoutAttractionEvaluations",
    "MaximizationUsesObjectiveSenseSymmetrically",
    "SameSeedProducesSameResult",
    "FactoryCreatesFireflyAlgorithm"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Firefly validation: focused test '$marker' is missing."
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
                "firefly-algorithm-yang-2009"
        }
    )

if ($entry.Count -ne 1) {
    throw "Firefly validation: catalog identity is missing or duplicated."
}

if ([string]$entry[0].doi -ne "10.1007/978-3-642-04944-6_14") {
    throw "Firefly validation: primary DOI mismatch."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Firefly validation: update mathematics must use aligned display layout."
}

$page =
    Read-Utf8 "docs\pages\algorithms\firefly-algorithm-yang-2009.md"

if ([regex]::IsMatch($page, '\\\([^\r\n]*?\\\)')) {
    throw "Firefly validation: page contains legacy inline Doxygen math."
}

if ([regex]::IsMatch($page, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
    throw "Firefly validation: page contains legacy display Doxygen math."
}

Write-Host `
    "Firefly validation passed: distance-decaying attraction + canonical randomization + exact partial-sweep accounting." `
    -ForegroundColor Green
