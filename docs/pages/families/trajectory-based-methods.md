@page family_trajectory_based_methods Trajectory-based methods

# Trajectory-based methods

Single-solution methods that explore a neighborhood trajectory through explicit acceptance,
memory and state-transition policies.

## Methods

- **[Simulated Annealing](../algorithms/simulated-annealing-metropolis.md)** —
  `simulated-annealing-metropolis` — O(C_move + C_eval) per attempted transition; O(C_delta)
  when an exact differential evaluator is available.
- **[Threshold Accepting](../algorithms/threshold-accepting-dueck-scheuer-1990.md)** —
  `threshold-accepting-dueck-scheuer-1990` — deterministic threshold acceptance with
  exact-delta/reversible trajectory execution.
- **[Tabu Search](../algorithms/tabu-search-glover.md)** — `tabu-search-glover` —
  best-admissible neighborhood search with short-term memory and aspiration.
- **[Reactive Tabu Search](../algorithms/reactive-tabu-search-battiti-tecchiolli-1994.md)** —
  `reactive-tabu-search-battiti-tecchiolli-1994` — repetition-aware adaptive tenure and
  reactive diversification.

## Scientific components

- @ref simulated_annealing_cooling_schedules "Simulated Annealing Scientific Cooling Catalog".
- @ref threshold_accepting_schedules "Threshold Accepting Schedule Catalog".
- @ref tabu_search_memory_control_strategies "Tabu Search Memory and Reactive Control Catalog".

## v0.23.0 Local Search Core

- `local-search-best-improvement` - Local Search, best improvement.
- `local-search-first-improvement` - Local Search, first improvement.
## Navigation

Return to @ref method_families "method families".
