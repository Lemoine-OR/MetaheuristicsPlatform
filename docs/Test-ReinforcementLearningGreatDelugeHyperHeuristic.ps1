[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010."
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
        "reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010",
        "10.4018/jamc.2010102603",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010" -ForegroundColor Green
