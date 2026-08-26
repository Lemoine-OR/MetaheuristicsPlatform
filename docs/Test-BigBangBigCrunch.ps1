[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\big-bang-big-crunch-erol-eksin-2006.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\BigBangBigCrunch\BigBangBigCrunchOptimizer.cs"

foreach ($requiredPath in @($catalogPath, $pagePath, $sourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog = [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "big-bang-big-crunch-erol-eksin-2006" })

if ($entry.Count -ne 1) { throw "Scientific contract expected exactly one structured catalog entry for big-bang-big-crunch-erol-eksin-2006." }
if ([string]$entry[0].doi -ne "10.1016/j.advengsoft.2005.04.005") { throw "Scientific contract DOI mismatch for big-bang-big-crunch-erol-eksin-2006." }
if ([string]$entry[0].class -ne "BigBangBigCrunchOptimizer") { throw "Scientific contract runtime class mismatch for big-bang-big-crunch-erol-eksin-2006." }
if ([string]$entry[0].factoryMode -ne "direct") { throw "Scientific contract requires direct factory mode for big-bang-big-crunch-erol-eksin-2006." }

$page = [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)
if (-not $page.Contains("10.1016/j.advengsoft.2005.04.005") -or -not $page.Contains("big-bang-big-crunch-erol-eksin-2006") -or -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source = [System.IO.File]::ReadAllText($sourcePath, [System.Text.Encoding]::UTF8)
if (-not $source.Contains("MetaheuristicAlgorithmIds.BigBangBigCrunch") -or -not $source.Contains("BigBangBigCrunchReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: big-bang-big-crunch-erol-eksin-2006" -ForegroundColor Green
