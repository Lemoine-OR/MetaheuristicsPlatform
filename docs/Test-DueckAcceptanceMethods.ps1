[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference="Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$r){
$p=Join-Path $Root $r
if(-not(Test-Path -LiteralPath $p)){throw "Dueck acceptance validation: missing '$r'."}
[System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8)
}
function C([string]$r,[string[]]$m){
$t=Read-Utf8 $r
foreach($x in $m){if(-not$t.Contains($x)){throw "Dueck acceptance validation: '$r' missing '$x'."}}
}

$v=(Read-Utf8 "version.json")|ConvertFrom-Json
if([version]([string]$v.version)-lt[version]"0.34.0"){throw "Dueck acceptance validation: expected 0.34.0 or later."}

C "src\MetaheuristicsPlatform\Algorithms\Acceptance\GreatDelugeAcceptancePolicy.cs" @(
"ITrajectoryAcceptancePolicy","CandidateObjective <= WaterLevel","CandidateObjective >= WaterLevel","AdvanceLevel","Extended-GDA"
)
C "src\MetaheuristicsPlatform\Algorithms\Acceptance\RecordToRecordTravelAcceptancePolicy.cs" @(
"ITrajectoryAcceptancePolicy","ComputeDegradation","BestObjective","Deviation"
)
C "src\MetaheuristicsPlatform\Trajectory\TrajectoryStepEvaluationAccounting.cs" @(
"RegisterExternalProbeEvaluation","step.Accepted","PromoteOwnedExternalProbeSnapshot"
)
C "src\MetaheuristicsPlatform\Algorithms\Acceptance\GreatDelugeOptimizer.cs" @(
'"great-deluge-dueck-1993"',"ReversibleTrajectoryStepExecutor","TrajectoryStepEvaluationAccounting.RegisterVisitedStep","RainSpeed","10.1006/jcph.1993.1010"
)
C "src\MetaheuristicsPlatform\Algorithms\Acceptance\RecordToRecordTravelOptimizer.cs" @(
'"record-to-record-travel-dueck-1993"',"ReversibleTrajectoryStepExecutor","TrajectoryStepEvaluationAccounting.RegisterVisitedStep","Deviation","10.1006/jcph.1993.1010"
)
C "src\MetaheuristicsPlatform\Algorithms\SA\SimulatedAnnealingOptimizer.cs" @("TrajectoryStepEvaluationAccounting.RegisterVisitedStep")
C "src\MetaheuristicsPlatform\Algorithms\TA\ThresholdAcceptingOptimizer.cs" @("TrajectoryStepEvaluationAccounting.RegisterVisitedStep")
C "tests\MetaheuristicsPlatform.Tests\DueckAcceptanceTrajectoryTests.cs" @(
"GreatDelugeMinimizationUsesAbsoluteWaterLevel","ClassicalGreatDelugeCanRejectCurrentImprovementAboveAdvancedLevel",
"RecordToRecordTravelUsesBestRecordNotCurrentSolution","GreatDelugeExactDeltaRejectsWithoutApplyingMove",
"GreatDelugeDoesNotPromoteRejectedImprovingProbe","StableIdsAndCatalogExposeDueckMethods"
)
C "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs" @(
'"great-deluge-dueck-1993"','"record-to-record-travel-dueck-1993"'
)

$c=(Read-Utf8 "docs\acceptance-based-trajectory-catalog.json")|ConvertFrom-Json
if(@($c.entries|Where-Object status -eq "implemented").Count-lt2){throw "Dueck acceptance validation: expected at least the two Dueck implemented entries."}
if(@($c.entries|Where-Object status -eq "reviewed-deferred").Count-lt2){throw "Dueck acceptance validation: expected at least the two Dueck deferred extensions."}

C "docs\pages\components\acceptance-based-trajectory-methods.md" @(
"@page acceptance_based_trajectory_methods","## Classical Great Deluge","## Classical Record-to-Record Travel",
"## Extended Great Deluge","## Adaptive Flex-Deluge","10.1006/jcph.1993.1010","10.2298/YJOR0302139B","10.1287/ijoc.2015.0680"
)
foreach($p in @(
"docs\pages\algorithms\great-deluge-dueck-1993.md",
"docs\pages\algorithms\record-to-record-travel-dueck-1993.md"
)){
C $p @("## General description","## Technical specifications","## Complexity","## Applicability","## Detailed operation",
"## Parameters","## API example","## Stable factory ID","## Mathematical details","### Problem formulation",
"### Update equations / iterations","### Assumptions","### Convergence conditions","### Scientific references",
"@subpage acceptance_based_trajectory_methods","10.1006/jcph.1993.1010")
}
C "README.md" @("28 public algorithms","19 trajectory methods","great-deluge-dueck-1993","record-to-record-travel-dueck-1993","components/acceptance-based-trajectory-methods.html")

Write-Host "Dueck acceptance validation passed: classical Great Deluge + Record-to-Record Travel executable; Extended GDA + Adaptive Flex-Deluge reviewed/deferred; visited-candidate accounting shared." -ForegroundColor Green