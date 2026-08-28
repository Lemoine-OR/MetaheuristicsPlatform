[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\homaifar-penalty-ga-1994.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for homaifar-penalty-ga-1994." }
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
        "homaifar-penalty-ga-1994",
        "10.1177/003754979406200405"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for homaifar-penalty-ga-1994: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "homaifar-penalty-ga-1994" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for homaifar-penalty-ga-1994." }
if ([string]$entries[0].doi -ne "10.1177/003754979406200405") { throw "Scientific catalog DOI mismatch for homaifar-penalty-ga-1994." }
Write-Host "Scientific structured contract GREEN: homaifar-penalty-ga-1994" -ForegroundColor Green
