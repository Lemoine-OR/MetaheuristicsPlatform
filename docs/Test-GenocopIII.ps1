[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\genocop-iii-michalewicz-nazhiyath-1995.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for genocop-iii-michalewicz-nazhiyath-1995." }
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
        "genocop-iii-michalewicz-nazhiyath-1995",
        "10.1109/ICEC.1995.487460"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for genocop-iii-michalewicz-nazhiyath-1995: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "genocop-iii-michalewicz-nazhiyath-1995" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for genocop-iii-michalewicz-nazhiyath-1995." }
if ([string]$entries[0].doi -ne "10.1109/ICEC.1995.487460") { throw "Scientific catalog DOI mismatch for genocop-iii-michalewicz-nazhiyath-1995." }
Write-Host "Scientific structured contract GREEN: genocop-iii-michalewicz-nazhiyath-1995" -ForegroundColor Green
