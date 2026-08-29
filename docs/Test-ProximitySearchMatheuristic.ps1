[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\proximity-search-fischetti-monaci-2014.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Matheuristic page missing for proximity-search-fischetti-monaci-2014."
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
        "proximity-search-fischetti-monaci-2014",
        "10.1007/s10732-014-9266-x",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation",
        "IExactRepairMatheuristicDomain",
        "ExactRepairRequest",
        "MatheuristicOptimizationResult"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Matheuristic page marker missing for proximity-search-fischetti-monaci-2014: {0}" -f
            $marker)
    }
}

Write-Host "Matheuristic scientific contract GREEN: proximity-search-fischetti-monaci-2014" -ForegroundColor Green
