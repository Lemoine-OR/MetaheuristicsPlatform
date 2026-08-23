[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8File([string]$Relative) {
    $p=Join-Path $Root $Relative
    if(-not(Test-Path -LiteralPath $p -PathType Leaf)){throw "IHSDE validation: missing '$Relative'."}
    return [System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8)
}
$optimizer=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\ImprovedHarmonySearchDifferentialMutationOptimizer.cs"
$parameters=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\ImprovedHarmonySearchDifferentialMutationParameters.cs"
$tests=Read-Utf8File "tests\\MetaheuristicsPlatform.Tests\\ImprovedHarmonySearchDifferentialMutationTests.cs"
$page=Read-Utf8File "docs\\pages\\algorithms\\improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012.md"
$science=$optimizer+$parameters+$page

foreach($marker in @(
    "improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012",
    "10.1155/2012/147950",
    "ImprovedHarmonySearchDifferentialMutationOptimizer",
    "0.6 + (0.4 * random.NextDouble())",
    "r2 == j || r2 == r1",
    "minimization only"
)){
    if(-not $science.Contains([string]$marker)){
        throw "IHSDE validation: scientific marker '$marker' is missing."
    }
}
foreach($marker in @(
    "ScaleFactorRangeIsPointSixToOne",
    "MaximizationIsRejected",
    "FactoryCreatesScientificIdentity"
)){
    if(-not $tests.Contains([string]$marker)){
        throw "IHSDE validation: focused test '$marker' is missing."
    }
}
foreach($marker in @(
    "## API example","### Problem formulation","### Update equations / iterations",
    "### Assumptions","### Convergence conditions","### Scientific references"
)){
    if(-not $page.Contains($marker)){
        throw "IHSDE validation: page marker '$marker' is missing."
    }
}
Write-Host "IHSDE scientific validation passed." -ForegroundColor Green
