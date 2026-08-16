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

if ([string]$version.version -ne "0.27.1") {
    throw "Scientific component documentation parity: version.json must be 0.27.1."
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
    "docs\build-documentation.ps1" @(
        "Build-PsoTopologyDocumentation.ps1",
        "Build-AdvancedVariableNeighborhoodDocumentation.ps1"
    )

Require-Contains `
    "docs\mainpage.md" @(
        "@subpage pso_communication_topologies",
        "@subpage advanced_variable_neighborhood_search_variants"
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

$readme = Read-Utf8 "README.md"

foreach ($marker in @(
    "## Scientific components",
    "PSO Communication Topology Catalog",
    "components/pso-communication-topologies.html",
    "components/simulated-annealing-cooling-schedules.html",
    "components/tabu-search-memory-control-strategies.html",
    "components/advanced-variable-neighborhood-search-variants.html"
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
    "Scientific component documentation parity passed: 10 PSO topologies, 4 README family headings, PSO/VNS component builders wired." `
    -ForegroundColor Green
