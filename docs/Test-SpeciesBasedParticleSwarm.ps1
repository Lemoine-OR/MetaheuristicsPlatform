[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\species-based-particle-swarm-parrott-li-2006.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\PSO\Speciation\SpeciesBasedParticleSwarmOptimizer.cs"

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
            [string]$_.id -eq "species-based-particle-swarm-parrott-li-2006"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract expected exactly one catalog entry for species-based-particle-swarm-parrott-li-2006."
}

if ([string]$entry[0].doi -ne "10.1109/TEVC.2005.859468") {
    throw "Scientific contract DOI mismatch for species-based-particle-swarm-parrott-li-2006."
}

if ([string]$entry[0].class -ne "SpeciesBasedParticleSwarmOptimizer") {
    throw "Scientific contract runtime-class mismatch for species-based-particle-swarm-parrott-li-2006."
}

if ([string]$entry[0].factoryMode -ne "direct") {
    throw "Scientific contract requires direct factory mode for species-based-particle-swarm-parrott-li-2006."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

foreach ($requiredText in @(
    "10.1109/TEVC.2005.859468",
    "species-based-particle-swarm-parrott-li-2006",
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

if (-not $source.Contains("MetaheuristicAlgorithmIds.SpeciesBasedParticleSwarm") -or
    -not $source.Contains("SpeciesBasedPsoReferences")) {
    throw "Scientific source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: species-based-particle-swarm-parrott-li-2006" -ForegroundColor Green
