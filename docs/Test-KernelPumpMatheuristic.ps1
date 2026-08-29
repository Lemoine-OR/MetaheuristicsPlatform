[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\kernel-pump-assuncao-urrutia-santos-2026.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Matheuristic page missing for kernel-pump-assuncao-urrutia-santos-2026."
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
        "kernel-pump-assuncao-urrutia-santos-2026",
        "10.1007/s12532-026-00333-2",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation",
        "IExactRepairMatheuristicDomain",
        "ExactRepairRequest",
        "MatheuristicOptimizationResult"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Matheuristic page marker missing for kernel-pump-assuncao-urrutia-santos-2026: {0}" -f
            $marker)
    }
}

Write-Host "Matheuristic scientific contract GREEN: kernel-pump-assuncao-urrutia-santos-2026" -ForegroundColor Green
