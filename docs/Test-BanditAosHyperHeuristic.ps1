[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\bandit-aos-fialho-da-costa-schoenauer-sebag-2010.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for bandit-aos-fialho-da-costa-schoenauer-sebag-2010."
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
        "bandit-aos-fialho-da-costa-schoenauer-sebag-2010",
        "10.1007/s10472-010-9213-y",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for bandit-aos-fialho-da-costa-schoenauer-sebag-2010: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: bandit-aos-fialho-da-costa-schoenauer-sebag-2010" -ForegroundColor Green
