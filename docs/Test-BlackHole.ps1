[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$catalogPath=Join-Path $Root "docs\algorithm-catalog.json"
$pagePath=Join-Path $Root "docs\\pages\\algorithms\\black-hole-algorithm-hatamlou-2013.md"
$sourcePath=Join-Path $Root "src\\MetaheuristicsPlatform\\Algorithms\\BlackHole\\BlackHoleOptimizer.cs"
foreach($requiredPath in @($catalogPath,$pagePath,$sourcePath)){if(-not(Test-Path -LiteralPath $requiredPath -PathType Leaf)){throw "Scientific contract missing required file '$requiredPath'."}}
$catalog=[System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entry=@($catalog.algorithms|Where-Object{[string]$_.id -eq "black-hole-algorithm-hatamlou-2013"})
if($entry.Count -ne 1){throw "Scientific contract expected exactly one structured catalog entry for black-hole-algorithm-hatamlou-2013."}
if([string]$entry[0].doi -ne "10.1016/j.ins.2012.08.023"){throw "Scientific contract DOI mismatch for black-hole-algorithm-hatamlou-2013."}
if([string]$entry[0].class -ne "BlackHoleOptimizer"){throw "Scientific contract runtime class mismatch for black-hole-algorithm-hatamlou-2013."}
if([string]$entry[0].factoryMode -ne "direct"){throw "Scientific contract requires direct factory mode for black-hole-algorithm-hatamlou-2013."}
$page=[System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if(-not $page.Contains("10.1016/j.ins.2012.08.023") -or -not $page.Contains("black-hole-algorithm-hatamlou-2013") -or -not $page.Contains("### Update equations / iterations")){throw "Scientific contract page lacks structured identity/equation sections."}
$source=[System.IO.File]::ReadAllText($sourcePath,[System.Text.Encoding]::UTF8)
if(-not $source.Contains("MetaheuristicAlgorithmIds.BlackHoleAlgorithm") -or -not $source.Contains("BlackHoleReferences")){throw "Scientific contract source is not bound to the canonical ID/reference object."}
Write-Host "Scientific structured contract GREEN: black-hole-algorithm-hatamlou-2013" -ForegroundColor Green
