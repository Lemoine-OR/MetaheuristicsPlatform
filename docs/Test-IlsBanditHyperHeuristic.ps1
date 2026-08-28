[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Hyper-heuristic page missing for ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017."
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
        "ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017",
        "10.1016/j.ejor.2017.01.042",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Hyper-heuristic page marker missing for ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017: {0}" -f
            $marker)
    }
}

Write-Host "Hyper-heuristic scientific contract GREEN: ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017" -ForegroundColor Green
