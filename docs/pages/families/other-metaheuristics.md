@page family_other_metaheuristics Other / music-inspired methods

# Other / music-inspired methods

Population-based or memory-driven metaheuristics whose scientific identity is not accurately
represented as swarm intelligence, evolutionary search, trajectory search, constructive
search or hybrid/memetic composition.

## Methods

- @subpage harmony_search_geem_kim_loganathan_2001 - canonical fixed-parameter Harmony Search foundation.
- @subpage improved_harmony_search_mahdavi_fesanghary_damangir_2007 - Improved Harmony Search with deterministic PAR/bw schedules.
- @subpage global_best_harmony_search_omran_mahdavi_2008 - Global-best Harmony Search with bandwidth-free global-best pitch adjustment.

## Classification note

Harmony Search, Improved Harmony Search and Global-best Harmony Search are represented by
`MetaheuristicFamily.Other` and `MetaheuristicSolutionModel.Population`. GHS explicitly borrows
a global-best influence concept from swarm intelligence, but the family keeps the Harmony
Search lineage together without silently reclassifying the 2001 or 2007 identities. Stable IDs
keep fixed-parameter HS, scheduled-parameter IHS and bandwidth-free global-best GHS distinct.

## Navigation

Return to @ref method_families "method families".