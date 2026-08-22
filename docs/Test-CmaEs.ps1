[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "CMA-ES validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if ([version]([string]$version.version) -lt [version]"0.46.0") {
    throw "CMA-ES validation: expected repository version 0.46.0 or later."
}

$required = @(
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsComponentIds.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsReferences.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsParameters.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsState.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsGaussianSampler.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsSymmetricEigenSolver.cs",
    "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsOptimizer.cs",
    "tests\MetaheuristicsPlatform.Tests\CmaEsTests.cs",
    "benchmarks\MetaheuristicsPlatform.Benchmarks\CmaEsBenchmarks.cs",
    "docs\cma-es-component-catalog.json",
    "docs\Build-CmaEsDocumentation.ps1",
    "docs\pages\components\cma-es-components.md",
    "docs\pages\algorithms\cma-es-hansen-ostermeier-2001.md"
)

foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $relative))) {
        throw "CMA-ES validation: missing '$relative'."
    }
}

$optimizer =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsOptimizer.cs"

foreach ($marker in @(
    "MetaheuristicAlgorithmIds.CmaEs",
    "ISpanContinuousOptimizationProblem",
    "BuildPositiveWeights",
    "muEffective",
    "pSigma",
    "pC",
    "cSigma",
    "cMu",
    "CmaEsSymmetricEigenSolver.Decompose",
    "CmaEsSymmetricEigenSolver.ReconstructPositiveDefinite",
    "CMA-ES requires finite objective values.",
    "context.Evaluate(",
    "context.EvaluateStopping",
    "MaximumCmaEsGenerations"
)) {
    if (-not $optimizer.Contains($marker)) {
        throw "CMA-ES validation: optimizer is missing '$marker'."
    }
}

$eigen =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\CMAES\CmaEsSymmetricEigenSolver.cs"

foreach ($marker in @(
    "Jacobi",
    "ApplyInverseSquareRoot",
    "minimumEigenvalue",
    "axisScales"
)) {
    if (-not $eigen.Contains($marker)) {
        throw "CMA-ES validation: eigensolver is missing '$marker'."
    }
}

$tests =
    Read-Utf8 "tests\MetaheuristicsPlatform.Tests\CmaEsTests.cs"

foreach ($marker in @(
    "CompleteGenerationsUseExactlyLambdaEvaluations",
    "EvaluationBudgetStopsInsideGenerationWithoutOvershoot",
    "SameSeedProducesSameResult",
    "CanonicalIdIsRegisteredByFactory",
    "InitialMeanMustBelongToBoundedSearchSpace"
)) {
    if (-not $tests.Contains($marker)) {
        throw "CMA-ES validation: focused test '$marker' is missing."
    }
}

$catalog =
    (Read-Utf8 "docs\cma-es-component-catalog.json") |
    ConvertFrom-Json

$entries = @($catalog.entries)

foreach ($foundationId in @(
    "cma.sampling.multivariate-normal",
    "cma.recombination.logarithmic-positive",
    "cma.path.cumulation",
    "cma.step-size.csa",
    "cma.covariance.rank-one",
    "cma.covariance.rank-mu"
)) {
    $foundation =
        @($entries | Where-Object { [string]$_.id -eq $foundationId })

    if ($foundation.Count -ne 1 -or
        [string]$foundation[0].status -ne "implemented") {
        throw "CMA-ES validation: foundation identity '$foundationId' is missing, duplicated or not implemented."
    }
}

foreach ($id in @(
    "cma.sampling.multivariate-normal",
    "cma.recombination.logarithmic-positive",
    "cma.path.cumulation",
    "cma.step-size.csa",
    "cma.covariance.rank-one",
    "cma.covariance.rank-mu",
    "cma.covariance.active",
    "cma.variant.separable",
    "cma.restart.ipop",
    "cma.restart.bipop"
)) {
    if (@($entries | Where-Object { [string]$_.id -eq $id }).Count -ne 1) {
        throw "CMA-ES validation: catalog identity '$id' is missing or duplicated."
    }
}

$page =
    Read-Utf8 "docs\pages\algorithms\cma-es-hansen-ostermeier-2001.md"

foreach ($marker in @(
    "@page cma_es_hansen_ostermeier_2001",
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
    "10.1162/106365601750190398",
    "10.1162/106365603321828970"
)) {
    if (-not $page.Contains($marker)) {
        throw "CMA-ES validation: scientific page is missing '$marker'."
    }
}

foreach ($pageRelative in @(
    "docs\pages\algorithms\cma-es-hansen-ostermeier-2001.md",
    "docs\pages\components\cma-es-components.md"
)) {
    $text = Read-Utf8 $pageRelative

    if ([regex]::IsMatch($text, '\\\([^\r\n]*?\\\)')) {
        throw "CMA-ES validation: '$pageRelative' contains legacy inline Doxygen math."
    }

    if ([regex]::IsMatch($text, '(?m)^[ \t]*\\(?:\[|\])[ \t]*$')) {
        throw "CMA-ES validation: '$pageRelative' contains legacy display Doxygen math."
    }
}

Write-Host `
    "CMA-ES validation passed: canonical full covariance + positive logarithmic recombination + CSA + rank-one/rank-mu adaptation + stable v0.46 foundation identities." `
    -ForegroundColor Green
