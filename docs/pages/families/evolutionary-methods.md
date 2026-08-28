@page family_evolutionary_methods Evolutionary methods

# Evolutionary methods

Population methods based on structured variation or combination, selection, reference-set learning and adaptive control.

## Methods

- **[Differential Evolution](../algorithms/differential-evolution.md)** — `differential-evolution` — O(ND) per generation for classical mutation/crossover, plus objective-evaluation cost
- **[jDE — Self-Adaptive Differential Evolution](../algorithms/jde-brest-2006.md)** — `jde-brest-2006` — O(ND) per generation plus objective-evaluation cost
- **[JADE](../algorithms/jade-2009.md)** — `jade-2009` — O(ND + N log N) per generation plus objective-evaluation cost
- **[SHADE](../algorithms/shade-2013.md)** — `shade-2013` — O(ND + N log N) per generation plus objective-evaluation cost
- **[L-SHADE](../algorithms/lshade-2014.md)** — `lshade-2014` — O(N_kD + N_k log N_k) at generation k plus objective-evaluation cost
- **[Generational Genetic Algorithm](../algorithms/genetic-algorithm-generational.md)** — `genetic-algorithm-generational` — fixed-size generational selection/crossover/mutation with optional elitism — @subpage advanced_genetic_algorithm_operators
- **[Scatter Search](../algorithms/scatter-search-marti-laguna-glover-2006.md)** — `scatter-search-marti-laguna-glover-2006` — five-method RefSet search with advanced dynamic/tiered update, rebuilding and representative subset components
- **[Cross-Entropy Method - Continuous Optimization](../algorithms/cross-entropy-continuous-kroese-porotsky-rubinstein-2006.md)** — `cross-entropy-continuous-kroese-porotsky-rubinstein-2006` — diagonal normal elite-distribution learning with dynamic sigma smoothing.

- **[Covariance Matrix Adaptation Evolution Strategy](../algorithms/cma-es-hansen-ostermeier-2001.md)** — cma-es-hansen-ostermeier-2001 — canonical full-covariance adaptation with CSA and rank-one/rank-mu updates.
- **[Active CMA-ES](../algorithms/active-cma-es-hansen-ros-2010.md)** — `active-cma-es-hansen-ros-2010` — weighted negative covariance adaptation using unsuccessful ranked directions.
- **[Separable CMA-ES](../algorithms/separable-cma-es-ros-hansen-2008.md)** — `separable-cma-es-ros-hansen-2008` — diagonal covariance adaptation with linear internal time and memory.
- **[IPOP-CMA-ES](../algorithms/ipop-cma-es-auger-hansen-2005.md)** — `ipop-cma-es-auger-hansen-2005` — geometrically increasing population across CMA-ES restarts.
- **[BIPOP-CMA-ES](../algorithms/bipop-cma-es-hansen-2009.md)** — `bipop-cma-es-hansen-2009` — evaluation-budget-balanced large/small population restart portfolio.
- **[Biogeography-Based Optimization](../algorithms/biogeography-based-optimization-simon-2008.md)** — `biogeography-based-optimization-simon-2008` — Bounded continuous derivative-free optimization using rank-based immigration, emigration, mutation and elitism.
- **[NSGA-II](../algorithms/nsga-ii-deb-pratap-agarwal-meyarivan-2002.md)** - `nsga-ii-deb-pratap-agarwal-meyarivan-2002` - Fast nondominated sorting, elitist parent-offspring survival and crowding-distance diversity.
- **[Pareto Archived Evolution Strategy](../algorithms/paes-knowles-corne-2000.md)** - `paes-knowles-corne-2000` - Canonical (1+1)-PAES local mutation with Pareto archive and adaptive objective-space grid.
- **[PESA-II](../algorithms/pesa-ii-corne-jerram-knowles-oates-2001.md)** - `pesa-ii-corne-jerram-knowles-oates-2001` - Region-based selection from an external nondominated archive using adaptive hyperbox density.
- **[Indicator-Based Evolutionary Algorithm](../algorithms/ibea-zitzler-kunzli-2004.md)** - `ibea-zitzler-kunzli-2004` - Binary additive-epsilon indicator fitness directly drives environmental selection.
- **[MOEA/D](../algorithms/moead-zhang-li-2007.md)** - `moead-zhang-li-2007` - Tchebycheff decomposition into neighboring scalar subproblems with differential reproduction and ideal-point updates.
- **[NSGA-III](../algorithms/nsga-iii-deb-jain-2014.md)** - `nsga-iii-deb-jain-2014` - NSGA-II framework with normalized objective vectors, Das-Dennis reference directions and reference niching.
- **[SMS-EMOA](../algorithms/sms-emoa-beume-naujoks-emmerich-2007.md)** - `sms-emoa-beume-naujoks-emmerich-2007` - Steady-state environmental selection removes the minimum dominated-hypervolume contributor from the worst front.
- **[RVEA](../algorithms/rvea-cheng-jin-olhofer-sendhoff-2016.md)** - `rvea-cheng-jin-olhofer-sendhoff-2016` - Reference-vector guided many-objective selection using angle-penalized distance normalized by nearest reference-vector angle and periodic vector adaptation.
- **[Strength Pareto Evolutionary Algorithm](../algorithms/strength-pareto-evolutionary-algorithm-zitzler-thiele-1999.md)** - `strength-pareto-evolutionary-algorithm-zitzler-thiele-1999` - Original SPEA with an external nondominated set, strength fitness assignment and archive clustering/truncation.
- **[SPEA2](../algorithms/spea2-zitzler-laumanns-thiele-2001.md)** - `spea2-zitzler-laumanns-thiele-2001` - SPEA2 with fine-grained raw strength fitness, kth-neighbor density estimation and nearest-neighbor archive truncation.
- **[Nondominated Sorting Genetic Algorithm](../algorithms/nondominated-sorting-genetic-algorithm-srinivas-deb-1994.md)** - `nondominated-sorting-genetic-algorithm-srinivas-deb-1994` - Original non-elitist NSGA using nondominated ranks and objective-space fitness sharing within fronts.
- **[Grid-Based Evolutionary Algorithm](../algorithms/grid-based-evolutionary-algorithm-yang-li-liu-zheng-2013.md)** - `grid-based-evolutionary-algorithm-yang-li-liu-zheng-2013` - GrEA uses normalized objective-space grids to couple convergence pressure with grid density and distribution.
- **[Multi-objective CMA-ES](../algorithms/multiobjective-cma-es-igel-hansen-roth-2007.md)** - `multiobjective-cma-es-igel-hansen-roth-2007` - MO-CMA-ES maintains individual covariance/step-size strategy states and applies nondominated multiobjective selection.
- **[MOEA/D-DE](../algorithms/moead-de-li-zhang-2009.md)** - `moead-de-li-zhang-2009` - MOEA/D-DE combines Tchebycheff decomposition, neighborhood mating/update and differential-evolution reproduction.
- **[HypE](../algorithms/hype-bader-zitzler-2011.md)** - `hype-bader-zitzler-2011` - HypE drives mating and environmental selection with Monte-Carlo estimates of hypervolume contribution.
- **[Two_Arch2](../algorithms/two-arch2-wang-jiao-yao-2015.md)** - `two-arch2-wang-jiao-yao-2015` - Two_Arch2 separates convergence and diversity responsibilities into two cooperating archives with distinct selection principles.
- **[MOEA/DD](../algorithms/moeadd-li-deb-zhang-kwong-2015.md)** - `moeadd-li-deb-zhang-kwong-2015` - MOEA/DD unifies Pareto dominance and decomposition through reference subregions and decomposition values.
- **[Theta-Dominance Evolutionary Algorithm](../algorithms/theta-dea-yuan-xu-wang-yao-2016.md)** - `theta-dea-yuan-xu-wang-yao-2016` - Theta-DEA clusters normalized objective vectors by reference directions and ranks solutions with theta-dominance/PBI pressure.
- **[Knee Point Driven Evolutionary Algorithm](../algorithms/knea-zhang-tian-jin-2015.md)** - `knea-zhang-tian-jin-2015` - KnEA detects locally preferred knee candidates and combines knee pressure with nondominated environmental selection.
- **[Vector Angle-Based Evolutionary Algorithm](../algorithms/vaea-xiang-zhou-li-chen-2017.md)** - `vaea-xiang-zhou-li-chen-2017` - VaEA uses normalized objective-vector angles for diversity and convergence-aware elimination without predefined reference vectors.
- **[Deb Feasibility Rules Genetic Algorithm](../algorithms/deb-feasibility-rules-ga-2000.md)** - `deb-feasibility-rules-ga-2000` - Feasibility-first Deb rules compare feasible candidates by objective, prefer feasible over infeasible, and compare infeasible candidates by aggregate constraint violation.
- **[Stochastic Ranking Evolution Strategy](../algorithms/stochastic-ranking-es-runarsson-yao-2000.md)** - `stochastic-ranking-es-runarsson-yao-2000` - Stochastic ranking repeatedly orders an evolution-strategy population using objective comparison with probability P_f and violation comparison otherwise, avoiding a fixed penalty coefficient.
- **[Dominance-Based Tournament Genetic Algorithm](../algorithms/dominance-based-tournament-ga-coello-mezura-2002.md)** - `dominance-based-tournament-ga-coello-mezura-2002` - Penalty-free tournament selection compares objective quality and aggregate constraint violation through a dominance relation, preserving nondominated tradeoffs among infeasible candidates.
- **[Joines-Houck Nonstationary Penalty Genetic Algorithm](../algorithms/nonstationary-penalty-ga-joines-houck-1994.md)** - `nonstationary-penalty-ga-joines-houck-1994` - Generation-dependent nonstationary penalty pressure follows the Joines-Houck mechanism with explicit C, alpha and beta controls.
- **[Homaifar-Qi-Lai Penalty Genetic Algorithm](../algorithms/homaifar-penalty-ga-1994.md)** - `homaifar-penalty-ga-1994` - Static multilevel penalty uses user-defined violation levels and a distinct penalty coefficient for each constraint/level pair; the active violation penalty is quadratic.
- **[Lemonge-Barbosa Adaptive Penalty Genetic Algorithm](../algorithms/adaptive-penalty-ga-lemonge-barbosa-2004.md)** - `adaptive-penalty-ga-lemonge-barbosa-2004` - Parameter-less constraint-specific penalty coefficients are recomputed from population-average objective and violation information.
- **[Tessema-Yen Adaptive Penalty Genetic Algorithm](../algorithms/adaptive-penalty-formulation-ga-tessema-yen-2009.md)** - `adaptive-penalty-formulation-ga-tessema-yen-2009` - Normalized objective and violation distance is combined with a feasible-ratio-driven adaptive penalty, retaining useful infeasible candidates without a user-tuned penalty coefficient.
- **[Epsilon-Constrained Differential Evolution](../algorithms/epsilon-constrained-de-takahama-sakai-iwane-2006.md)** - `epsilon-constrained-de-takahama-sakai-iwane-2006` - DE/rand/1/bin uses an epsilon-level ordering whose admissible violation threshold decreases to zero over a controlled number of generations.
- **[GENOCOP III](../algorithms/genocop-iii-michalewicz-nazhiyath-1995.md)** - `genocop-iii-michalewicz-nazhiyath-1995` - Search and feasible reference populations co-evolve; infeasible search points are repaired toward feasible reference points by segment bisection before evaluation.
- **[Homomorphous-Mapping Evolutionary Algorithm](../algorithms/homomorphous-mapping-ea-koziel-michalewicz-1999.md)** - `homomorphous-mapping-ea-koziel-michalewicz-1999` - A feasible reference point anchors a decoder that maps search points into the feasible region by radial segment projection and bisection.
- **[Ensemble of Constraint Handling Techniques](../algorithms/ensemble-constraint-handling-mallipeddi-suganthan-2010.md)** - `ensemble-constraint-handling-mallipeddi-suganthan-2010` - Multiple subpopulations apply distinct constraint-handling techniques and periodically exchange elites, preserving the ensemble principle.

## Navigation

Return to @ref method_families "method families".
