[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\tabu-search-hyperheuristic-burke-kendall-soubeiga-2003.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for tabu-search-hyperheuristic-burke-kendall-soubeiga-2003."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

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
        "tabu-search-hyperheuristic-burke-kendall-soubeiga-2003",
        "10.1023/B:HEUR.0000012446.94732.B6",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for tabu-search-hyperheuristic-burke-kendall-soubeiga-2003: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: tabu-search-hyperheuristic-burke-kendall-soubeiga-2003" -ForegroundColor Green
