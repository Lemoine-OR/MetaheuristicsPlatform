[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath = Join-Path $Root "docs\algorithm-catalog.json"
$pagePath = Join-Path $Root "docs\pages\algorithms\harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019.md"
$sourcePath = Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\HarrisHawks\HarrisHawksOptimizer.cs"

foreach ($requiredPath in @($catalogPath, $pagePath, $sourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog = [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$entry = @($catalog.algorithms | Where-Object { [string]$_.id -eq "harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019" })

if ($entry.Count -ne 1) { throw "Scientific contract expected exactly one structured catalog entry for harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019." }
if ([string]$entry[0].doi -ne "10.1016/j.future.2019.02.028") { throw "Scientific contract DOI mismatch for harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019." }
if ([string]$entry[0].class -ne "HarrisHawksOptimizer") { throw "Scientific contract runtime class mismatch for harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019." }
if ([string]$entry[0].factoryMode -ne "direct") { throw "Scientific contract requires direct factory mode for harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019." }

$page = [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)
if (-not $page.Contains("10.1016/j.future.2019.02.028") -or -not $page.Contains("harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019") -or -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source = [System.IO.File]::ReadAllText($sourcePath, [System.Text.Encoding]::UTF8)
if (-not $source.Contains("MetaheuristicAlgorithmIds.HarrisHawksOptimization") -or -not $source.Contains("HarrisHawksOptimizerReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019" -ForegroundColor Green
