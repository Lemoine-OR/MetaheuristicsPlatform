[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8File([string]$Relative) {
    $p=Join-Path $Root $Relative
    if(-not(Test-Path -LiteralPath $p -PathType Leaf)){throw "NSHS validation: missing '$Relative'."}
    return [System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8)
}
$optimizer=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\NovelSelfAdaptiveHarmonySearchOptimizer.cs"
$parameters=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\NovelSelfAdaptiveHarmonySearchParameters.cs"
$tests=Read-Utf8File "tests\\MetaheuristicsPlatform.Tests\\NovelSelfAdaptiveHarmonySearchTests.cs"
$page=Read-Utf8File "docs\\pages\\algorithms\\novel-self-adaptive-harmony-search-luo-2013.md"
$science=$optimizer+$parameters+$page

foreach($marker in @(
    "novel-self-adaptive-harmony-search-luo-2013",
    "10.1155/2013/653749",
    "NovelSelfAdaptiveHarmonySearchOptimizer",
    "1.0 - (1.0 / (dimension + 1.0))",
    "0.0001",
    "ReportedPitchAdjustmentRate => 0.0",
    "fstd"
)){
    if(-not $science.Contains([string]$marker)){
        throw "NSHS validation: scientific marker '$marker' is missing."
    }
}
foreach($marker in @(
    "HmcrIsDimensionDerived",
    "ParIsRemoved",
    "MaximizationIsRejected",
    "FactoryCreatesScientificIdentity"
)){
    if(-not $tests.Contains([string]$marker)){
        throw "NSHS validation: focused test '$marker' is missing."
    }
}
foreach($marker in @(
    "## API example","### Problem formulation","### Update equations / iterations",
    "### Assumptions","### Convergence conditions","### Scientific references"
)){
    if(-not $page.Contains($marker)){
        throw "NSHS validation: page marker '$marker' is missing."
    }
}
Write-Host "NSHS scientific validation passed." -ForegroundColor Green
