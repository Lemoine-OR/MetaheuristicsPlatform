[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\whale-optimization-algorithm-mirjalili-lewis-2016.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\WhaleOptimization\WhaleOptimizationAlgorithmOptimizer.cs"

foreach ($requiredPath in @($catalogPath, $pagePath, $sourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog = [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "whale-optimization-algorithm-mirjalili-lewis-2016" })

if ($entry.Count -ne 1) { throw "Scientific contract expected exactly one structured catalog entry for whale-optimization-algorithm-mirjalili-lewis-2016." }
if ([string]$entry[0].doi -ne "10.1016/j.advengsoft.2016.01.008") { throw "Scientific contract DOI mismatch for whale-optimization-algorithm-mirjalili-lewis-2016." }
if ([string]$entry[0].class -ne "WhaleOptimizationAlgorithmOptimizer") { throw "Scientific contract runtime class mismatch for whale-optimization-algorithm-mirjalili-lewis-2016." }
if ([string]$entry[0].factoryMode -ne "direct") { throw "Scientific contract requires direct factory mode for whale-optimization-algorithm-mirjalili-lewis-2016." }

$page = [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)
if (-not $page.Contains("10.1016/j.advengsoft.2016.01.008") -or -not $page.Contains("whale-optimization-algorithm-mirjalili-lewis-2016") -or -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source = [System.IO.File]::ReadAllText($sourcePath, [System.Text.Encoding]::UTF8)
if (-not $source.Contains("MetaheuristicAlgorithmIds.WhaleOptimizationAlgorithm") -or -not $source.Contains("WhaleOptimizationAlgorithmReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: whale-optimization-algorithm-mirjalili-lewis-2016" -ForegroundColor Green
