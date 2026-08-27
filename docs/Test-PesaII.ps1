[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "pesa-ii-corne-jerram-knowles-oates-2001" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one pesa-ii-corne-jerram-knowles-oates-2001 catalog entry."
}

if ([string]$entry[0].doi -ne "10.5555/2955239.2955289") {
    throw "Scientific contract: DOI mismatch for pesa-ii-corne-jerram-knowles-oates-2001."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for pesa-ii-corne-jerram-knowles-oates-2001."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\pesa-ii-corne-jerram-knowles-oates-2001.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for pesa-ii-corne-jerram-knowles-oates-2001."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.5555/2955239.2955289")) {
    throw "Scientific contract: page contract incomplete for pesa-ii-corne-jerram-knowles-oates-2001."
}

Write-Host "Scientific structured contract GREEN: pesa-ii-corne-jerram-knowles-oates-2001"
