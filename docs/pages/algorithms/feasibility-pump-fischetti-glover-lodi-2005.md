@page feasibility_pump_fischetti_glover_lodi_2005 Feasibility Pump

# Feasibility Pump

## General description

Feasibility Pump (`FeasibilityPumpMatheuristic`) is the public scientific identity associated with
Fischetti, Glover & Lodi (2005), *The feasibility pump*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The rounding/projection pump is preserved; generic domain callbacks replace the paper's concrete LP/MIP implementation.

## Technical specifications

- Stable ID: `feasibility-pump-fischetti-glover-lodi-2005`
- Class: `FeasibilityPumpMatheuristicOptimizer`
- Parameters: `FeasibilityPumpMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.156.0
- Primary DOI/permanent identifier: `10.1007/s10107-004-0570-3`

## Complexity

One relaxation projection per pump iteration plus O(n) rounding and cycle handling.

Space: O(n) fractional/rounded targets plus relaxation-solver state.

## Applicability

Generic MIP feasibility search when the domain can solve relaxation projections and test integer feasibility.

## Detailed operation

Alternates integer rounding with relaxation solves that minimize distance to the current integer target, with deterministic cycle perturbation.

## Parameters

`FeasibilityPumpMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new FeasibilityPumpMatheuristicOptimizer().Optimize(
        domain,
        new FeasibilityPumpMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`feasibility-pump-fischetti-glover-lodi-2005`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:Ax\ge b,\ x_I\in\mathbb Z\}
\f]

### Update equations / iterations

\f[
\begin{aligned}\tilde x^k&=\operatorname{round}(x^k),\\x^{k+1}&\in\arg\min_{x\in P}\Delta(x,\tilde x^k).\end{aligned}
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

Fischetti, Glover & Lodi (2005), *The feasibility pump*, Mathematical Programming 104(1), 91-104.
DOI/permanent identifier: `10.1007/s10107-004-0570-3`.
