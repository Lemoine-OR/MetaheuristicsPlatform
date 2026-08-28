[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\homomorphous-mapping-ea-koziel-michalewicz-1999.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for homomorphous-mapping-ea-koziel-michalewicz-1999." }
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
        "homomorphous-mapping-ea-koziel-michalewicz-1999",
        "10.1162/evco.1999.7.1.19"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for homomorphous-mapping-ea-koziel-michalewicz-1999: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "homomorphous-mapping-ea-koziel-michalewicz-1999" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for homomorphous-mapping-ea-koziel-michalewicz-1999." }
if ([string]$entries[0].doi -ne "10.1162/evco.1999.7.1.19") { throw "Scientific catalog DOI mismatch for homomorphous-mapping-ea-koziel-michalewicz-1999." }
Write-Host "Scientific structured contract GREEN: homomorphous-mapping-ea-koziel-michalewicz-1999" -ForegroundColor Green
