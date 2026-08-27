[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\standard-particle-swarm-bratton-kennedy-2007.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\PSO\Standard2007\StandardPso2007Optimizer.cs"

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
            [string]$_.id -eq "standard-particle-swarm-bratton-kennedy-2007"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract expected exactly one catalog entry for standard-particle-swarm-bratton-kennedy-2007."
}

if ([string]$entry[0].doi -ne "10.1109/SIS.2007.368035") {
    throw "Scientific contract DOI mismatch for standard-particle-swarm-bratton-kennedy-2007."
}

if ([string]$entry[0].class -ne "StandardPso2007Optimizer") {
    throw "Scientific contract runtime-class mismatch for standard-particle-swarm-bratton-kennedy-2007."
}

if ([string]$entry[0].factoryMode -ne "direct") {
    throw "Scientific contract requires direct factory mode for standard-particle-swarm-bratton-kennedy-2007."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

foreach ($requiredText in @(
    "10.1109/SIS.2007.368035",
    "standard-particle-swarm-bratton-kennedy-2007",
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

if (-not $source.Contains("MetaheuristicAlgorithmIds.StandardParticleSwarm2007") -or
    -not $source.Contains("StandardPso2007References")) {
    throw "Scientific source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: standard-particle-swarm-bratton-kennedy-2007" -ForegroundColor Green
