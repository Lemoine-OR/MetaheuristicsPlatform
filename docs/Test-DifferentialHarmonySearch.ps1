[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8File([string]$Relative) {
    $p=Join-Path $Root $Relative
    if(-not(Test-Path -LiteralPath $p -PathType Leaf)){throw "DHS validation: missing '$Relative'."}
    return [System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8)
}
$optimizer=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\DifferentialHarmonySearchOptimizer.cs"
$parameters=Read-Utf8File "src\\MetaheuristicsPlatform\\Algorithms\\HarmonySearch\\DifferentialHarmonySearchParameters.cs"
$tests=Read-Utf8File "tests\\MetaheuristicsPlatform.Tests\\DifferentialHarmonySearchTests.cs"
$page=Read-Utf8File "docs\\pages\\algorithms\\differential-harmony-search-chakraborty-roy-das-jain-abraham-2009.md"
$science=$optimizer+$parameters+$page

foreach($marker in @(
    "differential-harmony-search-chakraborty-roy-das-jain-abraham-2009",
    "10.3233/FI-2009-157",
    "DifferentialHarmonySearchOptimizer",
    "Eq. (5)",
    "DE/rand/1",
    "scaleFactor = random.NextDouble()",
    "Minimize",
    "maximization"
)){
    if(-not $science.Contains([string]$marker)){
        throw "DHS validation: scientific marker '$marker' is missing."
    }
}
foreach($marker in @(
    "SupportsPublishedMinimizeOrMaximize",
    "ScaleFactorIsUniformUnitInterval",
    "FactoryCreatesScientificIdentity"
)){
    if(-not $tests.Contains([string]$marker)){
        throw "DHS validation: focused test '$marker' is missing."
    }
}
foreach($marker in @(
    "## API example","### Problem formulation","### Update equations / iterations",
    "### Assumptions","### Convergence conditions","### Scientific references"
)){
    if(-not $page.Contains($marker)){
        throw "DHS validation: page marker '$marker' is missing."
    }
}
Write-Host "DHS scientific validation passed." -ForegroundColor Green
