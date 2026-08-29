@page improved_feasibility_pump_achterberg_berthold_2007 Improved Feasibility Pump

# Improved Feasibility Pump

## General description

Improved Feasibility Pump (`ImprovedFeasibilityPump`) is the public scientific identity associated with
Achterberg & Berthold (2007), *Improving the feasibility pump*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The objective-aware feasibility-pump projection is preserved; weight scheduling and solver callbacks are exposed as platform parameters.

## Technical specifications

- Stable ID: `improved-feasibility-pump-achterberg-berthold-2007`
- Class: `ImprovedFeasibilityPumpOptimizer`
- Parameters: `ImprovedFeasibilityPumpParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.158.0
- Primary DOI/permanent identifier: `10.1016/j.disopt.2006.10.004`

## Complexity

One weighted relaxation projection per iteration plus O(n) rounding.

Space: O(n) current target plus solver state.

## Applicability

MIP feasibility search where objective quality matters in addition to attaining integrality.

## Detailed operation

Biases the pump projection with the original objective while retaining the distance-to-rounded-target term to improve solution quality.

## Parameters

`ImprovedFeasibilityPumpParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new ImprovedFeasibilityPumpOptimizer().Optimize(
        domain,
        new ImprovedFeasibilityPumpParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`improved-feasibility-pump-achterberg-berthold-2007`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:Ax\ge b,\ x_I\in\mathbb Z\}
\f]

### Update equations / iterations

\f[
\begin{aligned}\tilde x^k&=\operatorname{round}(x^k),\\x^{k+1}&\in\arg\min_{x\in P}\bigl((1-\alpha_k)\Delta(x,\tilde x^k)+\alpha_k c^\top x\bigr).\end{aligned}
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

Achterberg & Berthold (2007), *Improving the feasibility pump*, Discrete Optimization 4(1), 77-86.
DOI/permanent identifier: `10.1016/j.disopt.2006.10.004`.
