@page family_constructive_methods Constructive methods

# Constructive methods

Constructive metaheuristics build solutions progressively from problem-defined components instead of starting exclusively from a complete incumbent.

## Public algorithms

- @subpage grasp_feo_resende_1995
- @subpage reactive_grasp_prais_ribeiro_2000 - Reactive GRASP with Prais-Ribeiro alpha adaptation.
- @subpage grasp_path_relinking - GRASP with quality/diversity elite memory and greedy forward path relinking.

## Platform contract

Constructive methods use the same `OptimizationContext` lifecycle, callbacks, stopping criteria, deterministic random source ownership and stable catalog identity as the other public families.

For GRASP, the domain owns candidate generation and the greedy score; the platform owns canonical RCL semantics, randomized selection, objective accounting, best-so-far management and local-search composition.
