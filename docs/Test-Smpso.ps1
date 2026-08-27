[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009 catalog entry."
}

if ([string]$entry[0].doi -ne "10.1109/MCDM.2009.4938830") {
    throw "Scientific contract: DOI mismatch for smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.1109/MCDM.2009.4938830")) {
    throw "Scientific contract: page contract incomplete for smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009."
}

Write-Host "Scientific structured contract GREEN: smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009"
