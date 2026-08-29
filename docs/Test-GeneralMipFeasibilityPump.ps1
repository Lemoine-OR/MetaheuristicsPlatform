[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\general-mip-feasibility-pump-bertacco-fischetti-lodi-2007.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Matheuristic page missing for general-mip-feasibility-pump-bertacco-fischetti-lodi-2007."
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
        "general-mip-feasibility-pump-bertacco-fischetti-lodi-2007",
        "10.1016/j.disopt.2006.10.001",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation",
        "IExactRepairMatheuristicDomain",
        "ExactRepairRequest",
        "MatheuristicOptimizationResult"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Matheuristic page marker missing for general-mip-feasibility-pump-bertacco-fischetti-lodi-2007: {0}" -f
            $marker)
    }
}

Write-Host "Matheuristic scientific contract GREEN: general-mip-feasibility-pump-bertacco-fischetti-lodi-2007" -ForegroundColor Green
