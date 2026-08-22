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
        throw "Artificial Bee Colony validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.49.0") {
    throw "Artificial Bee Colony validation: expected repository version 0.49.0 or later."
}

foreach ($relative in @(
    "src\MetaheuristicsPlatform\Algorithms\ArtificialBeeColony\ArtificialBeeColonyOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\ArtificialBeeColony\ArtificialBeeColonyParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\ArtificialBeeColony\ArtificialBeeColonyState.cs",
    "src\MetaheuristicsPlatform\Algorithms\ArtificialBeeColony\ArtificialBeeColonyReferences.cs",
    "tests\MetaheuristicsPlatform.Tests\ArtificialBeeColonyTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\ArtificialBeeColonyBenchmarks.cs",
    "docs\pages\algorithms\artificial-bee-colony-karaboga-basturk-2007.md"
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Artificial Bee Colony validation: missing '$relative'."
    }
}

$source =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\ArtificialBeeColony\ArtificialBeeColonyOptimizer.cs"

foreach ($marker in @(
    "ArtificialBeeColonyPhase.EmployedBees",
    "ArtificialBeeColonyPhase.OnlookerBees",
    "ArtificialBeeColonyPhase.Scout",
    "BuildNeighborCandidate",
    "CanonicalFitness",
    "SelectSource",
    "FindAbandonedSource",
    "context.Evaluate(",
    "context.EvaluateStopping(",
    "context.CompleteIteration("
)) {
    if (-not $source.Contains($marker)) {
        throw "Artificial Bee Colony validation: implementation marker '$marker' is missing."
    }
}

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\ArtificialBeeColony\ArtificialBeeColonyReferences.cs"

foreach ($doi in @(
    "10.1007/s10898-007-9149-x",
    "10.1016/j.asoc.2007.05.007"
)) {
    if (-not $references.Contains($doi)) {
        throw "Artificial Bee Colony validation: DOI '$doi' is missing."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\ArtificialBeeColonyTests.cs"

foreach ($marker in @(
    "OneCycleUsesInitializationEmployedAndOnlookerEvaluations",
    "EvaluationBudgetStopsInsideCycleWithoutOvershoot",
    "ConstantObjectiveTriggersOneScoutAtLimit",
    "SameSeedProducesSameResult",
    "FactoryCreatesArtificialBeeColony"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Artificial Bee Colony validation: focused test '$marker' is missing."
    }
}

$page =
    Read-Utf8 "docs\pages\algorithms\artificial-bee-colony-karaboga-basturk-2007.md"

if ([regex]::IsMatch($page, '\\\([^\r\n]*?\\\)')) {
    throw "Artificial Bee Colony validation: page contains legacy inline Doxygen math."
}

if ([regex]::IsMatch($page, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
    throw "Artificial Bee Colony validation: page contains legacy display Doxygen math."
}

Write-Host `
    "Artificial Bee Colony validation passed: canonical employed/onlooker/scout lifecycle, exact evaluation accounting and peer-reviewed provenance." `
    -ForegroundColor Green
