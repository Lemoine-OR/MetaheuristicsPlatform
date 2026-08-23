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

## Classification note

Harmony Search, Improved Harmony Search, Global-best Harmony Search, Self-Adaptive
Global-best Harmony Search and Novel Global Harmony Search are represented by
`MetaheuristicFamily.Other` and `MetaheuristicSolutionModel.Population`. GHS borrows
global-best influence from swarm intelligence, SGHS adds successful-parameter learning, and
NGHS combines a PSO-inspired best/worst position update with GA-style mutation and
unconditional Harmony Memory replacement. Stable IDs preserve all five literature identities.

## Navigation

Return to @ref method_families "method families".