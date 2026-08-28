[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\biobjective-multimodal-ea-deb-saha-2012.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific page missing for biobjective-multimodal-ea-deb-saha-2012."
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
    "biobjective-multimodal-ea-deb-saha-2012",
    "10.1162/EVCO_a_00042"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Scientific page marker missing for biobjective-multimodal-ea-deb-saha-2012: {0}" -f
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
            [string]$_.id -eq "biobjective-multimodal-ea-deb-saha-2012"
        }
    )

if ($entries.Count -ne 1) {
    throw "Scientific catalog identity count mismatch for biobjective-multimodal-ea-deb-saha-2012."
}

if ([string]$entries[0].doi -ne "10.1162/EVCO_a_00042") {
    throw "Scientific catalog DOI mismatch for biobjective-multimodal-ea-deb-saha-2012."
}

Write-Host "Scientific multimodal contract GREEN: biobjective-multimodal-ea-deb-saha-2012" -ForegroundColor Green
