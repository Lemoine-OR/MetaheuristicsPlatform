@page family_hyper_heuristics Hyper-heuristics and algorithm selection

# Hyper-heuristics and algorithm selection

Hyper-heuristics operate above problem-specific search operators. They select, sequence, adapt
or generate lower-level heuristics through a domain interface rather than hard-coding one
problem-specific neighborhood.

## Scientific scope

The family contains selection hyper-heuristics, adaptive operator selection, heuristic-memory
policies, bandit/credit mechanisms and move-acceptance combinations.

## Platform contract

- `IHyperHeuristicDomain` provides the objective, initial solution, features and low-level pool.
- `ILowLevelHeuristic` applies one domain-level search operation.
- `HyperHeuristicOptimizationResult` reports the best solution and the selected-heuristic trace.
- Every scientific page separates the paper's mechanism from platform adaptation.

## Methods
- **[Tabu-Search Hyper-Heuristic](../algorithms/tabu-search-hyperheuristic-burke-kendall-soubeiga-2003.md)** - `tabu-search-hyperheuristic-burke-kendall-soubeiga-2003` - Heuristic-level reinforcement scores compete under a tabu list; non-improving low-level heuristics become temporarily unavailable.
- **[Case-Based Heuristic Selection](../algorithms/case-based-heuristic-selection-burke-petrovic-qu-2006.md)** - `case-based-heuristic-selection-burke-petrovic-qu-2006` - A feature description of the current search state retrieves the most similar stored case and reuses its associated low-level heuristic.

## Navigation

Return to @ref method_families "method families".
