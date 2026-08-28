[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\frrmab-li-fialho-kwong-zhang-2014.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for frrmab-li-fialho-kwong-zhang-2014."
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
        "frrmab-li-fialho-kwong-zhang-2014",
        "10.1109/TEVC.2013.2239648",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for frrmab-li-fialho-kwong-zhang-2014: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: frrmab-li-fialho-kwong-zhang-2014" -ForegroundColor Green
