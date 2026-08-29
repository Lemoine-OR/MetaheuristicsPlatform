@page rins_danna_rothberg_le_pape_2005 Relaxation Induced Neighborhood Search

# Relaxation Induced Neighborhood Search

## General description

Relaxation Induced Neighborhood Search (`RinsMatheuristic`) is the public scientific identity associated with
Danna, Rothberg & Le Pape (2005), *Exploring relaxation induced neighborhoods to improve MIP solutions*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The published relaxation/incumbent agreement fixing rule is preserved; the solver interface and resource limits are platform adaptations.

## Technical specifications

- Stable ID: `rins-danna-rothberg-le-pape-2005`
- Class: `RinsMatheuristicOptimizer`
- Parameters: `RinsMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.155.0
- Primary DOI/permanent identifier: `10.1007/s10107-004-0518-7`

## Complexity

One relaxation solve and up to one restricted exact solve per RINS iteration.

Space: O(n) relaxation/incumbent vectors and fixing map plus exact-solver state.

## Applicability

Generic MIP improvement with an incumbent, a relaxation solution and an exact subsolver.

## Detailed operation

Fixes integer variables on which the incumbent and relaxation agree, then solves the induced exact neighborhood.

## Parameters

`RinsMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new RinsMatheuristicOptimizer().Optimize(
        domain,
        new RinsMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`rins-danna-rothberg-le-pape-2005`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X\cap\mathbb Z^p\}
\f]

### Update equations / iterations

\f[
\begin{aligned}F&=\{j\in I:\bar x_j=\tilde x_j\},\\x_j&=\bar x_j\quad(j\in F).\end{aligned}
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

Danna, Rothberg & Le Pape (2005), *Exploring relaxation induced neighborhoods to improve MIP solutions*, Mathematical Programming 102(1), 71-90.
DOI/permanent identifier: `10.1007/s10107-004-0518-7`.
