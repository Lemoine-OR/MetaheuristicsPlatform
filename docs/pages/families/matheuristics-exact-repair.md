@page family_matheuristics_exact_repair Matheuristics and exact-repair integration

# Matheuristics and exact-repair integration

Matheuristics combine heuristic search with mathematical-programming information or restricted
exact solves. This family is intentionally solver-agnostic: algorithms construct scientific
neighborhood/projection requests while the application supplies the exact/relaxation backend.

## Scientific scope

The family covers local-branching neighborhoods, relaxation-induced exact neighborhoods,
feasibility-pump projections, kernel/bucket decompositions, exact repair in large-neighborhood
search, proximity objectives and constructive reduced-instance solve/adapt frameworks.

## Platform contract

- `IExactRepairMatheuristicDomain` owns the mathematical model and solver integration.
- `ExactRepairRequest` carries fixings, bounds, active-component restrictions, distance targets,
  objective cutoffs and resource limits.
- `MatheuristicSolveResult` separates infeasible/unsolved restricted subproblems from valid points.
- `MatheuristicOptimizationResult` records the incumbent and exact/relaxation solve counts.
- Every algorithm page separates literature mechanism from platform adaptation.

## Methods
- **[Local Branching](../algorithms/local-branching-fischetti-lodi-2003.md)** - `local-branching-fischetti-lodi-2003` - Adds a Hamming-distance local-branching constraint around the incumbent and delegates the resulting neighborhood to the exact subsolver.
- **[Relaxation Induced Neighborhood Search](../algorithms/rins-danna-rothberg-le-pape-2005.md)** - `rins-danna-rothberg-le-pape-2005` - Fixes integer variables on which the incumbent and relaxation agree, then solves the induced exact neighborhood.
- **[Feasibility Pump](../algorithms/feasibility-pump-fischetti-glover-lodi-2005.md)** - `feasibility-pump-fischetti-glover-lodi-2005` - Alternates integer rounding with relaxation solves that minimize distance to the current integer target, with deterministic cycle perturbation.

## Navigation

Return to @ref method_families "method families".
