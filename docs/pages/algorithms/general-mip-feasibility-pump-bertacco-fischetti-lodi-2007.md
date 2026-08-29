@page general_mip_feasibility_pump_bertacco_fischetti_lodi_2007 General-MIP Feasibility Pump

# General-MIP Feasibility Pump

## General description

General-MIP Feasibility Pump (`GeneralMipFeasibilityPump`) is the public scientific identity associated with
Bertacco, Fischetti & Lodi (2007), *A feasibility pump heuristic for general mixed-integer problems*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The general-integer pumping and post-pump enumeration intent are preserved; platform exact repair abstracts the enumeration backend.

## Technical specifications

- Stable ID: `general-mip-feasibility-pump-bertacco-fischetti-lodi-2007`
- Class: `GeneralMipFeasibilityPumpOptimizer`
- Parameters: `GeneralMipFeasibilityPumpParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.157.0
- Primary DOI/permanent identifier: `10.1016/j.disopt.2006.10.001`

## Complexity

Relaxation projections per pump iteration plus one bounded exact finishing solve when needed.

Space: O(n) target/history state plus solver state.

## Applicability

Mixed binary/general-integer feasibility search with exact or relaxation repair callbacks.

## Detailed operation

Extends the pump to general-integer variables and uses structured perturbation plus an exact finishing neighborhood when direct pumping stalls.

## Parameters

`GeneralMipFeasibilityPumpParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new GeneralMipFeasibilityPumpOptimizer().Optimize(
        domain,
        new GeneralMipFeasibilityPumpParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`general-mip-feasibility-pump-bertacco-fischetti-lodi-2007`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:Ax\ge b,\ x_I\in\mathbb Z\}
\f]

### Update equations / iterations

\f[
\begin{aligned}\tilde x^k&=\Pi_{\mathbb Z}(x^k),\\x^{k+1}&\in\arg\min_{x\in P}\Delta(x,\tilde x^k),\\\tilde x^k&\leftarrow\operatorname{perturb}(\tilde x^k)\ \text{on cycles}.\end{aligned}
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

Bertacco, Fischetti & Lodi (2007), *A feasibility pump heuristic for general mixed-integer problems*, Discrete Optimization 4(1), 63-76.
DOI/permanent identifier: `10.1016/j.disopt.2006.10.001`.
