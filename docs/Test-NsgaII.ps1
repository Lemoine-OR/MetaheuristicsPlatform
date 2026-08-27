[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "nsga-ii-deb-pratap-agarwal-meyarivan-2002" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one nsga-ii-deb-pratap-agarwal-meyarivan-2002 catalog entry."
}

if ([string]$entry[0].doi -ne "10.1109/4235.996017") {
    throw "Scientific contract: DOI mismatch for nsga-ii-deb-pratap-agarwal-meyarivan-2002."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for nsga-ii-deb-pratap-agarwal-meyarivan-2002."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\nsga-ii-deb-pratap-agarwal-meyarivan-2002.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for nsga-ii-deb-pratap-agarwal-meyarivan-2002."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.1109/4235.996017")) {
    throw "Scientific contract: page contract incomplete for nsga-ii-deb-pratap-agarwal-meyarivan-2002."
}

Write-Host "Scientific structured contract GREEN: nsga-ii-deb-pratap-agarwal-meyarivan-2002"
