[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$catalogPath=Join-Path $Root "docs\algorithm-catalog.json"
$pagePath=Join-Path $Root "docs\\pages\\algorithms\\multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016.md"
$sourcePath=Join-Path $Root "src\\MetaheuristicsPlatform\\Algorithms\\MultiVerseOptimizer\\MultiVerseOptimizer.cs"
foreach($requiredPath in @($catalogPath,$pagePath,$sourcePath)){if(-not(Test-Path -LiteralPath $requiredPath -PathType Leaf)){throw "Scientific contract missing required file '$requiredPath'."}}
$catalog=[System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entry=@($catalog.algorithms|Where-Object{[string]$_.id -eq "multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016"})
if($entry.Count -ne 1){throw "Scientific contract expected exactly one structured catalog entry for multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016."}
if([string]$entry[0].doi -ne "10.1007/s00521-015-1870-7"){throw "Scientific contract DOI mismatch for multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016."}
if([string]$entry[0].class -ne "MultiVerseOptimizer"){throw "Scientific contract runtime class mismatch for multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016."}
if([string]$entry[0].factoryMode -ne "direct"){throw "Scientific contract requires direct factory mode for multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016."}
$page=[System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if(-not $page.Contains("10.1007/s00521-015-1870-7") -or -not $page.Contains("multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016") -or -not $page.Contains("### Update equations / iterations")){throw "Scientific contract page lacks structured identity/equation sections."}
$source=[System.IO.File]::ReadAllText($sourcePath,[System.Text.Encoding]::UTF8)
if(-not $source.Contains("MetaheuristicAlgorithmIds.MultiVerseOptimizer") -or -not $source.Contains("MultiVerseOptimizerReferences")){throw "Scientific contract source is not bound to the canonical ID/reference object."}
Write-Host "Scientific structured contract GREEN: multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016" -ForegroundColor Green
