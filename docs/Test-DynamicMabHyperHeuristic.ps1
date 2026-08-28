[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008."
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
        "dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008",
        "10.1145/1389095.1389272",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008" -ForegroundColor Green
