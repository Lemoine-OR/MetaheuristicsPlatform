[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "mopso-coello-pulido-lechuga-2004" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one mopso-coello-pulido-lechuga-2004 catalog entry."
}

if ([string]$entry[0].doi -ne "10.1109/TEVC.2004.826067") {
    throw "Scientific contract: DOI mismatch for mopso-coello-pulido-lechuga-2004."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for mopso-coello-pulido-lechuga-2004."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\mopso-coello-pulido-lechuga-2004.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for mopso-coello-pulido-lechuga-2004."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.1109/TEVC.2004.826067")) {
    throw "Scientific contract: page contract incomplete for mopso-coello-pulido-lechuga-2004."
}

Write-Host "Scientific structured contract GREEN: mopso-coello-pulido-lechuga-2004"
