[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\GravitationalSearch\GravitationalSearchOptimizer.cs"

foreach ($requiredPath in @($catalogPath, $pagePath, $sourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog = [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009" })

if ($entry.Count -ne 1) { throw "Scientific contract expected exactly one structured catalog entry for gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009." }
if ([string]$entry[0].doi -ne "10.1016/j.ins.2009.03.004") { throw "Scientific contract DOI mismatch for gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009." }
if ([string]$entry[0].class -ne "GravitationalSearchOptimizer") { throw "Scientific contract runtime class mismatch for gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009." }
if ([string]$entry[0].factoryMode -ne "direct") { throw "Scientific contract requires direct factory mode for gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009." }

$page = [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)
if (-not $page.Contains("10.1016/j.ins.2009.03.004") -or -not $page.Contains("gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009") -or -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source = [System.IO.File]::ReadAllText($sourcePath, [System.Text.Encoding]::UTF8)
if (-not $source.Contains("MetaheuristicAlgorithmIds.GravitationalSearch") -or -not $source.Contains("GravitationalSearchReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009" -ForegroundColor Green
