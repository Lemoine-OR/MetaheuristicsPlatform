[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$catalog = [System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "rvea-cheng-jin-olhofer-sendhoff-2016" })

if ($entry.Count -ne 1) {
    throw "Scientific contract: expected exactly one rvea-cheng-jin-olhofer-sendhoff-2016 catalog entry."
}

if ([string]$entry[0].doi -ne "10.1109/TEVC.2016.2519378") {
    throw "Scientific contract: DOI mismatch for rvea-cheng-jin-olhofer-sendhoff-2016."
}

if (-not ([string]$entry[0].update).Contains('\begin{aligned}')) {
    throw "Scientific contract: aligned update formula missing for rvea-cheng-jin-olhofer-sendhoff-2016."
}

$pagePath = Join-Path $Root "docs\\pages\\algorithms\\rvea-cheng-jin-olhofer-sendhoff-2016.md"
if (-not (Test-Path -LiteralPath $pagePath -PathType Leaf)) {
    throw "Scientific contract: page missing for rvea-cheng-jin-olhofer-sendhoff-2016."
}

$pageText = [System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if (-not $pageText.Contains("## API example") -or
    -not $pageText.Contains("10.1109/TEVC.2016.2519378")) {
    throw "Scientific contract: page contract incomplete for rvea-cheng-jin-olhofer-sendhoff-2016."
}

Write-Host "Scientific structured contract GREEN: rvea-cheng-jin-olhofer-sendhoff-2016"
