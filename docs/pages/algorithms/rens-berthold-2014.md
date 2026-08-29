@page rens_berthold_2014 Relaxation Enforced Neighborhood Search

# Relaxation Enforced Neighborhood Search

## General description

Relaxation Enforced Neighborhood Search (`RensMatheuristic`) is the public scientific identity associated with
Berthold (2014), *RENS: The optimal rounding*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The optimal-rounding neighborhood is preserved; the platform abstracts MILP/MIQCP/MINLP backend details behind exact-repair callbacks.

## Technical specifications

- Stable ID: `rens-berthold-2014`
- Class: `RensMatheuristicOptimizer`
- Parameters: `RensMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.162.0
- Primary DOI/permanent identifier: `10.1007/s12532-013-0060-9`

## Complexity

One relaxation solve followed by one exact restricted rounding solve.

Space: O(n) relaxation-derived fixing/bound state plus solver state.

## Applicability

MIP/MINLP-style domains exposing a relaxation and exact restricted solve.

## Detailed operation

Fixes relaxation-integral integer variables and bounds fractional integer variables to floor/ceiling values before solving the exact rounding subproblem.

## Parameters

`RensMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new RensMatheuristicOptimizer().Optimize(
        domain,
        new RensMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`rens-berthold-2014`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X\cap\mathbb Z^p\}
\f]

### Update equations / iterations

\f[
\begin{aligned}x_j&=\tilde x_j&&(\tilde x_j\in\mathbb Z),\\
\lfloor\tilde x_j\rfloor\le x_j&\le\lceil\tilde x_j\rceil&&(\tilde x_j\notin\mathbb Z).\end{aligned}
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

Berthold (2014), *RENS: The optimal rounding*, Mathematical Programming Computation 6(1), 33-54.
DOI/permanent identifier: `10.1007/s12532-013-0060-9`.
