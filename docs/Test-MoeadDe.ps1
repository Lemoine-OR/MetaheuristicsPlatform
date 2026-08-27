[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath =
    Join-Path $Root "docs\algorithm-catalog.json"

$catalog =
    [System.IO.File]::ReadAllText(
        $catalogPath,
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$entry =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq
                "moead-de-li-zhang-2009"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one moead-de-li-zhang-2009 entry."
}

if ([string]$entry[0].doi -ne "10.1109/TEVC.2008.925798") {
    throw "Scientific contract: DOI mismatch for moead-de-li-zhang-2009."
}

if (-not ([string]$entry[0].problem).Contains("ParetoMin") -or
    -not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: native Pareto/aligned mathematics missing for moead-de-li-zhang-2009."
}

$pagePath =
    Join-Path $Root "docs\\pages\\algorithms\\moead-de-li-zhang-2009.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for moead-de-li-zhang-2009."
}

$pageText =
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
    "10.1109/TEVC.2008.925798"
)) {
    if (-not $pageText.Contains($marker)) {
        throw "Scientific contract: page marker missing for moead-de-li-zhang-2009: $marker"
    }
}

Write-Host "Scientific structured contract GREEN: moead-de-li-zhang-2009"
