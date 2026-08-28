[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\epsilon-constrained-de-takahama-sakai-iwane-2006.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for epsilon-constrained-de-takahama-sakai-iwane-2006." }
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
        "epsilon-constrained-de-takahama-sakai-iwane-2006",
        "10.1109/ICSMC.2006.385209"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for epsilon-constrained-de-takahama-sakai-iwane-2006: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "epsilon-constrained-de-takahama-sakai-iwane-2006" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for epsilon-constrained-de-takahama-sakai-iwane-2006." }
if ([string]$entries[0].doi -ne "10.1109/ICSMC.2006.385209") { throw "Scientific catalog DOI mismatch for epsilon-constrained-de-takahama-sakai-iwane-2006." }
Write-Host "Scientific structured contract GREEN: epsilon-constrained-de-takahama-sakai-iwane-2006" -ForegroundColor Green
