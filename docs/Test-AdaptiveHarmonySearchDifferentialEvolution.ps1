[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8File([string]$Relative) {
    $p=Join-Path $Root $Relative
    if(-not(Test-Path -LiteralPath $p -PathType Leaf)){throw "aHSDE validation: missing '$Relative'."}
    return [System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8)
}
$optimizer=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\AdaptiveHarmonySearchDifferentialEvolutionOptimizer.cs"
$parameters=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\AdaptiveHarmonySearchDifferentialEvolutionParameters.cs"
$tests=Read-Utf8File "tests\\MetaheuristicsPlatform.Tests\\AdaptiveHarmonySearchDifferentialEvolutionTests.cs"
$page=Read-Utf8File "docs\\pages\\algorithms\\adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020.md"
$science=$optimizer+$parameters+$page

foreach($marker in @(
    "adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020",
    "10.3390/app10082916",
    "AdaptiveHarmonySearchDifferentialEvolutionOptimizer",
    "DE/best/2",
    "WeightedLehmerMean",
    "MinimumHarmonyMemorySize",
    "MaximumHarmonyMemorySizePerDimension",
    "LearningPeriod",
    "0.99"
)){
    if(-not $science.Contains([string]$marker)){
        throw "aHSDE validation: scientific marker '$marker' is missing."
    }
}
foreach($marker in @(
    "AdaptiveSampleIsClampedToPublishedRange",
    "LinearHarmonyMemoryReductionIsDocumented",
    "MaximizationIsRejected",
    "FactoryCreatesScientificIdentity"
)){
    if(-not $tests.Contains([string]$marker)){
        throw "aHSDE validation: focused test '$marker' is missing."
    }
}
foreach($marker in @(
    "## API example","### Problem formulation","### Update equations / iterations",
    "### Assumptions","### Convergence conditions","### Scientific references"
)){
    if(-not $page.Contains($marker)){
        throw "aHSDE validation: page marker '$marker' is missing."
    }
}
Write-Host "aHSDE scientific validation passed." -ForegroundColor Green
