[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009."
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
        "extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009",
        "10.1007/978-3-642-11169-3_13",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009" -ForegroundColor Green
