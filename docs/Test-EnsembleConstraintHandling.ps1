[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\ensemble-constraint-handling-mallipeddi-suganthan-2010.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for ensemble-constraint-handling-mallipeddi-suganthan-2010." }
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
        "ensemble-constraint-handling-mallipeddi-suganthan-2010",
        "10.1109/TEVC.2009.2033582"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for ensemble-constraint-handling-mallipeddi-suganthan-2010: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "ensemble-constraint-handling-mallipeddi-suganthan-2010" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for ensemble-constraint-handling-mallipeddi-suganthan-2010." }
if ([string]$entries[0].doi -ne "10.1109/TEVC.2009.2033582") { throw "Scientific catalog DOI mismatch for ensemble-constraint-handling-mallipeddi-suganthan-2010." }
Write-Host "Scientific structured contract GREEN: ensemble-constraint-handling-mallipeddi-suganthan-2010" -ForegroundColor Green
