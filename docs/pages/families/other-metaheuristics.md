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

## Navigation

Return to @ref method_families "method families".

- @subpage differential_harmony_search_chakraborty_roy_das_jain_abraham_2009 - Differential Harmony Search with DE/rand/1-style mutation replacing classical pitch adjustment.

- @subpage exploratory_harmony_search_das_mukhopadhyay_roy_abraham_panigrahi_2011 - Exploratory Harmony Search with Harmony-Memory standard-deviation fine-tuning width.

- @subpage improved_harmony_search_differential_mutation_yong_liu_zhang_feng_2012 - IHSDE with differential mutation and F sampled uniformly from [0.6,1].
