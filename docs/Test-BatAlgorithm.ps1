[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath =
    Join-Path $Root "docs\algorithm-catalog.json"

$pagePath =
    Join-Path $Root "docs\pages\algorithms\bat-algorithm-yang-2010.md"

$sourcePath =
    Join-Path $Root "src\MetaheuristicsPlatform\Algorithms\BatAlgorithm\BatAlgorithmOptimizer.cs"

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
            [string]$_.id -eq "bat-algorithm-yang-2010"
        }
    )

if ($entry.Count -ne 1) {
    throw "Scientific contract expected exactly one structured catalog entry for bat-algorithm-yang-2010."
}

if ([string]$entry[0].doi -ne "10.1007/978-3-642-12538-6_6") {
    throw "Scientific contract DOI mismatch for bat-algorithm-yang-2010."
}

if ([string]$entry[0].class -ne "BatAlgorithmOptimizer") {
    throw "Scientific contract runtime class mismatch for bat-algorithm-yang-2010."
}

if ([string]$entry[0].factoryMode -ne "direct") {
    throw "Scientific contract requires direct factory mode for bat-algorithm-yang-2010."
}

$page =
    [System.IO.File]::ReadAllText(
        $pagePath,
        [System.Text.Encoding]::UTF8)

if (-not $page.Contains("10.1007/978-3-642-12538-6_6") -or
    -not $page.Contains("bat-algorithm-yang-2010") -or
    -not $page.Contains("### Update equations / iterations")) {
    throw "Scientific contract page lacks structured identity/equation sections."
}

$source =
    [System.IO.File]::ReadAllText(
        $sourcePath,
        [System.Text.Encoding]::UTF8)

if (-not $source.Contains("MetaheuristicAlgorithmIds.BatAlgorithm") -or
    -not $source.Contains("BatAlgorithmReferences")) {
    throw "Scientific contract source is not bound to the canonical ID/reference object."
}

Write-Host "Scientific structured contract GREEN: bat-algorithm-yang-2010" -ForegroundColor Green
