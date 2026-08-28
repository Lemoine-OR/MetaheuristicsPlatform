[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$pagePath = Join-Path $Root "docs\pages\algorithms\dominance-based-tournament-ga-coello-mezura-2002.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) { throw "Scientific page is missing for dominance-based-tournament-ga-coello-mezura-2002." }
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
        "dominance-based-tournament-ga-coello-mezura-2002",
        "10.1016/S1474-0346(02)00011-3"
)) { if (-not $page.Contains($marker)) { throw ("Scientific page marker missing for dominance-based-tournament-ga-coello-mezura-2002: {0}" -f $marker) } }
$catalog=[System.IO.File]::ReadAllText((Join-Path $Root "docs\algorithm-catalog.json"),[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entries=@($catalog.algorithms|Where-Object { [string]$_.id -eq "dominance-based-tournament-ga-coello-mezura-2002" })
if ($entries.Count -ne 1) { throw "Scientific catalog identity count mismatch for dominance-based-tournament-ga-coello-mezura-2002." }
if ([string]$entries[0].doi -ne "10.1016/S1474-0346(02)00011-3") { throw "Scientific catalog DOI mismatch for dominance-based-tournament-ga-coello-mezura-2002." }
Write-Host "Scientific structured contract GREEN: dominance-based-tournament-ga-coello-mezura-2002" -ForegroundColor Green
