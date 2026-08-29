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
- **[General-MIP Feasibility Pump](../algorithms/general-mip-feasibility-pump-bertacco-fischetti-lodi-2007.md)** - `general-mip-feasibility-pump-bertacco-fischetti-lodi-2007` - Extends the pump to general-integer variables and uses structured perturbation plus an exact finishing neighborhood when direct pumping stalls.
- **[Improved Feasibility Pump](../algorithms/improved-feasibility-pump-achterberg-berthold-2007.md)** - `improved-feasibility-pump-achterberg-berthold-2007` - Biases the pump projection with the original objective while retaining the distance-to-rounded-target term to improve solution quality.
- **[Distance Induced Neighborhood Search](../algorithms/dins-ghosh-2007.md)** - `dins-ghosh-2007` - Builds an exact neighborhood whose distance to the relaxation is bounded by the incumbent-to-relaxation distance, with hard fixings for strong agreements.
- **[Kernel Search](../algorithms/kernel-search-angelelli-mansini-speranza-2010.md)** - `kernel-search-angelelli-mansini-speranza-2010` - Ranks binary variables from relaxation information, solves exact subproblems on a kernel plus one bucket, and promotes useful bucket variables into the kernel.
- **[MIP-based Adaptive Large Neighborhood Search](../algorithms/mip-alns-muller-spoorendonk-pisinger-2012.md)** - `mip-alns-muller-spoorendonk-pisinger-2012` - Destroys a variable subset, fixes the complement to the incumbent and invokes the exact solver as a large-neighborhood repair operator with adaptive destroy size.
- **[Relaxation Enforced Neighborhood Search](../algorithms/rens-berthold-2014.md)** - `rens-berthold-2014` - Fixes relaxation-integral integer variables and bounds fractional integer variables to floor/ceiling values before solving the exact rounding subproblem.
- **[Proximity Search](../algorithms/proximity-search-fischetti-monaci-2014.md)** - `proximity-search-fischetti-monaci-2014` - Replaces the subproblem objective by distance to the incumbent while imposing an original-objective cutoff that forces improvement.

## Navigation

Return to @ref method_families "method families".
