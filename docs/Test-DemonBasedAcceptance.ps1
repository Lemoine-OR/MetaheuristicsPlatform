[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Demon-Based Acceptance validation: missing '$Relative'."
    }
    return [System.IO.File]::ReadAllText($path,[System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative
    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Demon-Based Acceptance validation: '$Relative' is missing '$marker'."
        }
    }
}

$version=(Read-Utf8 "version.json")|ConvertFrom-Json
if([string]$version.version -ne "0.36.0") {
    throw "Demon-Based Acceptance validation: version.json must be 0.36.0."
}

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Acceptance\DemonAcceptanceReferences.cs" @(
    "Creutz1983",
    "Talbi2009",
    "WoodDowns1998",
    "ZimmermannSalamon1992"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Acceptance\DemonAcceptancePolicy.cs" @(
    "energyChange <= Credit",
    "Credit - energyChange",
    "ComputeEnergyChange",
    "Math.Max(0.0, next)"
)

Require-Contains "src\MetaheuristicsPlatform\Algorithms\Acceptance\DemonBasedAcceptanceOptimizer.cs" @(
    "demon-based-acceptance-talbi-2009",
    "DemonAcceptanceReferences.Creutz1983",
    "DemonAcceptanceReferences.Talbi2009",
    "TrajectoryStepEvaluationAccounting.RegisterVisitedStep",
    "policy.CompleteTransition"
)

Require-Contains "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
    "demon-based-acceptance-talbi-2009"
)

Require-Contains "tests\MetaheuristicsPlatform.Tests\DemonBasedAcceptanceTests.cs" @(
    "AcceptedMovesPreserveMinimizationEnergyInvariant",
    "AcceptedMovesPreserveMaximizationEnergyInvariant",
    "MaximizationMirrorsDemonEnergyOrientation",
    "ExactDeltaRejectionDoesNotApplyMove",
    "StableIdAndCatalogExposeDemonBasedAcceptance"
)

Require-Contains "docs\pages\algorithms\demon-based-acceptance-talbi-2009.md" @(
    "conserved-credit controller",
    "10.1103/PhysRevLett.50.1411",
    "10.1002/9780470496916.ch2",
    "Zimmermann",
    "@subpage acceptance_based_trajectory_methods"
)

$catalog=(Read-Utf8 "docs\acceptance-based-trajectory-catalog.json")|ConvertFrom-Json
$demon=@($catalog.entries|Where-Object { [string]$_.id -eq "acceptance.demon.budget" })
if($demon.Count -ne 1 -or [string]$demon[0].status -ne "implemented") {
    throw "Demon-Based Acceptance validation: conserved credit-budget component must be uniquely implemented."
}
if([string]$demon[0].formulaMode -ne "math") {
    throw "Demon-Based Acceptance validation: conserved credit-budget component must expose mathematics."
}

foreach($deferredId in @(
    "acceptance.demon.credit-reset-ils",
    "acceptance.demon.zimmermann-salamon-1992"
)) {
    $d=@($catalog.entries|Where-Object { [string]$_.id -eq $deferredId })
    if($d.Count -ne 1 -or [string]$d[0].status -ne "reviewed-deferred") {
        throw "Demon-Based Acceptance validation: '$deferredId' must remain separately reviewed/deferred."
    }
}

if([int]$catalog.implementedCount -ne 4 -or [int]$catalog.reviewedDeferredCount -ne 4) {
    throw "Demon-Based Acceptance validation: acceptance catalog counts must be 4 implemented / 4 deferred."
}

Write-Host "Demon-Based Acceptance validation passed: conserved Creutz/Talbi credit-energy controller executable; ILS credit-reset and Zimmermann-Salamon ensemble lineages remain distinct; Wood-Downs variants are documented without fabricated DOI metadata." -ForegroundColor Green
