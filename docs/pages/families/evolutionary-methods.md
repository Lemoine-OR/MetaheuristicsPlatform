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

- **[Covariance Matrix Adaptation Evolution Strategy](../algorithms/cma-es-hansen-ostermeier-2001.md)** — cma-es-hansen-ostermeier-2001 — canonical full-covariance adaptation with CSA and rank-one/rank-mu updates.
- **[Active CMA-ES](../algorithms/active-cma-es-hansen-ros-2010.md)** — `active-cma-es-hansen-ros-2010` — weighted negative covariance adaptation using unsuccessful ranked directions.
- **[Separable CMA-ES](../algorithms/separable-cma-es-ros-hansen-2008.md)** — `separable-cma-es-ros-hansen-2008` — diagonal covariance adaptation with linear internal time and memory.
- **[IPOP-CMA-ES](../algorithms/ipop-cma-es-auger-hansen-2005.md)** — `ipop-cma-es-auger-hansen-2005` — geometrically increasing population across CMA-ES restarts.
- **[BIPOP-CMA-ES](../algorithms/bipop-cma-es-hansen-2009.md)** — `bipop-cma-es-hansen-2009` — evaluation-budget-balanced large/small population restart portfolio.

## Navigation

Return to @ref method_families "method families".
