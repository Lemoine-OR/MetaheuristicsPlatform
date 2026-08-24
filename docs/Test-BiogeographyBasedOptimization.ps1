[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath =
    Join-Path $Root "docs\algorithm-catalog.json"

$pagePath =
    Join-Path $Root "docs\pages\algorithms\biogeography-based-optimization-simon-2008.md"

$sourcePath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\BiogeographyBasedOptimization\BiogeographyBasedOptimizationOptimizer.cs"

foreach ($requiredPath in @(
    $catalogPath,
    $pagePath,
    $sourcePath
)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Scientific contract missing required file '$requiredPath'."
    }
}

$catalog =
    [System.IO.File]::ReadAllText(
        $catalogPath,
        [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$entry =
    @(
        $catalog.algorithms |
        Where-Object {
            [string]$_.id -eq "biogeography-based-optimization-simon-2008"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract expected exactly one structured catalog entry for biogeography-based-optimization-simon-2008."
}

if ([string]$entry[0].doi -ne "10.1109/TEVC.2008.919004") {
    throw "Scientific contract DOI mismatch for biogeography-based-optimization-simon-2008."
}

if ([string]$entry[0].class -ne "BiogeographyBasedOptimizationOptimizer") {
    throw "Scientific contract runtime class mismatch for biogeography-based-optimization-simon-2008."
}

if ([string]$entry[0].factoryMode -ne "direct") {
    throw "Scientific contract requires direct factory mode for biogeography-based-optimization-simon-2008."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

if (-not $page.Contains("10.1109/TEVC.2008.919004") -or
    -not $page.Contains("biogeography-based-optimization-simon-2008") -or
    -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source =
    [System.IO.File]::ReadAllText(
        $sourcePath,
        [System.Text.Encoding]::UTF8)

if (-not $source.Contains("MetaheuristicAlgorithmIds.BiogeographyBasedOptimization") -or
    -not $source.Contains("BiogeographyBasedOptimizationReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: biogeography-based-optimization-simon-2008" -ForegroundColor Green
