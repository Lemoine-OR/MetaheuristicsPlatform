@page family_swarm_intelligence Swarm intelligence

# Swarm intelligence

Population and colony methods driven by distributed social information, collective motion,
probabilistic recruitment or shared pheromone memory.

## Methods

- **[Particle Swarm Optimization](../algorithms/particle-swarm.md)** — `particle-swarm` — continuous collective motion with configurable communication topology.
- **[Artificial Bee Colony](../algorithms/artificial-bee-colony-karaboga-basturk-2007.md)** — `artificial-bee-colony-karaboga-basturk-2007` — employed/onlooker/scout food-source search for bounded continuous optimization.
- **[Firefly Algorithm](../algorithms/firefly-algorithm-yang-2009.md)** — `firefly-algorithm-yang-2009` — distance-decaying pairwise attraction with additive stochastic exploration for bounded continuous optimization.
- **[Ant System - Dorigo-Maniezzo-Colorni](../algorithms/ant-system-dorigo-maniezzo-colorni-1996.md)** — `ant-system-dorigo-maniezzo-colorni-1996` — generic pheromone-guided constructive colony search.
- **[Ant Colony System - Dorigo-Gambardella](../algorithms/ant-colony-system-dorigo-gambardella-1997.md)** — `ant-colony-system-dorigo-gambardella-1997` — pseudo-random proportional construction with local and best-so-far pheromone updates.
- **[MAX-MIN Ant System - Stutzle-Hoos](../algorithms/max-min-ant-system-stutzle-hoos-2000.md)** — `max-min-ant-system-stutzle-hoos-2000` — bounded pheromone memory with selective best reinforcement.
- **[Cuckoo Search via Levy Flights](../algorithms/cuckoo-search-yang-deb-2009.md)** — `cuckoo-search-yang-deb-2009` — Bounded continuous derivative-free optimization with Levy-flight exploration and nest abandonment.
- **[Bat Algorithm](../algorithms/bat-algorithm-yang-2010.md)** — `bat-algorithm-yang-2010` — Bounded continuous derivative-free optimization with frequency-tuned motion, pulse-rate local search and loudness acceptance.
- **[Flower Pollination Algorithm](../algorithms/flower-pollination-algorithm-yang-2012.md)** — `flower-pollination-algorithm-yang-2012` — Bounded continuous derivative-free optimization mixing global Levy pollination and local flower constancy.
- **[Grey Wolf Optimizer](../algorithms/grey-wolf-optimizer-mirjalili-mirjalili-lewis-2014.md)** — `grey-wolf-optimizer-mirjalili-mirjalili-lewis-2014` — Bounded continuous derivative-free optimization using alpha/beta/delta leadership and encircling dynamics.
- **[Moth-Flame Optimization](../algorithms/moth-flame-optimization-mirjalili-2015.md)** — `moth-flame-optimization-mirjalili-2015` — Bounded continuous derivative-free optimization with logarithmic moth-to-flame spirals and a linearly decreasing flame count.
- **[Whale Optimization Algorithm](../algorithms/whale-optimization-algorithm-mirjalili-lewis-2016.md)** — `whale-optimization-algorithm-mirjalili-lewis-2016` — Bounded continuous derivative-free optimization with encircling, random-prey exploration and logarithmic bubble-net spirals.
- **[Sine Cosine Algorithm](../algorithms/sine-cosine-algorithm-mirjalili-2016.md)** — `sine-cosine-algorithm-mirjalili-2016` — Bounded continuous derivative-free optimization with sine/cosine oscillation around the best destination.
- **[Salp Swarm Algorithm](../algorithms/salp-swarm-algorithm-mirjalili-gandomi-mirjalili-saremi-faris-mirjalili-2017.md)** — `salp-swarm-algorithm-mirjalili-gandomi-mirjalili-saremi-faris-mirjalili-2017` — Bounded continuous derivative-free optimization with food-directed leaders and chain-following salps.
- **[Harris Hawks Optimization](../algorithms/harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019.md)** — `harris-hawks-optimization-heidari-mirjalili-faris-aljarah-mafarja-chen-2019` — Bounded continuous derivative-free optimization using surprise-pounce exploration, besiege modes and Levy rapid dives.
- **[Gravitational Search Algorithm](../algorithms/gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009.md)** — `gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009` — Bounded continuous derivative-free optimization using fitness-dependent masses, decaying gravitational attraction and velocity updates.
- **[Crow Search Algorithm](../algorithms/crow-search-algorithm-askarzadeh-2016.md)** — `crow-search-algorithm-askarzadeh-2016` — Bounded continuous derivative-free optimization using personal hiding-place memory, crow following, flight length and awareness-controlled random relocation.
- **[Symbiotic Organisms Search](../algorithms/symbiotic-organisms-search-cheng-prayogo-2014.md)** — `symbiotic-organisms-search-cheng-prayogo-2014` — Bounded continuous derivative-free optimization with parameter-free mutualism, commensalism and parasitism phases.
- @subpage inertia_weight_particle_swarm_shi_eberhart_1998 — Inertia Weight Particle Swarm Optimization (`inertia-weight-particle-swarm-shi-eberhart-1998`).
- @subpage constriction_particle_swarm_clerc_kennedy_2002 — Clerc-Kennedy Constriction Particle Swarm (`constriction-particle-swarm-clerc-kennedy-2002`).
- @subpage bare_bones_particle_swarm_kennedy_2003 — Bare Bones Particle Swarm (`bare-bones-particle-swarm-kennedy-2003`).
- @subpage fully_informed_particle_swarm_mendes_kennedy_neves_2004 — Fully Informed Particle Swarm (`fully-informed-particle-swarm-mendes-kennedy-neves-2004`).
- @subpage comprehensive_learning_particle_swarm_liang_qin_suganthan_baskar_2006 — Comprehensive Learning Particle Swarm Optimizer (`comprehensive-learning-particle-swarm-liang-qin-suganthan-baskar-2006`).
- @subpage cooperative_particle_swarm_cpso_sk_van_den_bergh_engelbrecht_2004 — Cooperative Particle Swarm Optimization (CPSO-SK) (`cooperative-particle-swarm-cpso-sk-van-den-bergh-engelbrecht-2004`).
- @subpage standard_particle_swarm_bratton_kennedy_2007 — Standard Particle Swarm Optimization 2007 (`standard-particle-swarm-bratton-kennedy-2007`).
- @subpage species_based_particle_swarm_parrott_li_2006 — Species-Based Particle Swarm Optimization (`species-based-particle-swarm-parrott-li-2006`).
- **[Multiobjective Particle Swarm Optimizer](../algorithms/mopso-coello-pulido-lechuga-2004.md)** - `mopso-coello-pulido-lechuga-2004` - Pareto-repository PSO with adaptive hypercubes, inverse-density leader selection, pbest dominance and decaying mutation.
- **[SMPSO](../algorithms/smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009.md)** - `smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009` - Speed-constrained MOPSO with constriction, componentwise velocity bounds, polynomial turbulence and external archive.
- **[Species-Based Particle Swarm Optimization](../algorithms/species-based-pso-li-2004.md)** - `species-based-pso-li-2004` - Particles are grouped into species around dominant seeds; each particle uses its species seed as its neighborhood best.
- **[Vector-Niche Particle Swarm Optimization](../algorithms/vector-niche-pso-schoeman-engelbrecht-2004.md)** - `vector-niche-pso-schoeman-engelbrecht-2004` - Vector relationships between particles and candidate niche leaders are used to demarcate niches and maintain independent subswarms.

## Navigation

Return to @ref method_families "method families".
