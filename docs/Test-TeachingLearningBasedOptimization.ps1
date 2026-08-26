[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\teaching-learning-based-optimization-rao-savsani-vakharia-2011.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\TeachingLearningBasedOptimization\TeachingLearningBasedOptimizationOptimizer.cs"

foreach ($requiredPath in @($catalogPath, $pagePath, $sourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog = [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "teaching-learning-based-optimization-rao-savsani-vakharia-2011" })

if ($entry.Count -ne 1) { throw "Scientific contract expected exactly one structured catalog entry for teaching-learning-based-optimization-rao-savsani-vakharia-2011." }
if ([string]$entry[0].doi -ne "10.1016/j.cad.2010.12.015") { throw "Scientific contract DOI mismatch for teaching-learning-based-optimization-rao-savsani-vakharia-2011." }
if ([string]$entry[0].class -ne "TeachingLearningBasedOptimizationOptimizer") { throw "Scientific contract runtime class mismatch for teaching-learning-based-optimization-rao-savsani-vakharia-2011." }
if ([string]$entry[0].factoryMode -ne "direct") { throw "Scientific contract requires direct factory mode for teaching-learning-based-optimization-rao-savsani-vakharia-2011." }

$page = [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)
if (-not $page.Contains("10.1016/j.cad.2010.12.015") -or -not $page.Contains("teaching-learning-based-optimization-rao-savsani-vakharia-2011") -or -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source = [System.IO.File]::ReadAllText($sourcePath, [System.Text.Encoding]::UTF8)
if (-not $source.Contains("MetaheuristicAlgorithmIds.TeachingLearningBasedOptimization") -or -not $source.Contains("TeachingLearningBasedOptimizationReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: teaching-learning-based-optimization-rao-savsani-vakharia-2011" -ForegroundColor Green
