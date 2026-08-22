[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Ant System validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$requiredFiles = @(
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntColonyContracts.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemConstructionEngine.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemPheromoneMemory.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemDepositPolicies.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemOptimizer.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemState.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemReferences.cs",
    "src\MetaheuristicsPlatform\Algorithms\AntColony\AntColonyComponentIds.cs",
    "tests\MetaheuristicsPlatform.Tests\AntSystemTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\AntSystemBenchmarks.cs",
    "docs\ant-colony-optimization-catalog.json",
    "docs\pages\algorithms\ant-system-dorigo-maniezzo-colorni-1996.md"
)

foreach ($relative in $requiredFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "Ant System validation: missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemOptimizer.cs"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.AntSystem",
    "MetaheuristicFamily.SwarmIntelligence",
    "MetaheuristicFamily.Constructive",
    "pheromones.Evaporate()",
    "_depositPolicy.GetDeposit",
    "context.Evaluate(",
    "context.EvaluateStopping",
    "MaximumAntSystemIterations"
)) {
    if (-not $optimizer.Contains($marker)) {
        throw "Ant System validation: optimizer is missing '$marker'."
    }
}

$engine =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemConstructionEngine.cs"

foreach ($marker in @(
    "Gumbel-max",
    "Math.Log(tau)",
    "Math.Log(eta)",
    "GetPheromoneKey",
    "EvaluateHeuristic",
    "MaximumConstructionSteps"
)) {
    if (-not $engine.Contains($marker)) {
        throw "Ant System validation: construction engine is missing '$marker'."
    }
}

$memory =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemPheromoneMemory.cs"

foreach ($marker in @(
    "Evaporate()",
    "_evaporationRounds++",
    "Math.Pow(_retention, rounds)",
    "double.Epsilon"
)) {
    if (-not $memory.Contains($marker)) {
        throw "Ant System validation: pheromone memory is missing '$marker'."
    }
}

$deposit =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\AntColony\AntSystemDepositPolicies.cs"

foreach ($marker in @(
    "ConstantAntSystemDepositPolicy",
    "PositiveInverseObjectiveAntSystemDepositPolicy",
    "OptimizationSense.Minimize",
    "_q / objective"
)) {
    if (-not $deposit.Contains($marker)) {
        throw "Ant System validation: deposit policies are missing '$marker'."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\AntSystemTests.cs"

foreach ($marker in @(
    "FullIterationsUseExactlyOneObjectiveEvaluationPerAnt",
    "GlobalEvaluationBudgetStopsInsideColonyWithoutOvershoot",
    "SameSeedProducesSameResult",
    "PositiveInverseDepositRejectsUnsupportedObjectiveScales",
    "StableIdSupportsTypedFactoryRegistration"
)) {
    if (-not $tests.Contains($marker)) {
        throw "Ant System validation: focused test '$marker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\ant-colony-optimization-catalog.json") |
    ConvertFrom-Json

$entries = @($catalog.entries)

if ($entries.Count -ne 6) {
    throw "Ant System validation: expected exactly six ACO catalog entries."
}

$implemented =
    @($entries | Where-Object { [string]$_.status -eq "implemented" })

$deferred =
    @($entries | Where-Object { [string]$_.status -eq "reviewed-deferred" })

if ($implemented.Count -ne 4 -or $deferred.Count -ne 2) {
    throw "Ant System validation: expected four implemented and two reviewed/deferred entries."
}

foreach ($id in @(
    "aco.transition.ant-system-proportional",
    "aco.update.all-ants",
    "aco.deposit.constant",
    "aco.deposit.inverse-positive-objective",
    "aco.variant.ant-colony-system",
    "aco.variant.max-min-ant-system"
)) {
    if (@($entries | Where-Object { [string]$_.id -eq $id }).Count -ne 1) {
        throw "Ant System validation: catalog identity '$id' is missing or duplicated."
    }
}

$page =
    Read-Utf8 "docs\pages\algorithms\ant-system-dorigo-maniezzo-colorni-1996.md"

foreach ($marker in @(
    "@page ant_system_dorigo_maniezzo_colorni_1996",
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
    "10.1109/3477.484436",
    "10.1109/4235.585892",
    "10.1016/S0167-739X(00)00043-1"
)) {
    if (-not $page.Contains($marker)) {
        throw "Ant System validation: scientific page is missing '$marker'."
    }
}


# ACO Markdown/Doxygen delimiter regression guard
if ([regex]::IsMatch(
        $page,
        '\\\([^\r\n]*?\\\)')) {
    throw "Ant System validation: scientific page contains legacy inline Doxygen math delimiters."
}

if ([regex]::IsMatch(
        $page,
        '(?m)^[ \t]*\\\[[ \t]*$') -or
    [regex]::IsMatch(
        $page,
        '(?m)^[ \t]*\\\][ \t]*$')) {
    throw "Ant System validation: scientific page contains legacy display Doxygen math delimiters."
}

Write-Host `
    "Ant System validation passed: generic proportional construction + lazy exact evaporation + all-ant reinforcement + 4 implemented ACO components + ACS/MMAS reviewed/deferred." `
    -ForegroundColor Green
