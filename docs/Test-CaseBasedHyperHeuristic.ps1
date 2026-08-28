[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\case-based-heuristic-selection-burke-petrovic-qu-2006.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for case-based-heuristic-selection-burke-petrovic-qu-2006."
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
        "case-based-heuristic-selection-burke-petrovic-qu-2006",
        "10.1007/s10951-006-6775-y",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for case-based-heuristic-selection-burke-petrovic-qu-2006: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: case-based-heuristic-selection-burke-petrovic-qu-2006" -ForegroundColor Green
