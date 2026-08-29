[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\cmsa-blum-pinacho-lopez-ibanez-lozano-2016.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Matheuristic page missing for cmsa-blum-pinacho-lopez-ibanez-lozano-2016."
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
        "cmsa-blum-pinacho-lopez-ibanez-lozano-2016",
        "10.1016/j.cor.2015.10.014",
        "## Reproduction mode",
        "mechanism-preserving-platform-adaptation",
        "IExactRepairMatheuristicDomain",
        "ExactRepairRequest",
        "MatheuristicOptimizationResult"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Matheuristic page marker missing for cmsa-blum-pinacho-lopez-ibanez-lozano-2016: {0}" -f
            $marker)
    }
}

Write-Host "Matheuristic scientific contract GREEN: cmsa-blum-pinacho-lopez-ibanez-lozano-2016" -ForegroundColor Green
