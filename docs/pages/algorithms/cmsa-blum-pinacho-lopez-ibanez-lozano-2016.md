@page cmsa_blum_pinacho_lopez_ibanez_lozano_2016 Construct, Merge, Solve & Adapt

# Construct, Merge, Solve & Adapt

## General description

Construct, Merge, Solve & Adapt (`CmsaMatheuristic`) is the public scientific identity associated with
Blum, Pinacho, Lopez-Ibanez & Lozano (2016), *Construct, Merge, Solve & Adapt A new general algorithm for combinatorial optimization*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The Construct-Merge-Solve-Adapt cycle and component aging are preserved; solution construction is supplied by the generic domain.

## Technical specifications

- Stable ID: `cmsa-blum-pinacho-lopez-ibanez-lozano-2016`
- Class: `CmsaMatheuristicOptimizer`
- Parameters: `CmsaMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.164.0
- Primary DOI/permanent identifier: `10.1016/j.cor.2015.10.014`

## Complexity

Several constructive samples plus one exact reduced-instance solve per CMSA iteration.

Space: O(n) component set/ages plus exact-solver state.

## Applicability

Component-based combinatorial optimization where candidate solutions identify active binary components and a reduced instance can be solved exactly.

## Detailed operation

Constructs candidate solutions, merges their active components into a reduced exact subproblem, solves it, then ages and removes stale components.

## Parameters

`CmsaMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new CmsaMatheuristicOptimizer().Optimize(
        domain,
        new CmsaMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`cmsa-blum-pinacho-lopez-ibanez-lozano-2016`

## Mathematical details

### Problem formulation

\f[
\min\{f(x):x\in X,\ C(x)\subseteq\mathcal C\}
\f]

### Update equations / iterations

\f[
\begin{aligned}C_t&\leftarrow C_t\cup\bigcup_{r=1}^{n_a}C(x_t^{(r)}),\\x_t^*&\in\arg\min\{f(x):x\in X(C_t)\},\\C_{t+1}&=\operatorname{adapt}(C_t,x_t^*).\end{aligned}
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

Blum, Pinacho, Lopez-Ibanez & Lozano (2016), *Construct, Merge, Solve & Adapt A new general algorithm for combinatorial optimization*, Computers & Operations Research 68, 75-88.
DOI/permanent identifier: `10.1016/j.cor.2015.10.014`.
