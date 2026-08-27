[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\constriction-particle-swarm-clerc-kennedy-2002.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\PSO\Scientific\ConstrictionParticleSwarmOptimizer.cs"

foreach ($requiredPath in @($catalogPath, $pagePath, $sourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog =
    [System.IO.File]::ReadAllText(
        $catalogPath,
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$entry =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq "constriction-particle-swarm-clerc-kennedy-2002"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract expected exactly one catalog entry for constriction-particle-swarm-clerc-kennedy-2002."
}

if ([string]$entry[0].doi -ne "10.1109/4235.985692") {
    throw "Scientific contract DOI mismatch for constriction-particle-swarm-clerc-kennedy-2002."
}

if ([string]$entry[0].class -ne "ConstrictionParticleSwarmOptimizer") {
    throw "Scientific contract runtime-class mismatch for constriction-particle-swarm-clerc-kennedy-2002."
}

if ([string]$entry[0].factoryMode -ne "direct") {
    throw "Scientific contract requires direct factory mode for constriction-particle-swarm-clerc-kennedy-2002."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

foreach ($requiredText in @(
    "10.1109/4235.985692",
    "constriction-particle-swarm-clerc-kennedy-2002",
    "## API example",
    "### Update equations / iterations"
)) {
    if (-not $page.Contains($requiredText)) {
        throw "Scientific page lacks required text '$requiredText'."
    }
}

$source =
    [System.IO.File]::ReadAllText(
        $sourcePath,
        [System.Text.Encoding]::UTF8)

if (-not $source.Contains("MetaheuristicAlgorithmIds.ConstrictionParticleSwarm") -or
    -not $source.Contains("ConstrictionPsoReferences")) {
    throw "Scientific source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: constriction-particle-swarm-clerc-kennedy-2002" -ForegroundColor Green
