namespace MetaheuristicsPlatform.Catalog;

/// <summary>
/// Stable public identifiers used by the repository catalog and factory.
/// </summary>
public static class MetaheuristicAlgorithmIds
{
    public const string ParticleSwarm = "particle-swarm";
    public const string ArtificialBeeColony =
        "artificial-bee-colony-karaboga-basturk-2007";
    public const string Firefly =
        "firefly-algorithm-yang-2009";
    public const string HarmonySearch =
        "harmony-search-geem-kim-loganathan-2001";
    public const string ImprovedHarmonySearch =
        "improved-harmony-search-mahdavi-fesanghary-damangir-2007";
    public const string GlobalBestHarmonySearch =
        "global-best-harmony-search-omran-mahdavi-2008";
    public const string SelfAdaptiveGlobalBestHarmonySearch =
        "self-adaptive-global-best-harmony-search-pan-suganthan-tasgetiren-liang-2010";
    public const string NovelGlobalHarmonySearch =
        "novel-global-harmony-search-zou-gao-wu-li-2010";
    public const string ParameterSettingFreeHarmonySearch =
        "parameter-setting-free-harmony-search-geem-sim-2010";
    public const string AdvancedParameterSettingFreeHarmonySearchIteration =
        "advanced-parameter-setting-free-harmony-search-iteration-jeong-park-geem-sim-2020";
    public const string AdvancedParameterSettingFreeHarmonySearchObject =
        "advanced-parameter-setting-free-harmony-search-object-jeong-park-geem-sim-2020";
    public const string DifferentialHarmonySearch =
        "differential-harmony-search-chakraborty-roy-das-jain-abraham-2009";
    public const string ExploratoryHarmonySearch =
        "exploratory-harmony-search-das-mukhopadhyay-roy-abraham-panigrahi-2011";
    public const string ImprovedHarmonySearchDifferentialMutation =
        "improved-harmony-search-differential-mutation-yong-liu-zhang-feng-2012";
    public const string NovelSelfAdaptiveHarmonySearch =
        "novel-self-adaptive-harmony-search-luo-2013";
    public const string AdaptiveHarmonySearchDifferentialEvolution =
        "adaptive-harmony-search-differential-evolution-zhao-li-hao-liu-yuan-2020";
    public const string AntSystem =
        "ant-system-dorigo-maniezzo-colorni-1996";
    public const string AntColonySystem =
        "ant-colony-system-dorigo-gambardella-1997";
    public const string MaxMinAntSystem =
        "max-min-ant-system-stutzle-hoos-2000";
    public const string CmaEs =
        "cma-es-hansen-ostermeier-2001";
    public const string ActiveCmaEs =
        "active-cma-es-hansen-ros-2010";
    public const string SeparableCmaEs =
        "separable-cma-es-ros-hansen-2008";
    public const string IpopCmaEs =
        "ipop-cma-es-auger-hansen-2005";
    public const string BipopCmaEs =
        "bipop-cma-es-hansen-2009";
    public const string ContinuousCrossEntropy =
        "cross-entropy-continuous-kroese-porotsky-rubinstein-2006";
    public const string DifferentialEvolution = "differential-evolution";
    public const string Jde = "jde-brest-2006";
    public const string Jade = "jade-2009";
    public const string Shade = "shade-2013";
    public const string LShade = "lshade-2014";
    public const string GeneticAlgorithm =
        "genetic-algorithm-generational";
    public const string MemeticAlgorithm =
        "memetic-algorithm-moscato-1989";
    public const string ScatterSearch =
        "scatter-search-marti-laguna-glover-2006";
    public const string SimulatedAnnealing = "simulated-annealing-metropolis";
    public const string ThresholdAccepting =
        "threshold-accepting-dueck-scheuer-1990";
    public const string GreatDeluge =
        "great-deluge-dueck-1993";
    public const string RecordToRecordTravel =
        "record-to-record-travel-dueck-1993";
    public const string LateAcceptanceHillClimbing =
        "late-acceptance-hill-climbing-burke-bykov-2017";
    public const string DemonBasedAcceptance =
        "demon-based-acceptance-talbi-2009";
    public const string TabuSearch = "tabu-search-glover";
    public const string ReactiveTabuSearch =
        "reactive-tabu-search-battiti-tecchiolli-1994";
    public const string LocalSearchBestImprovement = "local-search-best-improvement";
    public const string LocalSearchFirstImprovement = "local-search-first-improvement";
    public const string MultiStartLocalSearch = "multi-start-local-search";
    public const string IteratedLocalSearch = "iterated-local-search-lourenco-martin-stutzle";
    public const string IteratedGreedy = "iterated-greedy-ruiz-stutzle-2007";
    public const string LargeNeighborhoodSearch =
        "large-neighborhood-search-shaw-1998";
    public const string AdaptiveLargeNeighborhoodSearch =
        "adaptive-large-neighborhood-search-ropke-pisinger-2006";
    public const string VariableNeighborhoodDescent = "variable-neighborhood-descent";
    public const string VariableNeighborhoodSearch = "variable-neighborhood-search-mladenovic-hansen";
    public const string GuidedLocalSearch = "guided-local-search-voudouris-tsang-1999";
    public const string ReducedVariableNeighborhoodSearch = "reduced-variable-neighborhood-search";
    public const string GeneralVariableNeighborhoodSearch = "general-variable-neighborhood-search";
    public const string SkewedVariableNeighborhoodSearch = "skewed-variable-neighborhood-search-hansen-mladenovic-2001";
    public const string Grasp = "grasp-feo-resende-1995";
    public const string ReactiveGrasp = "reactive-grasp-prais-ribeiro-2000";
    public const string GraspPathRelinking = "grasp-path-relinking";

    public const string BiogeographyBasedOptimization =
        "biogeography-based-optimization-simon-2008";

    public const string CuckooSearch =
        "cuckoo-search-yang-deb-2009";

    public const string BatAlgorithm =
        "bat-algorithm-yang-2010";

    public const string FlowerPollinationAlgorithm =
        "flower-pollination-algorithm-yang-2012";

    public const string GreyWolfOptimizer =
        "grey-wolf-optimizer-mirjalili-mirjalili-lewis-2014";

    public const string MothFlameOptimization =
        "moth-flame-optimization-mirjalili-2015";

    public const string WhaleOptimizationAlgorithm =
        "whale-optimization-algorithm-mirjalili-lewis-2016";

    public const string SineCosineAlgorithm =
        "sine-cosine-algorithm-mirjalili-2016";

    public const string SalpSwarmAlgorithm =
        "salp-swarm-algorithm-mirjalili-gandomi-mirjalili-saremi-faris-mirjalili-2017";

    public const string HarrisHawksOptimization =
        "harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019";

    public const string BigBangBigCrunch =
        "big-bang-big-crunch-erol-eksin-2006";

    public const string GravitationalSearch =
        "gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009";

    public const string TeachingLearningBasedOptimization =
        "teaching-learning-based-optimization-rao-savsani-vakharia-2011";

    public const string CrowSearch =
        "crow-search-algorithm-askarzadeh-2016";

    public const string Jaya =
        "jaya-algorithm-rao-2016";

    public const string ImperialistCompetitiveAlgorithm =
        "imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007";

    public const string BlackHoleAlgorithm =
        "black-hole-algorithm-hatamlou-2013";

    public const string SymbioticOrganismsSearch =
        "symbiotic-organisms-search-cheng-prayogo-2014";

    public const string MultiVerseOptimizer =
        "multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016";

    public const string EquilibriumOptimizer =
        "equilibrium-optimizer-faramarzi-heidarinejad-stephens-mirjalili-2020";

    public const string InertiaWeightParticleSwarm =
        "inertia-weight-particle-swarm-shi-eberhart-1998";

    public const string ConstrictionParticleSwarm =
        "constriction-particle-swarm-clerc-kennedy-2002";

    public const string BareBonesParticleSwarm =
        "bare-bones-particle-swarm-kennedy-2003";

    public const string FullyInformedParticleSwarm =
        "fully-informed-particle-swarm-mendes-kennedy-neves-2004";

    public const string ComprehensiveLearningParticleSwarm =
        "comprehensive-learning-particle-swarm-liang-qin-suganthan-baskar-2006";

    public const string CooperativeParticleSwarm =
        "cooperative-particle-swarm-cpso-sk-van-den-bergh-engelbrecht-2004";

    public const string StandardParticleSwarm2007 =
        "standard-particle-swarm-bratton-kennedy-2007";

    public const string SpeciesBasedParticleSwarm =
        "species-based-particle-swarm-parrott-li-2006";

    public const string NsgaII =
        "nsga-ii-deb-pratap-agarwal-meyarivan-2002";

    public const string Paes =
        "paes-knowles-corne-2000";

    public const string PesaII =
        "pesa-ii-corne-jerram-knowles-oates-2001";

    public const string Ibea =
        "ibea-zitzler-kunzli-2004";

    public const string Moead =
        "moead-zhang-li-2007";

    public const string Mopso =
        "mopso-coello-pulido-lechuga-2004";

    public const string Smpso =
        "smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009";

    public const string NsgaIII =
        "nsga-iii-deb-jain-2014";

    public const string SmsEmoa =
        "sms-emoa-beume-naujoks-emmerich-2007";

    public const string Rvea =
        "rvea-cheng-jin-olhofer-sendhoff-2016";

    public const string Spea =
        "strength-pareto-evolutionary-algorithm-zitzler-thiele-1999";

    public const string Spea2 =
        "spea2-zitzler-laumanns-thiele-2001";

    public const string Nsga =
        "nondominated-sorting-genetic-algorithm-srinivas-deb-1994";

    public const string Grea =
        "grid-based-evolutionary-algorithm-yang-li-liu-zheng-2013";

    public const string MoCmaEs =
        "multiobjective-cma-es-igel-hansen-roth-2007";

    public const string MoeadDe =
        "moead-de-li-zhang-2009";

    public const string Hype =
        "hype-bader-zitzler-2011";

    public const string TwoArch2 =
        "two-arch2-wang-jiao-yao-2015";

    public const string Moeadd =
        "moeadd-li-deb-zhang-kwong-2015";

    public const string ThetaDea =
        "theta-dea-yuan-xu-wang-yao-2016";

    public const string Knea =
        "knea-zhang-tian-jin-2015";

    public const string Vaea =
        "vaea-xiang-zhou-li-chen-2017";
}
