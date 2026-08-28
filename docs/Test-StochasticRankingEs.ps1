[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\stochastic-ranking-es-runarsson-yao-2000.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for stochastic-ranking-es-runarsson-yao-2000." }
$page = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
foreach ($marker in @(
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
        "stochastic-ranking-es-runarsson-yao-2000",
        "10.1109/4235.873238"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for stochastic-ranking-es-runarsson-yao-2000: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "stochastic-ranking-es-runarsson-yao-2000" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for stochastic-ranking-es-runarsson-yao-2000." }
if ([string]$entries[0].doi -ne "10.1109/4235.873238") { throw "Scientific catalog DOI mismatch for stochastic-ranking-es-runarsson-yao-2000." }
Write-Host "Scientific structured contract GREEN: stochastic-ranking-es-runarsson-yao-2000" -ForegroundColor Green
