[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Late Acceptance validation: missing '$Relative'."
    }
    return [System.IO.File]::ReadAllText($path,[System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative
    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Late Acceptance validation: '$Relative' is missing '$marker'."
        }
    }
}

$version=(Read-Utf8 "version.json")|ConvertFrom-Json
if([version]([string]$version.version) -lt [version]"0.35.0") {
    throw "Late Acceptance validation: expected version 0.35.0 or later."
}

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Acceptance\LateAcceptancePolicy.cs" @(
    "CurrentReference",
    "CompleteTransition",
    "IsNoWorse",
    "TrajectoryObjectiveComparison.IsBetter"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Acceptance\LateAcceptanceHillClimbingOptimizer.cs" @(
    "late-acceptance-hill-climbing-burke-bykov-2017",
    "LateAcceptanceReferences.BurkeBykov2017",
    "TrajectoryStepEvaluationAccounting.RegisterVisitedStep",
    "policy.CompleteTransition"
)

Require-Contains "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
    "late-acceptance-hill-climbing-burke-bykov-2017"
)

Require-Contains "docs\pages\algorithms\late-acceptance-hill-climbing-burke-bykov-2017.md" @(
    "final Burke-Bykov formulation",
    "10.1016/j.ejor.2016.07.012",
    "HistoryLength = 1",
    "@subpage acceptance_based_trajectory_methods"
)

$catalog=(Read-Utf8 "docs\acceptance-based-trajectory-catalog.json")|ConvertFrom-Json
$lahc=@($catalog.entries|Where-Object { [string]$_.id -eq "acceptance.lahc.burke-bykov-2017" })
if($lahc.Count -ne 1 -or [string]$lahc[0].status -ne "implemented") {
    throw "Late Acceptance validation: LAHC component entry must be uniquely implemented."
}
if([string]$lahc[0].doi -ne "10.1016/j.ejor.2016.07.012") {
    throw "Late Acceptance validation: incorrect LAHC DOI."
}

foreach($demonId in @(
    "acceptance.demon.budget",
    "acceptance.demon.zimmermann-salamon-1992"
)) {
    $d=@($catalog.entries|Where-Object { [string]$_.id -eq $demonId })
    if($d.Count -ne 1) {
        throw "Late Acceptance validation: distinct Demon entry '$demonId' must remain cataloged."
    }
}

Write-Host "Late Acceptance validation passed: final Burke-Bykov LAHC executable; distinct Demon identities remain separately cataloged." -ForegroundColor Green
