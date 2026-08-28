[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$pagePath =
    Join-Path $Root `
        "docs\pages\algorithms\crowding-de-thomsen-2004.md"

if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific page missing for crowding-de-thomsen-2004."
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
    "crowding-de-thomsen-2004",
    "10.1109/CEC.2004.1331058"
)) {
    if (-not $page.Contains($marker)) {
        throw (
            "Scientific page marker missing for crowding-de-thomsen-2004: {0}" -f
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
            [string]$_.id -eq "crowding-de-thomsen-2004"
        }
    )

if ($entries.Count -ne 1) {
    throw "Scientific catalog identity count mismatch for crowding-de-thomsen-2004."
}

if ([string]$entries[0].doi -ne "10.1109/CEC.2004.1331058") {
    throw "Scientific catalog DOI mismatch for crowding-de-thomsen-2004."
}

Write-Host "Scientific multimodal contract GREEN: crowding-de-thomsen-2004" -ForegroundColor Green
