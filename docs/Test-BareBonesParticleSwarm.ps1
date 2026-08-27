[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\bare-bones-particle-swarm-kennedy-2003.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\PSO\BareBones\BareBonesParticleSwarmOptimizer.cs"

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
            [string]$_.id -eq "bare-bones-particle-swarm-kennedy-2003"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract expected exactly one catalog entry for bare-bones-particle-swarm-kennedy-2003."
}

if ([string]$entry[0].doi -ne "10.1109/SIS.2003.1202251") {
    throw "Scientific contract DOI mismatch for bare-bones-particle-swarm-kennedy-2003."
}

if ([string]$entry[0].class -ne "BareBonesParticleSwarmOptimizer") {
    throw "Scientific contract runtime-class mismatch for bare-bones-particle-swarm-kennedy-2003."
}

if ([string]$entry[0].factoryMode -ne "direct") {
    throw "Scientific contract requires direct factory mode for bare-bones-particle-swarm-kennedy-2003."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

foreach ($requiredText in @(
    "10.1109/SIS.2003.1202251",
    "bare-bones-particle-swarm-kennedy-2003",
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

if (-not $source.Contains("MetaheuristicAlgorithmIds.BareBonesParticleSwarm") -or
    -not $source.Contains("BareBonesPsoReferences")) {
    throw "Scientific source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: bare-bones-particle-swarm-kennedy-2003" -ForegroundColor Green
