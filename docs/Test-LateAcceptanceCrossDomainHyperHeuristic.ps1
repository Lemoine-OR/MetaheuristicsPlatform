[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\late-acceptance-selection-hh-jackson-ozcan-drake-2013.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for late-acceptance-selection-hh-jackson-ozcan-drake-2013."
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
        "late-acceptance-selection-hh-jackson-ozcan-drake-2013",
        "10.1109/UKCI.2013.6651310",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for late-acceptance-selection-hh-jackson-ozcan-drake-2013: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: late-acceptance-selection-hh-jackson-ozcan-drake-2013" -ForegroundColor Green
