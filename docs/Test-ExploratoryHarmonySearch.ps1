[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8File([string]$Relative) {
    $p=Join-Path $Root $Relative
    if(-not(Test-Path -LiteralPath $p -PathType Leaf)){throw "EHS validation: missing '$Relative'."}
    return [System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8)
}
$optimizer=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\ExploratoryHarmonySearchOptimizer.cs"
$parameters=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\ExploratoryHarmonySearchParameters.cs"
$tests=Read-Utf8File "tests\\MetaheuristicsPlatform.Tests\\ExploratoryHarmonySearchTests.cs"
$page=Read-Utf8File "docs\\pages\\algorithms\\exploratory-harmony-search-das-mukhopadhyay-roy-abraham-panigrahi-2011.md"
$science=$optimizer+$parameters+$page

foreach($marker in @(
    "exploratory-harmony-search-das-mukhopadhyay-roy-abraham-panigrahi-2011",
    "10.1109/TSMCB.2010.2046035",
    "ExploratoryHarmonySearchOptimizer",
    "StandardDeviationMultiplier",
    "Math.Sqrt(variance)",
    "1.17",
    "0.99",
    "0.33"
)){
    if(-not $science.Contains([string]$marker)){
        throw "EHS validation: scientific marker '$marker' is missing."
    }
}
foreach($marker in @(
    "PublishedDefaultKIsOnePointOneSeven",
    "FineTuningWidthUsesHarmonyMemoryStandardDeviation",
    "FactoryCreatesScientificIdentity"
)){
    if(-not $tests.Contains([string]$marker)){
        throw "EHS validation: focused test '$marker' is missing."
    }
}
foreach($marker in @(
    "## API example","### Problem formulation","### Update equations / iterations",
    "### Assumptions","### Convergence conditions","### Scientific references"
)){
    if(-not $page.Contains($marker)){
        throw "EHS validation: page marker '$marker' is missing."
    }
}
Write-Host "EHS scientific validation passed." -ForegroundColor Green
