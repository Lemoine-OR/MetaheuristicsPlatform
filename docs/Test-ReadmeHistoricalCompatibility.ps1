[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$readmePath =
    Join-Path $Root "README.md"

if (-not (Test-Path -LiteralPath $readmePath)) {
    throw "README historical compatibility: README.md is missing."
}

$readme =
    [System.IO.File]::ReadAllText(
        $readmePath,
        [System.Text.Encoding]::UTF8)

$requiredMarkers = @(
    "public algorithms",
    "### Swarm intelligence",
    "### Evolutionary methods",
    "### Trajectory-based methods",
    "### Constructive methods",
    "### Hybrid / memetic methods",
    "grasp-feo-resende-1995",
    "reactive-grasp-prais-ribeiro-2000",
    "GRASP with Path Relinking",
    "7 executable strategies",
    "generational Evolutionary Path Relinking",
    "Memetic Algorithm - Moscato",
    "memetic-algorithm-moscato-1989",
    "Scatter Search",
    "scatter-search-marti-laguna-glover-2006",
    "19 trajectory methods",
    "iterated-greedy-ruiz-stutzle-2007",
    "Advanced Iterated Greedy Strategies",
    "components/advanced-iterated-greedy-strategies.html",
    "ig.*",
    "threshold-accepting-dueck-scheuer-1990",
    "components/threshold-accepting-schedules.html",
    "## Scientific components",
    "PSO Communication Topology Catalog",
    "components/pso-communication-topologies.html",
    "components/simulated-annealing-cooling-schedules.html",
    "components/acceptance-based-trajectory-methods.html",
    "components/tabu-search-memory-control-strategies.html",
    "components/advanced-variable-neighborhood-search-variants.html",
    "components/path-relinking-strategies.html",
    "components/advanced-scatter-search-strategies.html",
    "components/advanced-genetic-algorithm-operators.html",
    "components/memetic-algorithm-components.html",
    "components/advanced-ant-colony-optimization.html",
    "components/cma-es-components.html"
)

foreach ($marker in $requiredMarkers) {
    if (-not $readme.Contains($marker)) {
        throw "README historical compatibility: missing '$marker'."
    }
}

Write-Host `
    "README historical compatibility passed: legacy validator markers preserved inside the redesigned public README." `
    -ForegroundColor Green
