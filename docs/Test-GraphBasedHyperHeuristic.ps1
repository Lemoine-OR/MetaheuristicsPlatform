[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007."
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
        "graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007",
        "10.1016/j.ejor.2005.08.012",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007" -ForegroundColor Green
