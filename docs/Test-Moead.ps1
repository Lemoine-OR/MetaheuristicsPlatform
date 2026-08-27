[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "moead-zhang-li-2007" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one moead-zhang-li-2007 catalog entry."
}

if ([string]$entry[0].doi -ne "10.1109/TEVC.2007.892759") {
    throw "Scientific contract: DOI mismatch for moead-zhang-li-2007."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for moead-zhang-li-2007."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\moead-zhang-li-2007.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for moead-zhang-li-2007."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.1109/TEVC.2007.892759")) {
    throw "Scientific contract: page contract incomplete for moead-zhang-li-2007."
}

Write-Host "Scientific structured contract GREEN: moead-zhang-li-2007"
