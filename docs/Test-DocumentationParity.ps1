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
    [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$algorithms = @($catalog.algorithms)
$families = @($catalog.families)

if ($algorithms.Count -lt 40) {
    throw "Documentation parity: expected at least the forty currently public algorithms."
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
        [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)

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
    "docs\Test-TextEncoding.ps1",
    "docs\Test-SimulatedAnnealingCoolingCatalog.ps1",
    "docs\Build-SimulatedAnnealingCoolingDocumentation.ps1",
    "docs\sa-cooling-catalog.json",
    "docs\pages\components\simulated-annealing-cooling-schedules.md",
    "docs\Test-ThresholdAccepting.ps1",
    "docs\threshold-accepting-schedule-catalog.json",
    "docs\Build-ThresholdAcceptingScheduleDocumentation.ps1",
    "docs\pages\components\threshold-accepting-schedules.md",
    "docs\pages\algorithms\threshold-accepting-dueck-scheuer-1990.md",
    "docs\Test-DueckAcceptanceMethods.ps1",
    "docs\acceptance-based-trajectory-catalog.json",
    "docs\Build-AcceptanceBasedTrajectoryDocumentation.ps1",
    "docs\pages\components\acceptance-based-trajectory-methods.md",
    "docs\pages\algorithms\great-deluge-dueck-1993.md",
    "docs\pages\algorithms\record-to-record-travel-dueck-1993.md",
    "docs\pages\algorithms\late-acceptance-hill-climbing-burke-bykov-2017.md",
    "docs\Test-LateAcceptance.ps1",
    "docs\Test-DemonBasedAcceptance.ps1",
    "docs\Test-IteratedGreedy.ps1",
    "docs\Test-AdvancedIteratedGreedy.ps1",
    "docs\Test-ScatterSearch.ps1",
    "docs\Test-AdvancedScatterSearch.ps1",
    "docs\Test-GeneticAlgorithm.ps1",
    "docs\Test-AdvancedGeneticAlgorithm.ps1",
    "docs\Test-MemeticAlgorithm.ps1",
    "docs\Test-AntSystem.ps1",
    "docs\Test-AdvancedAntColony.ps1",
    "docs\Test-ArtificialBeeColony.ps1",
    "docs\Test-ReadmeQuality.ps1",
    "docs\Test-ReadmeHistoricalCompatibility.ps1",
    "docs\pages\algorithms\artificial-bee-colony-karaboga-basturk-2007.md",
    "docs\Test-CmaEs.ps1",
    "docs\Test-AdvancedCmaEs.ps1",
    "docs\Test-RestartCmaEs.ps1",
    "docs\cma-es-component-catalog.json",
    "docs\Build-CmaEsDocumentation.ps1",
    "docs\pages\components\cma-es-components.md",
    "docs\pages\algorithms\cma-es-hansen-ostermeier-2001.md",
    "docs\pages\algorithms\active-cma-es-hansen-ros-2010.md",
    "docs\pages\algorithms\separable-cma-es-ros-hansen-2008.md",
    "docs\pages\algorithms\ipop-cma-es-auger-hansen-2005.md",
    "docs\pages\algorithms\bipop-cma-es-hansen-2009.md",
    "docs\advanced-ant-colony-optimization-catalog.json",
    "docs\Build-AdvancedAntColonyDocumentation.ps1",
    "docs\pages\components\advanced-ant-colony-optimization.md",
    "docs\pages\algorithms\ant-colony-system-dorigo-gambardella-1997.md",
    "docs\pages\algorithms\max-min-ant-system-stutzle-hoos-2000.md",
    "docs\ant-colony-optimization-catalog.json",
    "docs\pages\algorithms\ant-system-dorigo-maniezzo-colorni-1996.md",
    "docs\memetic-algorithm-catalog.json",
    "docs\Build-MemeticAlgorithmDocumentation.ps1",
    "docs\pages\components\memetic-algorithm-components.md",
    "docs\pages\algorithms\memetic-algorithm-moscato-1989.md",
    "docs\advanced-genetic-algorithm-catalog.json",
    "docs\Build-AdvancedGeneticAlgorithmDocumentation.ps1",
    "docs\pages\components\advanced-genetic-algorithm-operators.md",
    "docs\pages\algorithms\genetic-algorithm-generational.md",
    "docs\advanced-scatter-search-catalog.json",
    "docs\Build-AdvancedScatterSearchDocumentation.ps1",
    "docs\pages\components\advanced-scatter-search-strategies.md",
    "docs\pages\algorithms\scatter-search-marti-laguna-glover-2006.md",
    "docs\advanced-iterated-greedy-catalog.json",
    "docs\Build-AdvancedIteratedGreedyDocumentation.ps1",
    "docs\pages\components\advanced-iterated-greedy-strategies.md",
    "docs\pages\algorithms\iterated-greedy-ruiz-stutzle-2007.md",
    "docs\pages\algorithms\demon-based-acceptance-talbi-2009.md",
    "docs\pages\algorithms\tabu-search-glover.md",
    "docs\Test-TabuSearchFoundation.ps1",
    "docs\Test-TabuSearchAdvancedMemory.ps1",
    "docs\Build-TabuSearchAdvancedDocumentation.ps1",
    "docs\ts-memory-control-catalog.json",
    "docs\pages\components\tabu-search-memory-control-strategies.md",
    "docs\pages\algorithms\reactive-tabu-search-battiti-tecchiolli-1994.md",
    "docs\Test-LocalSearchFoundation.ps1",
    "docs\local-search-foundation-catalog.json",
    "docs\Test-RestartIteratedLocalSearch.ps1",
    "docs\restart-iterated-local-search-catalog.json",
    "docs\Test-VariableNeighborhoodSearch.ps1",
    "docs\variable-neighborhood-search-catalog.json",
    "docs\Test-GuidedLocalSearch.ps1",
    "docs\guided-local-search-catalog.json",
    "docs\Test-AdvancedVariableNeighborhoodSearch.ps1",
    "docs\advanced-variable-neighborhood-search-catalog.json",
    "docs\pages\components\advanced-variable-neighborhood-search-variants.md",
    "docs\pso-topology-catalog.json",
    "docs\pages\components\pso-communication-topologies.md",
    "docs\Build-PsoTopologyDocumentation.ps1",
    "docs\Build-AdvancedVariableNeighborhoodDocumentation.ps1",
    "docs\Test-ScientificComponentDocumentationParity.ps1",
    "docs\Test-ScientificFormulaQuality.ps1",
    "docs\Test-RenderedPortalQuality.ps1",
    "docs\Test-DoxygenDiagnosticQuality.ps1",
    "docs\Doxygen-CSharpCompatibilityFilter.ps1",
    "docs\Test-Grasp.ps1",
    "docs\Test-ReactiveGrasp.ps1",
    "docs\Test-GraspPathRelinking.ps1",
    "docs\path-relinking-strategy-catalog.json",
    "docs\Build-PathRelinkingStrategyDocumentation.ps1",
    "docs\pages\components\path-relinking-strategies.md",
    "docs\pages\algorithms\grasp-path-relinking.md",
    "docs\grasp-catalog.json",
    "docs\pages\algorithms\grasp-feo-resende-1995.md",
    "docs\pages\families\constructive-methods.md",
    "docs\doxygen-custom.css",
    "docs\assets\algorithms-icon.svg",
    "docs\assets\metaheuristicsplatform-logo.svg",
    "docs\assets\metaheuristicsplatform-favicon.svg",
    ".github\workflows\build.yml",
    ".github\workflows\documentation.yml",
    ".github\workflows\release.yml",
    "build\Build-Validated.ps1",
    "build\Build-All.ps1",
    "build\Prepare-ReleaseAssets.ps1",
    "build\Get-ReleaseNotes.ps1",
    "tools\Test-PowerShellSyntax.ps1",
    "tools\Test-Automation.ps1",
    "tools\Configure-GitHubRepository.ps1",
    "tools\Get-BuildTarget.ps1"
)

foreach ($relative in $requiredRepoFiles) {
    if (-not (Test-Path (Join-Path $Root $relative))) {
        throw "Documentation parity: required ULSAlgorithms-parity file missing: '$relative'."
    }
}

$readme =
    [System.IO.File]::ReadAllText((Join-Path $Root "README.md"), [System.Text.Encoding]::UTF8)

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
    [System.IO.File]::ReadAllText($idsSourcePath, [System.Text.Encoding]::UTF8)

foreach ($algorithm in $algorithms) {
    if (-not $idsSource.Contains('"' + $algorithm.id + '"')) {
        throw "Documentation parity: stable ID '$($algorithm.id)' is not declared in MetaheuristicAlgorithmIds."
    }
}

$version =
    [System.IO.File]::ReadAllText((Join-Path $Root "version.json"), [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

if ([string]$version.version -ne "0.49.0") {
    throw "Documentation parity: version.json must be 0.49.0 for this release."
}

& (Join-Path $Root "docs\Test-TextEncoding.ps1") -Root $Root
& (Join-Path $Root "docs\Test-SimulatedAnnealingCoolingCatalog.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ThresholdAccepting.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DueckAcceptanceMethods.ps1") -Root $Root
& (Join-Path $Root "docs\Test-LateAcceptance.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DemonBasedAcceptance.ps1") -Root $Root
& (Join-Path $Root "docs\Test-IteratedGreedy.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedIteratedGreedy.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ScatterSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedScatterSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GeneticAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedGeneticAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-MemeticAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AntSystem.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedAntColony.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ArtificialBeeColony.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ReadmeQuality.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ReadmeHistoricalCompatibility.ps1") -Root $Root
& (Join-Path $Root "docs\Test-CmaEs.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedCmaEs.ps1") -Root $Root
& (Join-Path $Root "docs\Test-RestartCmaEs.ps1") -Root $Root
& (Join-Path $Root "docs\Test-TabuSearchFoundation.ps1") -Root $Root
& (Join-Path $Root "docs\Test-TabuSearchAdvancedMemory.ps1") -Root $Root
& (Join-Path $Root "docs\Test-LocalSearchFoundation.ps1") -Root $Root
& (Join-Path $Root "docs\Test-RestartIteratedLocalSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-VariableNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GuidedLocalSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedVariableNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-Grasp.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ReactiveGrasp.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GraspPathRelinking.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ScientificComponentDocumentationParity.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ScientificFormulaQuality.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DoxygenDiagnosticQuality.ps1") -Root $Root

Write-Host "Documentation parity validation passed: $($algorithms.Count) algorithms, $($families.Count) family pages." -ForegroundColor Green
