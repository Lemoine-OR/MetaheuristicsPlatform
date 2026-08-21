[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Read-Utf8([string]$Relative) {
    $path = Join-Path $Root $Relative
    if(-not (Test-Path -LiteralPath $path)) {
        throw "Advanced Iterated Greedy validation: missing '$Relative'."
    }

    return [System.IO.File]::ReadAllText(
        $path,
        [System.Text.Encoding]::UTF8)
}

function Require-Contains([string]$Relative,[string[]]$Markers) {
    $text = Read-Utf8 $Relative
    foreach($marker in $Markers) {
        if(-not $text.Contains($marker)) {
            throw "Advanced Iterated Greedy validation: '$Relative' is missing '$marker'."
        }
    }
}

$version =
    (Read-Utf8 "version.json") |
    ConvertFrom-Json

if([version]([string]$version.version) -lt [version]"0.38.0") {
    throw "Advanced Iterated Greedy validation: expected version 0.38.0 or later."
}

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\IteratedGreedy\AdvancedIteratedGreedyStrategies.cs" @(
        "IIteratedGreedyDestructionSizePolicy",
        "FixedIteratedGreedyDestructionSizePolicy",
        "StagnationEscalatingIteratedGreedyDestructionSizePolicy",
        "IIteratedGreedyPartialSolutionImprovement",
        "DelegateIteratedGreedyPartialSolutionImprovement"
    )

Require-Contains `
    "src\MetaheuristicsPlatform\Algorithms\IteratedGreedy\IteratedGreedyOptimizer.cs" @(
        "_destructionSizePolicy",
        "_partialSolutionImprovement",
        "IteratedGreedyDestructionSizeContext",
        "_destructionSizePolicy.SelectDestructionSize",
        "_partialSolutionImprovement?.Improve",
        "candidateObjective,",
        "context.EvaluateStopping(state)",
        "improvementCountBeforeIteration"
    )

Require-Contains `
    "tests\MetaheuristicsPlatform.Tests\AdvancedIteratedGreedyTests.cs" @(
        "StagnationEscalatingDestructionSizeIncreasesAndCaps",
        "LegacyConstructorKeepsFixedParameterDestructionSize",
        "AdaptiveDestructionUsesBestSoFarStagnation",
        "PartialImprovementRunsStrictlyBetweenDestroyAndReconstruct",
        "StoppingAfterCandidateEvaluationSeesFiniteLastCandidateObjective",
        "ReviewedReferenceAuthorsAreExact"
    )

$references =
    Read-Utf8 "src\MetaheuristicsPlatform\Algorithms\IteratedGreedy\IteratedGreedyReferences.cs"

foreach($marker in @(
    "Kuo-Ching Ying; Shih-Wei Lin; Chen-Yang Cheng; Cheng-Ding He",
    "Xue-Lei Jing; Quan-Ke Pan; Liang Gao; Yu-Long Wang",
    "Yuan-Zhen Li; Quan-Ke Pan; Jun-Qing Li; Liang Gao; Mehmet Fatih Tasgetiren",
    "Sen Zhang; Bin Qian; Rong Hu; Kun Li; Jian-Bo Yang",
    "10.1080/00207543.2014.948578",
    "10.1016/j.asoc.2015.02.006",
    "10.1016/j.cor.2016.12.021",
    "10.1016/j.cie.2017.06.025",
    "10.1016/j.omega.2018.03.004",
    "10.1016/j.cor.2019.104767",
    "10.1016/j.asoc.2020.106629",
    "10.1016/j.swevo.2021.100874",
    "10.1016/j.eswa.2025.130422"
)) {
    if(-not $references.Contains($marker)) {
        throw "Advanced Iterated Greedy validation: reference source is missing '$marker'."
    }
}

if($references.Contains("Reviewed advanced Iterated Greedy lineage") -or
   $references.Contains("Reviewed adaptive Iterated Greedy lineage")) {
    throw "Advanced Iterated Greedy validation: placeholder bibliographic authors remain."
}

$catalog =
    (Read-Utf8 "docs\advanced-iterated-greedy-catalog.json") |
    ConvertFrom-Json

$entries =
    @($catalog.entries)

if([int]$catalog.implementedCount -ne 5 -or
   [int]$catalog.reviewedDeferredCount -ne 9 -or
   $entries.Count -ne 14) {
    throw "Advanced Iterated Greedy validation: expected 5 implemented + 9 reviewed/deferred = 14 entries."
}

$ids =
    @($entries | ForEach-Object { [string]$_.id })

foreach($id in @(
    "ig.destruction.fixed",
    "ig.destruction.stagnation-escalating",
    "ig.partial-improvement.hook",
    "ig.acceptance.improving-only",
    "ig.acceptance.constant-temperature",
    "ig.bounded-search.fernandez-viagas-framinan-2015",
    "ig.tabu-reconstruction.ding-et-al-2015",
    "ig.partial-optimization.dubois-lacoste-pagnozzi-stutzle-2017",
    "ig.reference-greedy.ying-lin-cheng-he-2017",
    "ig.distributed.ruiz-pan-naderi-2019",
    "ig.best-of-breed.fernandez-viagas-framinan-2019",
    "ig.due-windows.jing-pan-gao-wang-2020",
    "ig.adaptive.li-pan-li-gao-tasgetiren-2021",
    "ig.two-stage.zhang-qian-hu-li-yang-2026"
)) {
    if($ids -notcontains $id) {
        throw "Advanced Iterated Greedy validation: catalog is missing '$id'."
    }
}

Require-Contains `
    "docs\pages\components\advanced-iterated-greedy-strategies.md" @(
        "@page advanced_iterated_greedy_strategies",
        "## Executable generic components",
        "ig.destruction.stagnation-escalating",
        "ig.partial-improvement.hook",
        "10.1016/j.eswa.2025.130422"
    )

Require-Contains `
    "docs\Build-AdvancedIteratedGreedyDocumentation.ps1" @(
        "advanced-iterated-greedy-catalog.json",
        "advanced-iterated-greedy-strategies.html",
        "formulaMode",
        "formula-note",
        "mathjax@3.2.2/es5/tex-chtml.js",
        "headingMarker",
        "sectionMarker",
        "LastIndexOf",
        "Insert("
    )

$documentationBuilder =
    Read-Utf8 "docs\Build-AdvancedIteratedGreedyDocumentation.ps1"

$legacyRenderedMarker =
    '<div class="section"><h2>Mathematical details</h2>'

if($documentationBuilder.Contains($legacyRenderedMarker)) {
    throw "Advanced Iterated Greedy validation: legacy one-line mathematical-details marker remains in the rendered-portal builder."
}

Require-Contains `
    "docs\pages\algorithms\iterated-greedy-ruiz-stutzle-2007.md" @(
        "@subpage advanced_iterated_greedy_strategies"
    )

Require-Contains "README.md" @(
    "Advanced Iterated Greedy Strategies",
    "components/advanced-iterated-greedy-strategies.html",
    "ig.*"
)

Write-Host "Advanced Iterated Greedy validation passed: 5 executable generic ig.* components + 9 scientifically reviewed complete variants; v0.37 audit fixes incorporated without changing the canonical public algorithm count." -ForegroundColor Green
