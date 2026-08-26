@page family_other_metaheuristics Other / music-inspired methods

# Other / music-inspired methods

Population-based or memory-driven metaheuristics whose scientific identity is not accurately
represented as swarm intelligence, evolutionary search, trajectory search, constructive
search or hybrid/memetic composition.

## Methods

- @subpage harmony_search_geem_kim_loganathan_2001 - canonical fixed-parameter Harmony Search foundation.
- @subpage improved_harmony_search_mahdavi_fesanghary_damangir_2007 - Improved Harmony Search with deterministic PAR/bw schedules.
- @subpage global_best_harmony_search_omran_mahdavi_2008 - Global-best Harmony Search with bandwidth-free global-best pitch adjustment.
- @subpage self_adaptive_global_best_harmony_search_pan_suganthan_tasgetiren_liang_2010 - Self-Adaptive Global-best Harmony Search with successful-parameter learning and piecewise bandwidth.
- @subpage novel_global_harmony_search_zou_gao_wu_li_2010 - Novel Global Harmony Search with reflected-best position updating, genetic mutation and unconditional replacement.
- @subpage parameter_setting_free_harmony_search_geem_sim_2010 - Parameter-Setting-Free Harmony Search with Operation Type Matrix adaptation.
- @subpage advanced_parameter_setting_free_harmony_search_iteration_jeong_park_geem_sim_2020 - Advanced PSF-HS iteration scheme with sigmoid HMCR/PAR and no OTM.
- @subpage advanced_parameter_setting_free_harmony_search_object_jeong_park_geem_sim_2020 - Advanced PSF-HS object scheme with target-dependent HMCR/PAR and Equation (9) bandwidth.

## Classification note

Harmony Search, Improved Harmony Search, Global-best Harmony Search, Self-Adaptive
Global-best Harmony Search, Novel Global Harmony Search, Parameter-Setting-Free Harmony
Search and the two Advanced Parameter-Setting-Free Harmony Search schemes are represented
by `MetaheuristicFamily.Other` and `MetaheuristicSolutionModel.Population`. Conventional
PSF-HS learns probabilities from OTM; iteration APSF-HS uses iteration/dimension formulas;
object APSF-HS uses a target-dependent HM mean and object-only adaptive bandwidth. Stable IDs
keep all eight scientific identities separate.
- **[Big Bang-Big Crunch](../algorithms/big-bang-big-crunch-erol-eksin-2006.md)** — `big-bang-big-crunch-erol-eksin-2006` — Bounded continuous derivative-free optimization using alternating random expansion and a shrinking Big-Crunch representative.
- **[Teaching-Learning-Based Optimization](../algorithms/teaching-learning-based-optimization-rao-savsani-vakharia-2011.md)** — `teaching-learning-based-optimization-rao-savsani-vakharia-2011` — Bounded continuous derivative-free optimization using teacher-phase mean displacement and learner-to-learner interaction without algorithm-specific tuning parameters.
- **[Jaya Algorithm](../algorithms/jaya-algorithm-rao-2016.md)** — `jaya-algorithm-rao-2016` — Bounded continuous derivative-free population optimization that moves every variable toward the current best and away from the current worst without algorithm-specific control parameters.
- **[Imperialist Competitive Algorithm](../algorithms/imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007.md)** — `imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007` — Bounded continuous derivative-free optimization using countries, empires, assimilation, revolution and imperialistic competition.
- **[Black Hole Algorithm](../algorithms/black-hole-algorithm-hatamlou-2013.md)** — `black-hole-algorithm-hatamlou-2013` — Positive-cost bounded continuous minimization using attraction to the current black hole and event-horizon replacement.
- **[Multi-Verse Optimizer](../algorithms/multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016.md)** — `multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016` — Non-negative-cost bounded continuous minimization using white-hole exchange and best-universe wormholes.
- **[Equilibrium Optimizer](../algorithms/equilibrium-optimizer-faramarzi-heidarinejad-stephens-mirjalili-2020.md)** — `equilibrium-optimizer-faramarzi-heidarinejad-stephens-mirjalili-2020` — Bounded continuous derivative-free optimization using an equilibrium pool, exponential turnover and generation-rate control.

## Navigation

Return to @ref method_families "method families".

- @subpage differential_harmony_search_chakraborty_roy_das_jain_abraham_2009 - Differential Harmony Search with DE/rand/1-style mutation replacing classical pitch adjustment.

- @subpage exploratory_harmony_search_das_mukhopadhyay_roy_abraham_panigrahi_2011 - Exploratory Harmony Search with Harmony-Memory standard-deviation fine-tuning width.

- @subpage improved_harmony_search_differential_mutation_yong_liu_zhang_feng_2012 - IHSDE with differential mutation and F sampled uniformly from [0.6,1].

- @subpage novel_self_adaptive_harmony_search_luo_2013 - Novel Self-Adaptive Harmony Search with dimension-derived HMCR and fitness-dispersion control.

- @subpage adaptive_harmony_search_differential_evolution_zhao_li_hao_liu_yuan_2020 - aHSDE with differential mutation, periodic parameter learning and linear HMS reduction.
