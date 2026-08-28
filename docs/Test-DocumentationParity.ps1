[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$catalogPath =
    Join-Path $Root "docs\algorithm-catalog.json"

if (-not (Test-Path $catalogPath)) {
    throw "Missing docs/algorithm-catalog.json."
}

$catalog =
    [System.IO.File]::ReadAllText($catalogPath, [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

$algorithms = @($catalog.algorithms)
$families = @($catalog.families)

& (Join-Path $Root "docs\Test-EpsilonConstrainedDe.ps1") -Root $Root

& (Join-Path $Root "docs\Test-TessemaYenPenaltyGa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-AdaptivePenaltyGa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-HomaifarPenaltyGa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-JoinesHouckPenaltyGa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-DominanceTournamentGa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-StochasticRankingEs.ps1") -Root $Root

& (Join-Path $Root "docs\Test-DebConstraintGa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Vaea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Knea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-ThetaDea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Moeadd.ps1") -Root $Root

& (Join-Path $Root "docs\Test-TwoArch2.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Hype.ps1") -Root $Root

& (Join-Path $Root "docs\Test-MoeadDe.ps1") -Root $Root

& (Join-Path $Root "docs\Test-MoCmaEs.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Grea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Nsga.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Spea2.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Spea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Rvea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-SmsEmoa.ps1") -Root $Root

& (Join-Path $Root "docs\Test-NsgaIII.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Smpso.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Mopso.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Moead.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Ibea.ps1") -Root $Root

& (Join-Path $Root "docs\Test-PesaII.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Paes.ps1") -Root $Root

& (Join-Path $Root "docs\Test-NsgaII.ps1") -Root $Root

& (Join-Path $Root "docs\Test-SpeciesBasedParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-StandardParticleSwarm2007.ps1") -Root $Root

& (Join-Path $Root "docs\Test-CooperativeParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-ComprehensiveLearningParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-FullyInformedParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-BareBonesParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-ConstrictionParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-InertiaWeightParticleSwarm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-EquilibriumOptimizer.ps1") -Root $Root

& (Join-Path $Root "docs\Test-MultiVerseOptimizer.ps1") -Root $Root

& (Join-Path $Root "docs\Test-SymbioticOrganismsSearch.ps1") -Root $Root

& (Join-Path $Root "docs\Test-BlackHole.ps1") -Root $Root

& (Join-Path $Root "docs\Test-ImperialistCompetitiveAlgorithm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-Jaya.ps1") -Root $Root

& (Join-Path $Root "docs\Test-CrowSearch.ps1") -Root $Root

& (Join-Path $Root "docs\Test-TeachingLearningBasedOptimization.ps1") -Root $Root

& (Join-Path $Root "docs\Test-GravitationalSearch.ps1") -Root $Root

& (Join-Path $Root "docs\Test-BigBangBigCrunch.ps1") -Root $Root

& (Join-Path $Root "docs\Test-HarrisHawksOptimization.ps1") -Root $Root

& (Join-Path $Root "docs\Test-SalpSwarmAlgorithm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-SineCosineAlgorithm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-WhaleOptimizationAlgorithm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-MothFlameOptimization.ps1") -Root $Root

& (Join-Path $Root "docs\Test-GreyWolfOptimizer.ps1") -Root $Root

& (Join-Path $Root "docs\Test-FlowerPollinationAlgorithm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-BatAlgorithm.ps1") -Root $Root

& (Join-Path $Root "docs\Test-CuckooSearch.ps1") -Root $Root

& (Join-Path $Root "docs\Test-BiogeographyBasedOptimization.ps1") -Root $Root

if ($algorithms.Count -lt 115) {
    throw "Documentation parity: expected at least 57 currently public algorithms."
}

$requiredFields = @(
    "id","name","class","category","kind","family","time","space",
    "applicability","sourcePath","publication","doi","implementation",
    "page","problem","update","assumptions","convergence","factoryMode"
)

$ids = @{}

foreach ($algorithm in $algorithms) {
    foreach ($field in $requiredFields) {
        $value = $algorithm.$field

        if ($null -eq $value -or
            [string]::IsNullOrWhiteSpace([string]$value)) {
            throw "Documentation parity: '$($algorithm.id)' is missing '$field'."
        }
    }

    if ($ids.ContainsKey($algorithm.id)) {
        throw "Documentation parity: duplicate stable ID '$($algorithm.id)'."
    }

    $ids[$algorithm.id] = $true

    $pagePath =
        Join-Path $Root $algorithm.page

    if (-not (Test-Path $pagePath)) {
        throw "Documentation parity: missing page '$($algorithm.page)'."
    }

    $page =
        [System.IO.File]::ReadAllText($pagePath, [System.Text.Encoding]::UTF8)

    $requiredPageMarkers = @(
        "## General description",
        "## Technical specifications",
        "## Complexity",
        "## Applicability",
        "## Detailed operation",
        "## Parameters",
        "## API example",
        "## Stable factory ID",
        "## Mathematical details",
        "### Problem formulation",
        "### Update equations / iterations",
        "### Assumptions",
        "### Convergence conditions",
        "### Scientific references",
        $algorithm.id,
        $algorithm.doi
    )

    foreach ($marker in $requiredPageMarkers) {
        if (-not $page.Contains([string]$marker)) {
            throw "Documentation parity: '$($algorithm.id)' page is missing '$marker'."
        }
    }

    $sourcePath =
        Join-Path $Root $algorithm.sourcePath

    if (-not (Test-Path $sourcePath)) {
        throw "Documentation parity: catalog source path does not exist: '$($algorithm.sourcePath)'."
    }
}

foreach ($family in $families) {
    $familyPath =
        Join-Path $Root "docs\pages\families\$($family.id).md"

    if (-not (Test-Path $familyPath)) {
        throw "Documentation parity: missing family page '$($family.id)'."
    }

    $familyLines =
        [System.IO.File]::ReadAllLines(
            $familyPath,
            [System.Text.Encoding]::UTF8)

    foreach ($familyLine in $familyLines) {
        $subpageCount =
            [regex]::Matches(
                [string]$familyLine,
                [regex]::Escape("@subpage")).Count

        if ($subpageCount -gt 1) {
            throw (
                "Documentation parity: family page '$($family.id)' contains multiple @subpage directives on one line.")
        }
    }
}

$requiredRepoFiles = @(
    "README.md",
    "CHANGELOG.md",
    "version.json",
    "API-STABILITY.md",
    "CITATION.cff",
    "docs\Doxyfile",
    "docs\mainpage.md",
    "docs\build-documentation.ps1",
    "docs\Test-DocumentationLinks.ps1",
    "docs\Test-DocumentationParity.ps1",
    "docs\Test-TextEncoding.ps1",
    "docs\Test-SimulatedAnnealingCoolingCatalog.ps1",
    "docs\Build-SimulatedAnnealingCoolingDocumentation.ps1",
    "docs\sa-cooling-catalog.json",
    "docs\pages\components\simulated-annealing-cooling-schedules.md",
    "docs\Test-ThresholdAccepting.ps1",
    "docs\threshold-accepting-schedule-catalog.json",
    "docs\Build-ThresholdAcceptingScheduleDocumentation.ps1",
    "docs\pages\components\threshold-accepting-schedules.md",
    "docs\pages\algorithms\threshold-accepting-dueck-scheuer-1990.md",
    "docs\Test-DueckAcceptanceMethods.ps1",
    "docs\acceptance-based-trajectory-catalog.json",
    "docs\Build-AcceptanceBasedTrajectoryDocumentation.ps1",
    "docs\pages\components\acceptance-based-trajectory-methods.md",
    "docs\pages\algorithms\great-deluge-dueck-1993.md",
    "docs\pages\algorithms\record-to-record-travel-dueck-1993.md",
    "docs\pages\algorithms\late-acceptance-hill-climbing-burke-bykov-2017.md",
    "docs\Test-LateAcceptance.ps1",
    "docs\Test-DemonBasedAcceptance.ps1",
    "docs\Test-IteratedGreedy.ps1",
    "docs\Test-AdvancedIteratedGreedy.ps1",
    "docs\Test-ScatterSearch.ps1",
    "docs\Test-AdvancedScatterSearch.ps1",
    "docs\Test-GeneticAlgorithm.ps1",
    "docs\Test-AdvancedGeneticAlgorithm.ps1",
    "docs\Test-MemeticAlgorithm.ps1",
    "docs\Test-AntSystem.ps1",
    "docs\Test-AdvancedAntColony.ps1",
    "docs\Test-ArtificialBeeColony.ps1",
    "docs\Test-FireflyAlgorithm.ps1",
    "docs\Test-HarmonySearch.ps1",
    "docs\pages\algorithms\harmony-search-geem-kim-loganathan-2001.md",
    "docs\Test-ImprovedHarmonySearch.ps1",
    "docs\pages\algorithms\improved-harmony-search-mahdavi-fesanghary-damangir-2007.md",
    "docs\Test-GlobalBestHarmonySearch.ps1",
    "docs\pages\algorithms\global-best-harmony-search-omran-mahdavi-2008.md",
    "docs\Test-SelfAdaptiveGlobalBestHarmonySearch.ps1",
    "docs\pages\algorithms\self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010.md",
    "docs\Test-NovelGlobalHarmonySearch.ps1",
    "docs\pages\algorithms\novel-global-harmony-search-zou-gao-wu-li-2010.md",
    "docs\Test-ParameterSettingFreeHarmonySearch.ps1",
    "docs\pages\algorithms\parameter-setting-free-harmony-search-geem-sim-2010.md",
    "docs\Test-AdvancedParameterSettingFreeHarmonySearchIteration.ps1",
    "docs\pages\algorithms\advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020.md",
    "docs\Test-AdvancedParameterSettingFreeHarmonySearchObject.ps1",
    "docs\pages\algorithms\advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020.md",
    "docs\Test-DifferentialHarmonySearch.ps1",
    "docs\pages\algorithms\differential-harmony-search-chakraborty-roy-das-jain-abraham-2009.md",
    "docs\Test-ExploratoryHarmonySearch.ps1",
    "docs\pages\algorithms\exploratory-harmony-search-das-mukhopadhyay-roy-abraham-panigrahi-2011.md",
    "docs\Test-ImprovedHarmonySearchDifferentialMutation.ps1",
    "docs\pages\algorithms\improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012.md",
    "docs\Test-NovelSelfAdaptiveHarmonySearch.ps1",
    "docs\pages\algorithms\novel-self-adaptive-harmony-search-luo-2013.md",
    "docs\Test-AdaptiveHarmonySearchDifferentialEvolution.ps1",
    "docs\pages\algorithms\adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020.md",
    "docs\pages\families\other-metaheuristics.md",
    "docs\Test-BiogeographyBasedOptimization.ps1",
    "docs\pages\algorithms\biogeography-based-optimization-simon-2008.md",
    "docs\Test-CuckooSearch.ps1",
    "docs\pages\algorithms\cuckoo-search-yang-deb-2009.md",
    "docs\Test-BatAlgorithm.ps1",
    "docs\pages\algorithms\bat-algorithm-yang-2010.md",
    "docs\Test-FlowerPollinationAlgorithm.ps1",
    "docs\pages\algorithms\flower-pollination-algorithm-yang-2012.md",
    "docs\Test-GreyWolfOptimizer.ps1",
    "docs\pages\algorithms\grey-wolf-optimizer-mirjalili-mirjalili-lewis-2014.md",
    "docs\Test-MothFlameOptimization.ps1",
    "docs\pages\algorithms\moth-flame-optimization-mirjalili-2015.md",
    "docs\Test-WhaleOptimizationAlgorithm.ps1",
    "docs\pages\algorithms\whale-optimization-algorithm-mirjalili-lewis-2016.md",
    "docs\Test-SineCosineAlgorithm.ps1",
    "docs\pages\algorithms\sine-cosine-algorithm-mirjalili-2016.md",
    "docs\Test-SalpSwarmAlgorithm.ps1",
    "docs\pages\algorithms\salp-swarm-algorithm-mirjalili-gandomi-mirjalili-saremi-faris-mirjalili-2017.md",
    "docs\Test-HarrisHawksOptimization.ps1",
    "docs\pages\algorithms\harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019.md",
    "docs\Test-BigBangBigCrunch.ps1",
    "docs\pages\algorithms\big-bang-big-crunch-erol-eksin-2006.md",
    "docs\Test-GravitationalSearch.ps1",
    "docs\pages\algorithms\gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009.md",
    "docs\Test-TeachingLearningBasedOptimization.ps1",
    "docs\pages\algorithms\teaching-learning-based-optimization-rao-savsani-vakharia-2011.md",
    "docs\Test-CrowSearch.ps1",
    "docs\pages\algorithms\crow-search-algorithm-askarzadeh-2016.md",
    "docs\Test-Jaya.ps1",
    "docs\pages\algorithms\jaya-algorithm-rao-2016.md",
    "docs\Test-ImperialistCompetitiveAlgorithm.ps1",
    "docs\pages\algorithms\imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007.md",
    "docs\Test-BlackHole.ps1",
    "docs\pages\algorithms\black-hole-algorithm-hatamlou-2013.md",
    "docs\Test-SymbioticOrganismsSearch.ps1",
    "docs\pages\algorithms\symbiotic-organisms-search-cheng-prayogo-2014.md",
    "docs\Test-MultiVerseOptimizer.ps1",
    "docs\pages\algorithms\multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016.md",
    "docs\Test-EquilibriumOptimizer.ps1",
    "docs\pages\algorithms\equilibrium-optimizer-faramarzi-heidarinejad-stephens-mirjalili-2020.md",
    "docs\Test-InertiaWeightParticleSwarm.ps1",
    "docs\pages\algorithms\inertia-weight-particle-swarm-shi-eberhart-1998.md",
    "docs\Test-ConstrictionParticleSwarm.ps1",
    "docs\pages\algorithms\constriction-particle-swarm-clerc-kennedy-2002.md",
    "docs\Test-BareBonesParticleSwarm.ps1",
    "docs\pages\algorithms\bare-bones-particle-swarm-kennedy-2003.md",
    "docs\Test-FullyInformedParticleSwarm.ps1",
    "docs\pages\algorithms\fully-informed-particle-swarm-mendes-kennedy-neves-2004.md",
    "docs\Test-ComprehensiveLearningParticleSwarm.ps1",
    "docs\pages\algorithms\comprehensive-learning-particle-swarm-liang-qin-suganthan-baskar-2006.md",
    "docs\Test-CooperativeParticleSwarm.ps1",
    "docs\pages\algorithms\cooperative-particle-swarm-cpso-sk-van-den-bergh-engelbrecht-2004.md",
    "docs\Test-StandardParticleSwarm2007.ps1",
    "docs\pages\algorithms\standard-particle-swarm-bratton-kennedy-2007.md",
    "docs\Test-SpeciesBasedParticleSwarm.ps1",
    "docs\pages\algorithms\species-based-particle-swarm-parrott-li-2006.md",
    "docs\Test-NsgaII.ps1",
    "docs\pages\algorithms\nsga-ii-deb-pratap-agarwal-meyarivan-2002.md",
    "docs\Test-Paes.ps1",
    "docs\pages\algorithms\paes-knowles-corne-2000.md",
    "docs\Test-PesaII.ps1",
    "docs\pages\algorithms\pesa-ii-corne-jerram-knowles-oates-2001.md",
    "docs\Test-Ibea.ps1",
    "docs\pages\algorithms\ibea-zitzler-kunzli-2004.md",
    "docs\Test-Moead.ps1",
    "docs\pages\algorithms\moead-zhang-li-2007.md",
    "docs\Test-Mopso.ps1",
    "docs\pages\algorithms\mopso-coello-pulido-lechuga-2004.md",
    "docs\Test-Smpso.ps1",
    "docs\pages\algorithms\smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009.md",
    "docs\Test-NsgaIII.ps1",
    "docs\pages\algorithms\nsga-iii-deb-jain-2014.md",
    "docs\Test-SmsEmoa.ps1",
    "docs\pages\algorithms\sms-emoa-beume-naujoks-emmerich-2007.md",
    "docs\Test-Rvea.ps1",
    "docs\pages\algorithms\rvea-cheng-jin-olhofer-sendhoff-2016.md",
    "docs\Test-Spea.ps1",
    "docs\pages\algorithms\strength-pareto-evolutionary-algorithm-zitzler-thiele-1999.md",
    "docs\Test-Spea2.ps1",
    "docs\pages\algorithms\spea2-zitzler-laumanns-thiele-2001.md",
    "docs\Test-Nsga.ps1",
    "docs\pages\algorithms\nondominated-sorting-genetic-algorithm-srinivas-deb-1994.md",
    "docs\Test-Grea.ps1",
    "docs\pages\algorithms\grid-based-evolutionary-algorithm-yang-li-liu-zheng-2013.md",
    "docs\Test-MoCmaEs.ps1",
    "docs\pages\algorithms\multiobjective-cma-es-igel-hansen-roth-2007.md",
    "docs\Test-MoeadDe.ps1",
    "docs\pages\algorithms\moead-de-li-zhang-2009.md",
    "docs\Test-Hype.ps1",
    "docs\pages\algorithms\hype-bader-zitzler-2011.md",
    "docs\Test-TwoArch2.ps1",
    "docs\pages\algorithms\two-arch2-wang-jiao-yao-2015.md",
    "docs\Test-Moeadd.ps1",
    "docs\pages\algorithms\moeadd-li-deb-zhang-kwong-2015.md",
    "docs\Test-ThetaDea.ps1",
    "docs\pages\algorithms\theta-dea-yuan-xu-wang-yao-2016.md",
    "docs\Test-Knea.ps1",
    "docs\pages\algorithms\knea-zhang-tian-jin-2015.md",
    "docs\Test-Vaea.ps1",
    "docs\pages\algorithms\vaea-xiang-zhou-li-chen-2017.md",
    "docs\Test-DebConstraintGa.ps1",
    "docs\pages\algorithms\deb-feasibility-rules-ga-2000.md",
    "docs\Test-StochasticRankingEs.ps1",
    "docs\pages\algorithms\stochastic-ranking-es-runarsson-yao-2000.md",
    "docs\Test-DominanceTournamentGa.ps1",
    "docs\pages\algorithms\dominance-based-tournament-ga-coello-mezura-2002.md",
    "docs\Test-JoinesHouckPenaltyGa.ps1",
    "docs\pages\algorithms\nonstationary-penalty-ga-joines-houck-1994.md",
    "docs\Test-HomaifarPenaltyGa.ps1",
    "docs\pages\algorithms\homaifar-penalty-ga-1994.md",
    "docs\Test-AdaptivePenaltyGa.ps1",
    "docs\pages\algorithms\adaptive-penalty-ga-lemonge-barbosa-2004.md",
    "docs\Test-TessemaYenPenaltyGa.ps1",
    "docs\pages\algorithms\adaptive-penalty-formulation-ga-tessema-yen-2009.md",
    "docs\Test-EpsilonConstrainedDe.ps1",
    "docs\pages\algorithms\epsilon-constrained-de-takahama-sakai-iwane-2006.md",
    "docs\Test-DoxygenMarkupSafety.ps1",
    "docs\Test-ContinuousCrossEntropy.ps1",
    "docs\Test-LargeNeighborhoodSearch.ps1",
    "docs\Test-AdaptiveLargeNeighborhoodSearch.ps1",
    "docs\adaptive-large-neighborhood-search-component-catalog.json",
    "docs\Build-AdaptiveLargeNeighborhoodSearchDocumentation.ps1",
    "docs\pages\components\adaptive-large-neighborhood-search-components.md",
    "docs\pages\algorithms\adaptive-large-neighborhood-search-ropke-pisinger-2006.md",
    "docs\Test-AdvancedAdaptiveLargeNeighborhoodSearch.ps1",
    "docs\advanced-adaptive-large-neighborhood-search-catalog.json",
    "docs\Build-AdvancedAdaptiveLargeNeighborhoodSearchDocumentation.ps1",
    "docs\pages\components\advanced-adaptive-large-neighborhood-search-components.md",
    "docs\Test-ReadmeQuality.ps1",
    "docs\Test-ReadmeHistoricalCompatibility.ps1",
    "docs\large-neighborhood-search-component-catalog.json",
    "docs\Build-LargeNeighborhoodSearchDocumentation.ps1",
    "docs\pages\components\large-neighborhood-search-components.md",
    "docs\pages\algorithms\large-neighborhood-search-shaw-1998.md",
    "docs\pages\algorithms\artificial-bee-colony-karaboga-basturk-2007.md",
    "docs\pages\algorithms\firefly-algorithm-yang-2009.md",
    "docs\pages\algorithms\cross-entropy-continuous-kroese-porotsky-rubinstein-2006.md",
    "docs\Test-CmaEs.ps1",
    "docs\Test-AdvancedCmaEs.ps1",
    "docs\Test-RestartCmaEs.ps1",
    "docs\cma-es-component-catalog.json",
    "docs\Build-CmaEsDocumentation.ps1",
    "docs\pages\components\cma-es-components.md",
    "docs\pages\algorithms\cma-es-hansen-ostermeier-2001.md",
    "docs\pages\algorithms\active-cma-es-hansen-ros-2010.md",
    "docs\pages\algorithms\separable-cma-es-ros-hansen-2008.md",
    "docs\pages\algorithms\ipop-cma-es-auger-hansen-2005.md",
    "docs\pages\algorithms\bipop-cma-es-hansen-2009.md",
    "docs\advanced-ant-colony-optimization-catalog.json",
    "docs\Build-AdvancedAntColonyDocumentation.ps1",
    "docs\pages\components\advanced-ant-colony-optimization.md",
    "docs\pages\algorithms\ant-colony-system-dorigo-gambardella-1997.md",
    "docs\pages\algorithms\max-min-ant-system-stutzle-hoos-2000.md",
    "docs\ant-colony-optimization-catalog.json",
    "docs\pages\algorithms\ant-system-dorigo-maniezzo-colorni-1996.md",
    "docs\memetic-algorithm-catalog.json",
    "docs\Build-MemeticAlgorithmDocumentation.ps1",
    "docs\pages\components\memetic-algorithm-components.md",
    "docs\pages\algorithms\memetic-algorithm-moscato-1989.md",
    "docs\advanced-genetic-algorithm-catalog.json",
    "docs\Build-AdvancedGeneticAlgorithmDocumentation.ps1",
    "docs\pages\components\advanced-genetic-algorithm-operators.md",
    "docs\pages\algorithms\genetic-algorithm-generational.md",
    "docs\advanced-scatter-search-catalog.json",
    "docs\Build-AdvancedScatterSearchDocumentation.ps1",
    "docs\pages\components\advanced-scatter-search-strategies.md",
    "docs\pages\algorithms\scatter-search-marti-laguna-glover-2006.md",
    "docs\advanced-iterated-greedy-catalog.json",
    "docs\Build-AdvancedIteratedGreedyDocumentation.ps1",
    "docs\pages\components\advanced-iterated-greedy-strategies.md",
    "docs\pages\algorithms\iterated-greedy-ruiz-stutzle-2007.md",
    "docs\pages\algorithms\demon-based-acceptance-talbi-2009.md",
    "docs\pages\algorithms\tabu-search-glover.md",
    "docs\Test-TabuSearchFoundation.ps1",
    "docs\Test-TabuSearchAdvancedMemory.ps1",
    "docs\Build-TabuSearchAdvancedDocumentation.ps1",
    "docs\ts-memory-control-catalog.json",
    "docs\pages\components\tabu-search-memory-control-strategies.md",
    "docs\pages\algorithms\reactive-tabu-search-battiti-tecchiolli-1994.md",
    "docs\Test-LocalSearchFoundation.ps1",
    "docs\local-search-foundation-catalog.json",
    "docs\Test-RestartIteratedLocalSearch.ps1",
    "docs\restart-iterated-local-search-catalog.json",
    "docs\Test-VariableNeighborhoodSearch.ps1",
    "docs\variable-neighborhood-search-catalog.json",
    "docs\Test-GuidedLocalSearch.ps1",
    "docs\guided-local-search-catalog.json",
    "docs\Test-AdvancedVariableNeighborhoodSearch.ps1",
    "docs\advanced-variable-neighborhood-search-catalog.json",
    "docs\pages\components\advanced-variable-neighborhood-search-variants.md",
    "docs\pso-topology-catalog.json",
    "docs\pages\components\pso-communication-topologies.md",
    "docs\Build-PsoTopologyDocumentation.ps1",
    "docs\Build-AdvancedVariableNeighborhoodDocumentation.ps1",
    "docs\Test-ScientificComponentDocumentationParity.ps1",
    "docs\Test-ScientificFormulaQuality.ps1",
    "docs\Test-RenderedPortalQuality.ps1",
    "docs\Test-DoxygenDiagnosticQuality.ps1",
    "docs\Doxygen-CSharpCompatibilityFilter.ps1",
    "docs\Test-Grasp.ps1",
    "docs\Test-ReactiveGrasp.ps1",
    "docs\Test-GraspPathRelinking.ps1",
    "docs\path-relinking-strategy-catalog.json",
    "docs\Build-PathRelinkingStrategyDocumentation.ps1",
    "docs\pages\components\path-relinking-strategies.md",
    "docs\pages\algorithms\grasp-path-relinking.md",
    "docs\grasp-catalog.json",
    "docs\pages\algorithms\grasp-feo-resende-1995.md",
    "docs\pages\families\constructive-methods.md",
    "docs\doxygen-custom.css",
    "docs\assets\algorithms-icon.svg",
    "docs\assets\metaheuristicsplatform-logo.svg",
    "docs\assets\metaheuristicsplatform-favicon.svg",
    ".github\workflows\build.yml",
    ".github\workflows\documentation.yml",
    ".github\workflows\release.yml",
    "build\Build-Validated.ps1",
    "build\Build-All.ps1",
    "build\Prepare-ReleaseAssets.ps1",
    "build\Get-ReleaseNotes.ps1",
    "tools\Test-PowerShellSyntax.ps1",
    "tools\Test-Automation.ps1",
    "tools\Test-ReleaseWorkflowTopology.ps1",
    "tools\Configure-GitHubRepository.ps1",
    "tools\Get-BuildTarget.ps1"
)

foreach ($relative in $requiredRepoFiles) {
    if (-not (Test-Path (Join-Path $Root $relative))) {
        throw "Documentation parity: required ULSAlgorithms-parity file missing: '$relative'."
    }
}

$readme =
    [System.IO.File]::ReadAllText((Join-Path $Root "README.md"), [System.Text.Encoding]::UTF8)

foreach ($algorithm in $algorithms) {
    if (-not $readme.Contains($algorithm.id)) {
        throw "Documentation parity: README panel missing stable ID '$($algorithm.id)'."
    }
}


$idsSourcePath =
    Join-Path $Root "src\MetaheuristicsPlatform\Catalog\MetaheuristicAlgorithmIds.cs"

if (-not (Test-Path $idsSourcePath)) {
    throw "Documentation parity: MetaheuristicAlgorithmIds.cs is missing."
}

$idsSource =
    [System.IO.File]::ReadAllText($idsSourcePath, [System.Text.Encoding]::UTF8)

foreach ($algorithm in $algorithms) {
    if (-not $idsSource.Contains('"' + $algorithm.id + '"')) {
        throw "Documentation parity: stable ID '$($algorithm.id)' is not declared in MetaheuristicAlgorithmIds."
    }
}

$version =
    [System.IO.File]::ReadAllText((Join-Path $Root "version.json"), [System.Text.Encoding]::UTF8) |
    ConvertFrom-Json

if ([string]$version.version -ne "0.125.0") {
    throw "Documentation parity: version.json must be 0.125.0 for this release."
}

& (Join-Path $Root "docs\Test-TextEncoding.ps1") -Root $Root
& (Join-Path $Root "docs\Test-SimulatedAnnealingCoolingCatalog.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ThresholdAccepting.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DueckAcceptanceMethods.ps1") -Root $Root
& (Join-Path $Root "docs\Test-LateAcceptance.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DemonBasedAcceptance.ps1") -Root $Root
& (Join-Path $Root "docs\Test-IteratedGreedy.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedIteratedGreedy.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ScatterSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedScatterSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GeneticAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedGeneticAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-MemeticAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AntSystem.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedAntColony.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ArtificialBeeColony.ps1") -Root $Root
& (Join-Path $Root "docs\Test-FireflyAlgorithm.ps1") -Root $Root
& (Join-Path $Root "docs\Test-HarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ImprovedHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GlobalBestHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-SelfAdaptiveGlobalBestHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-NovelGlobalHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ParameterSettingFreeHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedParameterSettingFreeHarmonySearchIteration.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedParameterSettingFreeHarmonySearchObject.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DifferentialHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ExploratoryHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ImprovedHarmonySearchDifferentialMutation.ps1") -Root $Root
& (Join-Path $Root "docs\Test-NovelSelfAdaptiveHarmonySearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdaptiveHarmonySearchDifferentialEvolution.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DoxygenMarkupSafety.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ContinuousCrossEntropy.ps1") -Root $Root
& (Join-Path $Root "docs\Test-LargeNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdaptiveLargeNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedAdaptiveLargeNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "tools\Test-ReleaseWorkflowTopology.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ReadmeQuality.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ReadmeHistoricalCompatibility.ps1") -Root $Root
& (Join-Path $Root "docs\Test-CmaEs.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedCmaEs.ps1") -Root $Root
& (Join-Path $Root "docs\Test-RestartCmaEs.ps1") -Root $Root
& (Join-Path $Root "docs\Test-TabuSearchFoundation.ps1") -Root $Root
& (Join-Path $Root "docs\Test-TabuSearchAdvancedMemory.ps1") -Root $Root
& (Join-Path $Root "docs\Test-LocalSearchFoundation.ps1") -Root $Root
& (Join-Path $Root "docs\Test-RestartIteratedLocalSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-VariableNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GuidedLocalSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-AdvancedVariableNeighborhoodSearch.ps1") -Root $Root
& (Join-Path $Root "docs\Test-Grasp.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ReactiveGrasp.ps1") -Root $Root
& (Join-Path $Root "docs\Test-GraspPathRelinking.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ScientificComponentDocumentationParity.ps1") -Root $Root
& (Join-Path $Root "docs\Test-ScientificFormulaQuality.ps1") -Root $Root
& (Join-Path $Root "docs\Test-DoxygenDiagnosticQuality.ps1") -Root $Root

Write-Host "Documentation parity validation passed: $($algorithms.Count) algorithms, $($families.Count) family pages." -ForegroundColor Green

# xUnit analyzer safety: xUnit2031
# Assert.Single(collection.Where(predicate)) is rejected by xUnit analyzers.
# Use Assert.Single(collection, predicate) so violations fail during documentation
# parity before the more expensive build stage.
$testRoot =
    Join-Path $Root "tests"

if (Test-Path -LiteralPath $testRoot -PathType Container) {
    $xunit2031Pattern =
        '(?s)Assert\.Single\s*\(\s*[^;]{0,1200}?\.Where\s*\('

    $xunit2031Violations =
        @(
            Get-ChildItem `
                -LiteralPath $testRoot `
                -Recurse `
                -File `
                -Filter "*.cs" |
            Where-Object {
                $source =
                    [System.IO.File]::ReadAllText(
                        $_.FullName,
                        [System.Text.Encoding]::UTF8)

                [regex]::IsMatch(
                    $source,
                    $xunit2031Pattern)
            }
        )

    if ($xunit2031Violations.Count -gt 0) {
        throw (
            "Documentation parity: xUnit2031 safety violation(s): {0}. " +
            "Replace Assert.Single(collection.Where(predicate)) with " +
            "Assert.Single(collection, predicate)." -f
            (($xunit2031Violations |
                ForEach-Object { $_.FullName }) -join "; "))
    }
}