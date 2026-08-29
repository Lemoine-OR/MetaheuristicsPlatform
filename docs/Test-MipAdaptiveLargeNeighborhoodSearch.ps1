[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\mip-alns-muller-spoorendonk-pisinger-2012.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Matheuristic page missing for mip-alns-muller-spoorendonk-pisinger-2012."
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
        "mip-alns-muller-spoorendonk-pisinger-2012",
        "10.1016/j.ejor.2011.11.036",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation",
        "IExactRepairMatheuristicDomain",
        "ExactRepairRequest",
        "MatheuristicOptimizationResult"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Matheuristic page marker missing for mip-alns-muller-spoorendonk-pisinger-2012: {0}" -f
            $marker)
    }
}

Write-Host "Matheuristic scientific contract GREEN: mip-alns-muller-spoorendonk-pisinger-2012" -ForegroundColor Green
