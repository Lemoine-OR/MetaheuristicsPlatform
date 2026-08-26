[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$catalogPath=Join-Path $Root "docs\algorithm-catalog.json"
$pagePath=Join-Path $Root "docs\\pages\\algorithms\\imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007.md"
$sourcePath=Join-Path $Root "src\\MetaheuristicsPlatform\\Algorithms\\ImperialistCompetitiveAlgorithm\\ImperialistCompetitiveAlgorithmOptimizer.cs"
foreach($requiredPath in @($catalogPath,$pagePath,$sourcePath)){if(-not(Test-Path -LiteralPath $requiredPath -PathType Leaf)){throw "Scientific contract missing required file '$requiredPath'."}}
$catalog=[System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entry=@($catalog.algorithms|Where-Object{[string]$_.id -eq "imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007"})
if($entry.Count -ne 1){throw "Scientific contract expected exactly one structured catalog entry for imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007."}
if([string]$entry[0].doi -ne "10.1109/CEC.2007.4425083"){throw "Scientific contract DOI mismatch for imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007."}
if([string]$entry[0].class -ne "ImperialistCompetitiveAlgorithmOptimizer"){throw "Scientific contract runtime class mismatch for imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007."}
if([string]$entry[0].factoryMode -ne "direct"){throw "Scientific contract requires direct factory mode for imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007."}
$page=[System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if(-not $page.Contains("10.1109/CEC.2007.4425083") -or -not $page.Contains("imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007") -or -not $page.Contains("### Update equations / iterations")){throw "Scientific contract page lacks structured identity/equation sections."}
$source=[System.IO.File]::ReadAllText($sourcePath,[System.Text.Encoding]::UTF8)
if(-not $source.Contains("MetaheuristicAlgorithmIds.ImperialistCompetitiveAlgorithm") -or -not $source.Contains("ImperialistCompetitiveAlgorithmReferences")){throw "Scientific contract source is not bound to the canonical ID/reference object."}
Write-Host "Scientific structured contract GREEN: imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007" -ForegroundColor Green
