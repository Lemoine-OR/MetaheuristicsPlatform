[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\kernel-search-angelelli-mansini-speranza-2010.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Matheuristic page missing for kernel-search-angelelli-mansini-speranza-2010."
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
        "kernel-search-angelelli-mansini-speranza-2010",
        "10.1016/j.cor.2010.02.002",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation",
        "IExactRepairMatheuristicDomain",
        "ExactRepairRequest",
        "MatheuristicOptimizationResult"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Matheuristic page marker missing for kernel-search-angelelli-mansini-speranza-2010: {0}" -f
            $marker)
    }
}

Write-Host "Matheuristic scientific contract GREEN: kernel-search-angelelli-mansini-speranza-2010" -ForegroundColor Green
