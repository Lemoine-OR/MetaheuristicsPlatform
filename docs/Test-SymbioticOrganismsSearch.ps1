[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$catalogPath=Join-Path $Root "docs\algorithm-catalog.json"
$pagePath=Join-Path $Root "docs\\pages\\algorithms\\symbiotic-organisms-search-cheng-prayogo-2014.md"
$sourcePath=Join-Path $Root "src\\MetaheuristicsPlatform\\Algorithms\\SymbioticOrganismsSearch\\SymbioticOrganismsSearchOptimizer.cs"
foreach($requiredPath in @($catalogPath,$pagePath,$sourcePath)){if(-not(Test-Path -LiteralPath $requiredPath -PathType Leaf)){throw "Scientific contract missing required file '$requiredPath'."}}
$catalog=[System.IO.File]::ReadAllText($catalogPath,[System.Text.Encoding]::UTF8)|ConvertFrom-Json
$entry=@($catalog.algorithms|Where-Object{[string]$_.id -eq "symbiotic-organisms-search-cheng-prayogo-2014"})
if($entry.Count -ne 1){throw "Scientific contract expected exactly one structured catalog entry for symbiotic-organisms-search-cheng-prayogo-2014."}
if([string]$entry[0].doi -ne "10.1016/j.compstruc.2014.03.007"){throw "Scientific contract DOI mismatch for symbiotic-organisms-search-cheng-prayogo-2014."}
if([string]$entry[0].class -ne "SymbioticOrganismsSearchOptimizer"){throw "Scientific contract runtime class mismatch for symbiotic-organisms-search-cheng-prayogo-2014."}
if([string]$entry[0].factoryMode -ne "direct"){throw "Scientific contract requires direct factory mode for symbiotic-organisms-search-cheng-prayogo-2014."}
$page=[System.IO.File]::ReadAllText($pagePath,[System.Text.Encoding]::UTF8)
if(-not $page.Contains("10.1016/j.compstruc.2014.03.007") -or -not $page.Contains("symbiotic-organisms-search-cheng-prayogo-2014") -or -not $page.Contains("### Update equations / iterations")){throw "Scientific contract page lacks structured identity/equation sections."}
$source=[System.IO.File]::ReadAllText($sourcePath,[System.Text.Encoding]::UTF8)
if(-not $source.Contains("MetaheuristicAlgorithmIds.SymbioticOrganismsSearch") -or -not $source.Contains("SymbioticOrganismsSearchReferences")){throw "Scientific contract source is not bound to the canonical ID/reference object."}
Write-Host "Scientific structured contract GREEN: symbiotic-organisms-search-cheng-prayogo-2014" -ForegroundColor Green
