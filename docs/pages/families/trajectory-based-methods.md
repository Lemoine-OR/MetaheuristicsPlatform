@page family_trajectory_based_methods Trajectory-based methods

# Trajectory-based methods

Single-solution methods that explore a neighborhood trajectory through explicit acceptance,
memory and state-transition policies.

## Methods

- **[Simulated Annealing](../algorithms/simulated-annealing-metropolis.md)** —
  `simulated-annealing-metropolis` — O(C_move + C_eval) per attempted transition; O(C_delta)
  when an exact differential evaluator is available.
- **[Tabu Search](../algorithms/tabu-search-glover.md)** — `tabu-search-glover` —
  best-admissible neighborhood search with short-term memory and aspiration.
- **[Reactive Tabu Search](../algorithms/reactive-tabu-search-battiti-tecchiolli-1994.md)** —
  `reactive-tabu-search-battiti-tecchiolli-1994` — repetition-aware adaptive tenure and
  reactive diversification.

## Scientific components

- @ref simulated_annealing_cooling_schedules "Simulated Annealing Scientific Cooling Catalog".
- @ref tabu_search_memory_control_strategies "Tabu Search Memory and Reactive Control Catalog".

## v0.23.0 Local Search Core

- `local-search-best-improvement` - Local Search, best improvement.
- `local-search-first-improvement` - Local Search, first improvement.
## Navigation

Return to @ref method_families "method families".
