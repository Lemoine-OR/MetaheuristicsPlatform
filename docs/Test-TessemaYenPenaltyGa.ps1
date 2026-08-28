[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\adaptive-penalty-formulation-ga-tessema-yen-2009.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for adaptive-penalty-formulation-ga-tessema-yen-2009." }
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
        "adaptive-penalty-formulation-ga-tessema-yen-2009",
        "10.1109/TSMCA.2009.2013333"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for adaptive-penalty-formulation-ga-tessema-yen-2009: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "adaptive-penalty-formulation-ga-tessema-yen-2009" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for adaptive-penalty-formulation-ga-tessema-yen-2009." }
if ([string]$entries[0].doi -ne "10.1109/TSMCA.2009.2013333") { throw "Scientific catalog DOI mismatch for adaptive-penalty-formulation-ga-tessema-yen-2009." }
Write-Host "Scientific structured contract GREEN: adaptive-penalty-formulation-ga-tessema-yen-2009" -ForegroundColor Green
