[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "sms-emoa-beume-naujoks-emmerich-2007" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one sms-emoa-beume-naujoks-emmerich-2007 catalog entry."
}

if ([string]$entry[0].doi -ne "10.1016/j.ejor.2006.08.008") {
    throw "Scientific contract: DOI mismatch for sms-emoa-beume-naujoks-emmerich-2007."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for sms-emoa-beume-naujoks-emmerich-2007."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\sms-emoa-beume-naujoks-emmerich-2007.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for sms-emoa-beume-naujoks-emmerich-2007."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.1016/j.ejor.2006.08.008")) {
    throw "Scientific contract: page contract incomplete for sms-emoa-beume-naujoks-emmerich-2007."
}

Write-Host "Scientific structured contract GREEN: sms-emoa-beume-naujoks-emmerich-2007"
