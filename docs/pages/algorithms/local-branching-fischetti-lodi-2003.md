@page local_branching_fischetti_lodi_2003 Local Branching

# Local Branching

## General description

Local Branching (`LocalBranchingMatheuristic`) is the public scientific identity associated with
Fischetti & Lodi (2003), *Local branching*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The published Hamming-neighborhood mechanism is preserved; solver callbacks, generic variable metadata, seed/cancellation plumbing and benchmark harness are platform adaptations.

## Technical specifications

- Stable ID: `local-branching-fischetti-lodi-2003`
- Class: `LocalBranchingMatheuristicOptimizer`
- Parameters: `LocalBranchingMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.154.0
- Primary DOI/permanent identifier: `10.1007/s10107-003-0395-5`

## Complexity

Up to one exact restricted solve per local-branching iteration plus O(n) neighborhood construction.

Space: O(n) incumbent/reference state plus exact-solver state.

## Applicability

Generic binary or mixed-integer improvement when an incumbent and exact restricted solve are available.

## Detailed operation

Adds a Hamming-distance local-branching constraint around the incumbent and delegates the resulting neighborhood to the exact subsolver.

## Parameters

`LocalBranchingMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new LocalBranchingMatheuristicOptimizer().Optimize(
        domain,
        new LocalBranchingMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`local-branching-fischetti-lodi-2003`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X\cap\mathbb Z^p\}
\f]

### Update equations / iterations

\f[
\begin{aligned}\Delta_B(x,\bar x)&=\sum_{j\in S}(1-x_j)+\sum_{j\in B\setminus S}x_j,\\N_k(\bar x)&=\{x\in X:\Delta_B(x,\bar x)\le k\}.\end{aligned}
\f]

### Assumptions

The domain exposes finite objective values, variable-kind metadata, integer-feasibility testing,
a feasible incumbent constructor, and deterministic exact/relaxation solve callbacks for a fixed
platform seed and solver configuration.

### Convergence conditions

No universal finite-time global-convergence claim is asserted for the matheuristic wrapper.
Whenever an exact restricted subproblem is solved to optimality by the supplied domain, the
returned point is exact only for that restricted subproblem; global optimality is not implied.

### Scientific references

Fischetti & Lodi (2003), *Local branching*, Mathematical Programming 98(1-3), 23-47.
DOI/permanent identifier: `10.1007/s10107-003-0395-5`.
