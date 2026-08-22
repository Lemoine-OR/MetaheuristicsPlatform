[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative

    if (-not (Test-Path -LiteralPath $path)) {
        throw "Scientific component documentation parity: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains(
    [string]$Relative,
    [string[]]$Markers) {

    $text = Read-Utf8 $Relative

    foreach ($marker in $Markers) {
        if (-not $text.Contains($marker)) {
            throw "Scientific component documentation parity: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

$versionText = [version]([string]$version.version)

if ($versionText -lt [version]"0.27.1") {
    throw "Scientific component documentation parity: expected repository version 0.27.1 or later."
}

$catalog =
    (Read-Utf8 "docs\pso-topology-catalog.json") |
    ConvertFrom-Json

$entries = @($catalog.entries)

if ($entries.Count -ne 10) {
    throw "PSO topology documentation: expected exactly ten implemented topology entries."
}

$ids =
    @($entries | ForEach-Object { [string]$_.id })

$expectedIds = @(
    "fully-connected",
    "ring",
    "hub-and-spoke",
    "toroidal-von-neumann",
    "random-connected",
    "clustered-general",
    "small-world-watts-strogatz",
    "scale-free-barabasi-albert",
    "dcluster-exact",
    "custom-graph"
)

foreach ($id in $expectedIds) {
    if ($ids -notcontains $id) {
        throw "PSO topology documentation: catalog is missing '$id'."
    }
}

$defaultEntries =
    @($entries | Where-Object { [bool]$_.inDefaultCatalog })

if ($defaultEntries.Count -ne 8) {
    throw "PSO topology documentation: expected eight PsoTopologyCatalog.CreateDefaults entries."
}

$parameterDefaults =
    @($entries | Where-Object { [bool]$_.isPsoParameterDefault })

if ($parameterDefaults.Count -ne 1 -or
    [string]$parameterDefaults[0].id -ne "fully-connected") {
    throw "PSO topology documentation: Fully Connected must be the unique PsoParameters default."
}

$dcluster =
    @($entries | Where-Object id -eq "dcluster-exact")

if ($dcluster.Count -ne 1) {
    throw "PSO topology documentation: DCluster entry is missing."
}

if ([string]$dcluster[0].dynamics -ne "FitnessDynamic" -or
    [string]$dcluster[0].requiredData -ne "CurrentFitness") {
    throw "PSO topology documentation: DCluster dynamic metadata is incorrect."
}

if (-not ([string]$dcluster[0].parameters).Contains("N=p(p+1)")) {
    throw "PSO topology documentation: DCluster N=p(p+1) condition is missing."
}

$topologySources = @{
    "fully-connected" = "FullyConnectedTopology.cs"
    "ring" = "RingTopology.cs"
    "hub-and-spoke" = "HubAndSpokeTopology.cs"
    "toroidal-von-neumann" = "ToroidalVonNeumannTopology.cs"
    "random-connected" = "RandomConnectedTopology.cs"
    "clustered-general" = "ClusteredTopology.cs"
    "small-world-watts-strogatz" = "WattsStrogatzSmallWorldTopology.cs"
    "scale-free-barabasi-albert" = "BarabasiAlbertScaleFreeTopology.cs"
    "dcluster-exact" = "DClusterTopology.cs"
    "custom-graph" = "CustomGraphTopology.cs"
}

foreach ($id in $topologySources.Keys) {
    $relative =
        "src\MetaheuristicsPlatform\Algorithms\PSO\Topologies\" +
        $topologySources[$id]

    Require-Contains $relative @(
        '"' + $id + '"'
    )
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\PSO\Topologies\DClusterTopology.cs" @(
        "FitnessDynamic",
        "CurrentFitness",
        "ClusterSize * (ClusterSize + 1)",
        "CompareWorstFirst",
        "10.1007/s11047-014-9465-2"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\PSO\ParticleSwarmOptimizer.cs" @(
        "implicitFullyConnectedCanonical",
        "EnsureGraph",
        "PsoTopologyDynamics.FitnessDynamic",
        "graphHolder.Graph = null"
    )

Require-Contains `
    "docs\pages\components\pso-communication-topologies.md" @(
        "@page pso_communication_topologies",
        "fully-connected",
        "ring",
        "hub-and-spoke",
        "toroidal-von-neumann",
        "random-connected",
        "clustered-general",
        "small-world-watts-strogatz",
        "scale-free-barabasi-albert",
        "dcluster-exact",
        "custom-graph",
        "N=p(p+1)",
        "10.1007/s11047-014-9465-2"
    )

Require-Contains `
    "docs\Build-PsoTopologyDocumentation.ps1" @(
        "PSO Communication Topology Catalog",
        "pso-topology-catalog.json",
        "Scientific components",
        "particle-swarm.html"
    )

Require-Contains `
    "docs\Build-AdvancedVariableNeighborhoodDocumentation.ps1" @(
        "Advanced Variable Neighborhood Search Variants",
        "advanced-variable-neighborhood-search-variants.html",
        "Scientific components"
    )

Require-Contains `
    "docs\Build-ThresholdAcceptingScheduleDocumentation.ps1" @(
        "Threshold Accepting Schedules",
        "threshold-accepting-schedules.html",
        "threshold-accepting-schedule-catalog.json",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\Build-AcceptanceBasedTrajectoryDocumentation.ps1" @(
        "Acceptance-Based Trajectory Methods",
        "acceptance-based-trajectory-methods.html",
        "acceptance-based-trajectory-catalog.json",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\Build-PathRelinkingStrategyDocumentation.ps1" @(
        "Advanced Path Relinking Strategies",
        "path-relinking-strategies.html",
        "path-relinking-strategy-catalog.json",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\build-documentation.ps1" @(
        "Build-PsoTopologyDocumentation.ps1",
        "Build-AdvancedVariableNeighborhoodDocumentation.ps1",
        "Build-ThresholdAcceptingScheduleDocumentation.ps1",
        "Build-AcceptanceBasedTrajectoryDocumentation.ps1",
        "Build-PathRelinkingStrategyDocumentation.ps1",
        "Build-AdvancedIteratedGreedyDocumentation.ps1",
        "Build-AdvancedScatterSearchDocumentation.ps1",
        "Build-AdvancedGeneticAlgorithmDocumentation.ps1",
        "Build-MemeticAlgorithmDocumentation.ps1",
        "Build-AdvancedAntColonyDocumentation.ps1"
    )

Require-Contains `
    "docs\mainpage.md" @(
        "@subpage pso_communication_topologies",
        "@subpage advanced_variable_neighborhood_search_variants",
        "@subpage threshold_accepting_schedules",
        "@subpage acceptance_based_trajectory_methods",
        "@subpage path_relinking_strategies",
        "@subpage advanced_iterated_greedy_strategies",
        "@subpage advanced_scatter_search_strategies",
        "@subpage advanced_genetic_algorithm_operators",
        "@subpage memetic_algorithm_components",
        "@subpage advanced_ant_colony_optimization"
    )

Require-Contains `
    "docs\pages\algorithms\particle-swarm.md" @(
        "## Implemented communication topologies",
        "@subpage pso_communication_topologies"
    )

Require-Contains `
    "docs\pages\components\advanced-variable-neighborhood-search-variants.md" @(
        "@page advanced_variable_neighborhood_search_variants"
    )

Require-Contains `
    "docs\Build-AdvancedIteratedGreedyDocumentation.ps1" @(
        "Advanced Iterated Greedy Strategies",
        "advanced-iterated-greedy-strategies.html",
        "advanced-iterated-greedy-catalog.json",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )
Require-Contains `
    "docs\Build-AdvancedScatterSearchDocumentation.ps1" @(
        "Advanced Scatter Search Strategies",
        "advanced-scatter-search-strategies.html",
        "advanced-scatter-search-catalog.json",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\pages\components\advanced-scatter-search-strategies.md" @(
        "@page advanced_scatter_search_strategies",
        "ss.refset.update.dynamic-refresh",
        "ss.refset.update.two-tier",
        "ss.refset.rebuild.max-min",
        "ss.subsets.glover-types-1-4"
    )

Require-Contains `
    "docs\Build-AdvancedGeneticAlgorithmDocumentation.ps1" @(
        "Advanced Genetic Algorithm Operators",
        "advanced-genetic-algorithm-operators.html",
        "advanced-genetic-algorithm-catalog.json",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\pages\components\advanced-genetic-algorithm-operators.md" @(
        "@page advanced_genetic_algorithm_operators",
        "ga.selection.linear-ranking",
        "ga.crossover.pmx",
        "ga.mutation.polynomial-bounded",
        "ga.replacement.steady-state"
    )
Require-Contains `
    "docs\Build-MemeticAlgorithmDocumentation.ps1" @(
        "Memetic Algorithm Components",
        "memetic-algorithm-components.html",
        "memetic-algorithm-catalog.json",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\pages\components\memetic-algorithm-components.md" @(
        "@page memetic_algorithm_components",
        "ma.local-search.adaptive-stagnation",
        "ma.learning.lamarckian",
        "ma.learning.baldwinian",
        "10.1109/TEVC.2005.850260"
    )
Require-Contains `
    "docs\Build-AdvancedAntColonyDocumentation.ps1" @(
        "Advanced Ant Colony Optimization",
        "advanced-ant-colony-optimization.html",
        "advanced-ant-colony-optimization-catalog.json",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js"
    )

Require-Contains `
    "docs\pages\components\advanced-ant-colony-optimization.md" @(
        "@page advanced_ant_colony_optimization",
        "aco.transition.acs-pseudo-random-proportional",
        "aco.update.acs-local",
        "aco.memory.mmas-bounds",
        "10.1109/4235.585892",
        "10.1016/S0167-739X(00)00043-1"
    )
$readme = Read-Utf8 "README.md"

foreach ($marker in @(
    "## Scientific components",
    "PSO Communication Topology Catalog",
    "components/pso-communication-topologies.html",
    "components/simulated-annealing-cooling-schedules.html",
    "components/threshold-accepting-schedules.html",
    "components/acceptance-based-trajectory-methods.html",
    "components/tabu-search-memory-control-strategies.html",
    "components/advanced-variable-neighborhood-search-variants.html",
    "components/path-relinking-strategies.html",
    "components/advanced-iterated-greedy-strategies.html",
    "components/advanced-scatter-search-strategies.html",
    "components/advanced-genetic-algorithm-operators.html",
    "components/memetic-algorithm-components.html",
    "components/advanced-ant-colony-optimization.html"
)) {
    if (-not $readme.Contains($marker)) {
        throw "README documentation parity: missing '$marker'."
    }
}

if ($readme.Contains("</table>`n###") -or
    $readme.Contains("</table>`r`n###")) {
    throw "README rendering parity: a Markdown family heading immediately follows an HTML table without a blank line."
}

foreach ($heading in @(
    "### Swarm intelligence",
    "### Evolutionary methods",
    "### Trajectory-based methods",
    "### Hybrid / memetic methods"
)) {
    if (-not $readme.Contains($heading)) {
        throw "README rendering parity: missing family heading '$heading'."
    }
}

Write-Host `
    "Scientific component documentation parity passed: PSO/VNS/Threshold-Accepting/Dueck-Acceptance/Path-Relinking/Advanced-IG/Advanced-SS/Advanced-GA/Memetic/Advanced-ACO component builders wired." `
    -ForegroundColor Green
