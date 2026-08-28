[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\adaptive-niching-pso-bird-li-2006.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific page missing for adaptive-niching-pso-bird-li-2006."
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
    "adaptive-niching-pso-bird-li-2006",
    "10.1145/1143997.1143999"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Scientific page marker missing for adaptive-niching-pso-bird-li-2006: {0}" -f
            $marker)
    }
}

$catalog =
    [System.IO.File]::ReadAllText(
        (Join-Path $Root "docs\algorithm-catalog.json"),
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$entries =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq "adaptive-niching-pso-bird-li-2006"
        }
    )

if ($entries.Count -ne 1) {
    throw "Scientific catalog identity count mismatch for adaptive-niching-pso-bird-li-2006."
}

if ([string]$entries[0].doi -ne "10.1145/1143997.1143999") {
    throw "Scientific catalog DOI mismatch for adaptive-niching-pso-bird-li-2006."
}

Write-Host "Scientific multimodal contract GREEN: adaptive-niching-pso-bird-li-2006" -ForegroundColor Green
