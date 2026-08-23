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

## Classification note

Harmony Search, Improved Harmony Search, Global-best Harmony Search and Self-Adaptive
Global-best Harmony Search are represented by `MetaheuristicFamily.Other` and
`MetaheuristicSolutionModel.Population`. GHS borrows global-best influence from swarm
intelligence, while SGHS adds successful-parameter learning and corresponding-coordinate
best-harmony exploitation. Stable IDs preserve HS 2001, IHS 2007, GHS 2008 and SGHS 2010
as four distinct scientific identities.

## Navigation

Return to @ref method_families "method families".